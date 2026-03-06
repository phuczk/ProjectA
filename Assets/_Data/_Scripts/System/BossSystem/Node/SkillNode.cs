using System;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
using SerializeReferenceEditor;

[Serializable]
public abstract class SkillNode : BossNode
{
    [Header("Base Settings")]
    public float Weight = 1f;
    public float DelayBeforeExecute = 1f;
    public bool IsCollapsed = false; // Save collapse state
    
    [Header("Animation Settings")]
    public string AnimationName = "";
    public bool UseCustomAnimation = false;
    public BossAnimationType AnimationType = BossAnimationType.Idle;
    public int AttackVariant = -1;
    public float DelayAnimation = 0f;

    [Header("Entry Conditions")]
    [SerializeReference, SR] public List<IBossCondition> EntryConditions = new List<IBossCondition>();

    public abstract BossSkillType SkillType { get; }

    public virtual void Initialize(BossController bossMachine)
    {
        machine = bossMachine;
    }

    public virtual bool CanEnter()
    {
        // Always return true for testing
        return true;
    }

    public override void Enter()
    {
        IsFinished = false;
    }

    public override void ExecuteLogic()
    {
        // Override in derived classes for specific skill logic
    }

    public override void Exit()
    {
        IsFinished = true;
    }

    public virtual void LogicUpdate()
    {
        // Override in derived classes for update logic
    }

    public override BossNode Execute(BossController boss)
    {
        // Call base Execute() to handle ExecuteLogic()
        return base.Execute(boss);
    }

    protected virtual string GetAnimationName()
    {
        return AnimationType switch
        {
            BossAnimationType.Idle => "Idle",
            BossAnimationType.Attack => AttackVariant >= 0 ? $"Attack{AttackVariant}" : "Attack",
            BossAnimationType.Special => "Special",
            BossAnimationType.Damage => "Damage",
            BossAnimationType.Death => "Death",
            _ => ""
        };
    }
}
