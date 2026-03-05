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

    public override void ExecuteLogic()
    {
        // Perform attack logic here
        //Debug.Log("AttackSkillNode.ExecuteLogic() - Performing attack");
        
        // For now, just mark as finished immediately
        // You can add attack animation, damage, etc. later
        IsFinished = true;
    }
}
