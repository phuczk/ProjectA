using System;
using UnityEngine;

[Serializable]
public class AttackSkillNode : SkillNode
{
    [Header("Attack Settings")]
    public int Damage = 1;
    public float AttackRange = 2f;
    public float AttackCooldown = 1f;
    public LayerMask TargetLayer;
    public Vector3 AttackOffset = Vector3.zero;
    public bool UseAttackAnimation = true;

    public override BossSkillType SkillType => BossSkillType.Attack;
    public override BossStateType StateType => BossStateType.Attack;

    public override void Enter()
    {
        base.Enter();
        Debug.Log("AttackSkillNode.Enter() - Entering attack state");
    }

    public override void ExecuteLogic()
    {
        IsFinished = true;
    }
}
