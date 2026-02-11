using System;
using UnityEngine;
using GlobalEnums;

[Serializable]
public class FanAttackNode : EnemyStateNode
{
    public enum ShootDirection { Right, Left, Up, Down }

    public override EnemyStateType StateType => EnemyStateType.Attack;

    [Header("Fan Settings")]
    public GameObject ProjectilePrefab;
    public ShootDirection BaseDirection = ShootDirection.Right;
    public int ProjectileCount = 3;
    [Range(0, 360)] public float SectorAngle = 60f;

    [Header("Timing")]
    public float WindupTime = 0.5f;
    public float RecoverTime = 0.8f;
    public Vector2 SpawnOffset = new Vector2(0.5f, 0.5f);

    private float _timer;
    private bool _hasThrown;

    public override void Enter()
    {
        IsFinished = false;
        _hasThrown = false;
        _timer = WindupTime;
        machine.Movement.SetMoveDirection(Vector2.zero);
        
        // Tự động quay mặt nếu bắn ngang
        if (BaseDirection == ShootDirection.Left || BaseDirection == ShootDirection.Right)
            FaceDirection();
    }

    private void FaceDirection()
    {
        float scaleX = (BaseDirection == ShootDirection.Right) ? 1f : -1f;
        machine.CachedTransform.localScale = new Vector3(scaleX, 1, 1);
    }

    public override void ExecuteLogic()
    {
        _timer -= Time.deltaTime;

        if (!_hasThrown && _timer <= 0f)
        {
            PerformFanShoot();
            _hasThrown = true;
            _timer = RecoverTime;
        }
        else if (_hasThrown && _timer <= 0f)
        {
            IsFinished = true;
        }
    }

    private void PerformFanShoot()
    {
        if (ProjectilePrefab == null || ProjectileCount <= 0) return;

        Vector3 spawnPos = machine.CachedTransform.position + (Vector3)SpawnOffset;
        
        // Xác định góc trung tâm dựa trên hướng chọn
        float centerAngle = GetBaseAngle(BaseDirection);
        
        // Tính góc bắt đầu (bên trái cùng của rẻ quạt)
        float startAngle = centerAngle - (SectorAngle / 2f);
        
        // Tính bước nhảy góc giữa mỗi viên đạn
        float angleStep = (ProjectileCount > 1) ? SectorAngle / (ProjectileCount - 1) : 0;

        for (int i = 0; i < ProjectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 dir = DegreeToVector2(currentAngle);
            SpawnProjectile(spawnPos, dir);
        }
    }

    private float GetBaseAngle(ShootDirection dir)
    {
        return dir switch
        {
            ShootDirection.Right => 0f,
            ShootDirection.Up    => 90f,
            ShootDirection.Left  => 180f,
            ShootDirection.Down  => 270f,
            _ => 0f
        };
    }

    private Vector2 DegreeToVector2(float degree)
    {
        float radian = degree * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    private void SpawnProjectile(Vector3 pos, Vector2 dir)
    {
        GameObject obj = BulletPool.Instance.Get(ProjectilePrefab, pos, Quaternion.identity);
        var projectile = obj.GetComponent<EnemyProjectile>();
        
        // Projectile của bạn nên dùng rb.AddForce(dir * speed) hoặc tương tự trong Initialize
        projectile?.Initialize(dir);
    }

    public override void Exit() { }
}
