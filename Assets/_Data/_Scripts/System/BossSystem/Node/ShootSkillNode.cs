using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ShootSkillNode : SkillNode
{
    [Header("Shoot Settings")]
    public GameObject BulletPrefab;
    public float BulletSpeed = 10f;
    public float BulletDamage = 5f;
    public float FireRate = 2f;
    public int BurstCount = 1;
    public float BurstDelay = 0.1f;
    public Vector3 ShootOffset = Vector3.forward;
    public bool AimAtPlayer = true;

    public override BossSkillType SkillType => BossSkillType.Shoot;
    public override BossStateType StateType => BossStateType.Shoot;

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"ShootSkillNode.Enter() - Entering shoot state");
    }

    public override void ExecuteLogic()
    {
        IsFinished = true;
    }
}
