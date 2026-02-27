using System;
using UnityEngine;
using GlobalEnums;

[Serializable]
public class ThrowAttackNode : EnemyStateNode
{
    public enum ThrowMode { HorizontalOnly, AimAtPlayer, FixedDirections }

    public override EnemyStateType StateType => EnemyStateType.Attack;

    [Header("Throw Settings")]
    public GameObject ProjectilePrefab;
    public ThrowMode Mode = ThrowMode.HorizontalOnly;
    
    [Header("Randomness")]
    public float VerticalPosSpread = 0.5f;
    public float AngleSpread = 0.0f;

    [Header("Base Config")]
    public Vector2[] FixedAngles = new Vector2[] { Vector2.right };
    public float WindupTime = 0.5f;
    public float RecoverTime = 0.5f;
    public Vector2 SpawnOffset = new Vector2(0.5f, 0.5f);
    public int ThrowCount = 1;

    private float _timer;
    private bool _hasThrown;
    private int throwCount;

    public override void Enter()
    {
        base.Enter();
        IsFinished = false;
        _hasThrown = false;
        _timer = WindupTime;
        machine.Movement.SetMoveDirection(Vector2.zero);
        FacePlayer();
        throwCount = ThrowCount;
    }

    private void FacePlayer()
    {
        if (machine.Target == null) return;
        float dir = (machine.Target.position.x > machine.CachedTransform.position.x) ? 1f : -1f;
        machine.CachedTransform.localScale = new Vector3(dir, 1, 1);
    }

    public override void ExecuteLogic()
    {
        base.ExecuteLogic();
        _timer -= Time.deltaTime;

        if (!_hasThrown && _timer <= 0f && throwCount > 0)
        {
            PerformThrow();
            _hasThrown = true;
            
            _timer = RecoverTime; 
        }
        else if (_hasThrown && _timer <= 0f) 
        {
            if (throwCount > 0) 
            {
                _hasThrown = false; 
                _timer = WindupTime;
            }
            else 
            {
                IsFinished = true; 
                machine.SetCooldown("GlobalAttack", 1.0f); 
            }
        }
    }

    private void PerformThrow()
    {
        if (ProjectilePrefab == null) return;

        float facingDir = machine.CachedTransform.localScale.x > 0 ? 1f : -1f;
        
        Vector3 baseSpawnPos = machine.CachedTransform.position + new Vector3(SpawnOffset.x * facingDir, SpawnOffset.y, 0);

        float randomY = UnityEngine.Random.Range(-VerticalPosSpread, VerticalPosSpread);
        Vector3 finalSpawnPos = baseSpawnPos + new Vector3(0, randomY, 0);

        float randomAngle = UnityEngine.Random.Range(-AngleSpread, AngleSpread);

        switch (Mode)
        {
            case ThrowMode.HorizontalOnly:
                Vector2 horDir = new Vector2(facingDir, randomAngle).normalized;
                SpawnProjectile(finalSpawnPos, horDir);
                break;

            case ThrowMode.AimAtPlayer:
                if (machine.Target != null)
                {
                    Vector2 targetDir = ((Vector2)machine.Target.position - (Vector2)finalSpawnPos).normalized;
                    targetDir = new Vector2(targetDir.x, targetDir.y + randomAngle).normalized;
                    SpawnProjectile(finalSpawnPos, targetDir);
                }
                else SpawnProjectile(finalSpawnPos, new Vector2(facingDir, randomAngle).normalized);
                break;

            case ThrowMode.FixedDirections:
                foreach (var angle in FixedAngles)
                {
                    Vector2 dir = new Vector2(angle.x * facingDir, angle.y + randomAngle).normalized;
                    SpawnProjectile(finalSpawnPos, dir);
                }
                break;
            
            default:
                break;
        }
        throwCount--;
    }

    private void SpawnProjectile(Vector3 pos, Vector2 dir)
    {
        GameObject obj = BulletPool.Instance.Get(ProjectilePrefab, pos, Quaternion.identity);
        var projectile = obj.GetComponent<EnemyProjectile>();
        projectile?.Initialize(dir);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
