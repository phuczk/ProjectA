using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SpecialSkillNode : SkillNode
{
    [Header("Special Settings")]
    public SpecialSkillType SpecialType;
    public float Duration = 3f;
    public float Cooldown = 10f;
    public float EffectRadius = 5f;
    public GameObject EffectPrefab;
    public AudioClip SpecialSound;

    public override BossSkillType SkillType => BossSkillType.Special;
    public override BossStateType StateType => BossStateType.Special;

    public override void ExecuteLogic()
    {
        // Perform special skill logic here
        Debug.Log($"SpecialSkillNode.ExecuteLogic() - Performing special: {SpecialType}");
        
        // For now, just mark as finished immediately
        // You can add special effects, etc. later
        IsFinished = true;
    }
}

public enum SpecialSkillType
{
    AreaDamage,
    Buff,
    Summon
}
