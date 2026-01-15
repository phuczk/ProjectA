using UnityEngine;
using System;

public interface IStateDecision
{
    bool Decide(EnemyUniversalMachine machine);
}

/// <summary>
/// Dùng để chuyển sang trạng thái Tấn công khi Player đi vào vùng Attack Range.
/// </summary>
[Serializable]
public class DecisionPlayerInAttackRange : IStateDecision
{
    public bool Decide(EnemyUniversalMachine machine) => machine.IsPlayerInAttackRange();
}

/// <summary>
/// Dùng cho các trạng thái cần đợi một khoảng thời gian cố định độc lập với logic của Node.
/// Lưu ý: Cần reset _timer khi bắt đầu sử dụng.
/// </summary>
[Serializable]
public class DecisionTimeElapsed : IStateDecision
{
    public float WaitTime;
    private float _timer;
    
    public bool Decide(EnemyUniversalMachine machine)
    {
        _timer -= Time.deltaTime;
        return _timer <= 0;
    }
}

/// <summary>
/// Dùng để tạo tính ngẫu nhiên cho hành vi AI (ví dụ: 30% cơ hội chuyển sang Idle thay vì Patrol).
/// </summary>
[Serializable]
public class DecisionRandomChance : IStateDecision
{
    [Range(0, 100)] public float Chance = 50f;
    public bool Decide(EnemyUniversalMachine machine) => UnityEngine.Random.Range(0f, 100f) <= Chance;
}

/// <summary>
/// Dùng để phát hiện tường hoặc vật cản phía trước mặt để quái vật quay đầu.
/// </summary>
[Serializable]
public class DecisionObstacleAhead : IStateDecision
{
    public float CheckDistance = 1f;
    public LayerMask ObstacleLayer;
    public bool Decide(EnemyUniversalMachine machine)
    {
        float dir = machine.transform.localScale.x > 0 ? 1 : -1;
        RaycastHit2D hit = Physics2D.Raycast(machine.transform.position, Vector2.right * dir, CheckDistance, ObstacleLayer);
        return hit.collider != null;
    }
}

/// <summary>
/// CỰC KỲ QUAN TRỌNG: Dùng để chuyển trạng thái khi Node hiện tại báo đã xong (IsFinished = true).
/// Thường dùng để thoát khỏi Idle hoặc Patrol khi hết thời gian ngẫu nhiên đã định sẵn.
/// </summary>
[Serializable]
public class DecisionNodeTimerFinished : IStateDecision
{
    public bool Decide(EnemyUniversalMachine machine) 
    {
        return machine.IsCurrentNodeFinished();
    }
}

/// <summary>
/// Dùng để phát hiện Player lọt vào tầm nhìn để bắt đầu đuổi theo (Chase).
/// </summary>
[Serializable]
public class DecisionPlayerInChaseRange : IStateDecision
{
    public bool Decide(EnemyUniversalMachine machine) => machine.IsPlayerInChaseRange();
}

/// <summary>
/// Dùng trong ChaseNode để kiểm tra xem đã mất dấu Player (ra khỏi vùng Chase) để quay lại tuần tra.
/// </summary>
[Serializable]
public class DecisionLostPlayerTimeout : IStateDecision
{
    public bool Decide(EnemyUniversalMachine machine)
    {
        return !machine.IsPlayerInChaseRange();
    }
}

[Serializable]
public class DecisionCanSeePlayer : IStateDecision
{
    public LayerMask ObstacleLayer;
    public float ViewAngle = 90f;

    public bool Decide(EnemyUniversalMachine machine)
    {
        if (!machine.IsPlayerInChaseRange()) return false;

        Vector2 dirToPlayer = (machine.Target.position - machine.transform.position).normalized;
        float dot = Vector2.Dot(machine.transform.localScale.x > 0 ? Vector2.right : Vector2.left, dirToPlayer);

        if (dot < Mathf.Cos(ViewAngle * 0.5f * Mathf.Deg2Rad)) return false;

        float dist = Vector2.Distance(machine.transform.position, machine.Target.position);
        RaycastHit2D hit = Physics2D.Raycast(machine.transform.position, dirToPlayer, dist, ObstacleLayer);

        return hit.collider == null; 
    }
}

[Serializable]
public class DecisionIsCooldownFinished : IStateDecision
{
    public string ActionName;
    public bool Decide(EnemyUniversalMachine machine) => machine.IsCooldownFinished(ActionName);
}