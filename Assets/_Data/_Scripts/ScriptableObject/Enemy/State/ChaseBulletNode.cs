using System;
using UnityEngine;
using GlobalEnums;

[Serializable]
public class ChaseBulletNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Attack;

    [Header("Chase Settings")]
    public GameObject ChasingProjectilePrefab;
    public int BulletCount = 3;
    public float TimeBetweenBullets = 0.3f;
    public float SpreadAngle = 30f;

    [Header("Timing")]
    public float WindupTime = 0.5f;
    public float Cooldown = 1.0f;

    private float _timer;
    private int _firedCount;
    private bool _isFiring;

    public override void Enter()
    {
        IsFinished = false;
        _firedCount = 0;
        _timer = WindupTime;
        _isFiring = true;
        machine.Movement.SetMoveDirection(Vector2.zero);
    }

    public override void ExecuteLogic()
    {
        _timer -= Time.deltaTime;

        if (_isFiring && _timer <= 0f)
        {
            if (_firedCount < BulletCount)
            {
                ShootChasingBullet();
                _firedCount++;
                _timer = TimeBetweenBullets;
            }
            else
            {
                _isFiring = false;
                _timer = Cooldown;
            }
        }
        else if (!_isFiring && _timer <= 0f)
        {
            IsFinished = true;
        }
    }

    private void ShootChasingBullet()
    {
        if (ChasingProjectilePrefab == null || machine.Target == null) return;

        Vector3 spawnPos = machine.CachedTransform.position;
        
        // Tính hướng ban đầu hướng về Player
        Vector2 dirToTarget = (machine.Target.position - spawnPos).normalized;
        
        // Thêm độ lệch Spread nếu muốn
        float baseAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        float randomSpread = UnityEngine.Random.Range(-SpreadAngle, SpreadAngle);
        float finalAngle = (baseAngle + randomSpread) * Mathf.Deg2Rad;
        Vector2 initialDir = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));

        // Lấy đạn từ Pool (Sử dụng BulletPool của bạn)
        GameObject obj = BulletPool.Instance.Get(ChasingProjectilePrefab, spawnPos, Quaternion.identity);
        var projectile = obj.GetComponent<EnemyProjectile>();
        
        if (projectile != null)
        {
            // Chỉ truyền Vector2 theo đúng yêu cầu class cha
            projectile.Initialize(initialDir);
        }
    }

    public override void Exit() { }
}
