using System;
using System.Collections;
using UnityEngine;
using GlobalEnums;

[Serializable]
public class AimedBurstNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Attack;

    [Header("Burst Settings")]
    public GameObject ProjectilePrefab;
    public int BulletsPerBurst = 5;     // Số lượng đạn mỗi loạt
    public float TimeBetweenBullets = 0.1f; // Delay giữa mỗi viên (Staccato)
    public float ProjectileSpeed = 10f;

    [Header("Timing")]
    public float WindupTime = 0.5f;     // Thời gian gồng trước khi bắn
    public float CooldownAfterBurst = 1.0f; // Thời gian nghỉ sau khi bắn xong loạt đạn
    public Vector2 SpawnOffset = new Vector2(0.5f, 0.5f);

    private float _timer;
    private int _bulletsFired;
    private bool _isBursting;
    private Vector2 _currentDir;

    public override void Enter()
    {
        IsFinished = false;
        _isBursting = false;
        _bulletsFired = 0;
        _timer = WindupTime;
        
        machine.Movement.SetMoveDirection(Vector2.zero);
    }

    public override void ExecuteLogic()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            // Nếu chưa bắt đầu bắn hoặc đang trong quá trình bắn loạt
            if (_bulletsFired < BulletsPerBurst)
            {
                ShootSingleBullet();
                _bulletsFired++;
                _timer = TimeBetweenBullets; // Nghỉ một chút rồi bắn viên tiếp theo
                _isBursting = true;
            }
            // Sau khi bắn đủ số viên, đợi hết Cooldown mới kết thúc
            else if (_isBursting)
            {
                _isBursting = false;
                _timer = CooldownAfterBurst;
            }
            else
            {
                IsFinished = true;
            }
        }
    }

    private void ShootSingleBullet()
    {
        if (ProjectilePrefab == null || machine.Target == null) return;

        // Tính toán hướng bắn ngay tại thời điểm bắn viên đạn đó (Đuổi gắt)
        // Hoặc bạn có thể tính hướng 1 lần duy nhất ở đầu loạt nếu muốn bắn theo đường thẳng cố định
        float facingDir = machine.CachedTransform.localScale.x > 0 ? 1f : -1f;
        Vector3 spawnPos = machine.CachedTransform.position + new Vector3(SpawnOffset.x * facingDir, SpawnOffset.y, 0);
        
        Vector2 targetDir = ((Vector2)machine.Target.position - (Vector2)spawnPos).normalized;

        // Quay mặt về phía người chơi khi bắn
        float lookDir = (machine.Target.position.x > machine.CachedTransform.position.x) ? 1f : -1f;
        machine.CachedTransform.localScale = new Vector3(lookDir, 1, 1);

        SpawnProjectile(spawnPos, targetDir);
    }

    private void SpawnProjectile(Vector3 pos, Vector2 dir)
    {
        GameObject obj = BulletPool.Instance.Get(ProjectilePrefab, pos, Quaternion.identity);
        var projectile = obj.GetComponent<EnemyProjectile>();
        
        // Sử dụng lực đẩy thay vì gán velocity trực tiếp (theo yêu cầu add force của bạn)
        projectile?.Initialize(dir); 
    }

    public override void Exit() { }
}
