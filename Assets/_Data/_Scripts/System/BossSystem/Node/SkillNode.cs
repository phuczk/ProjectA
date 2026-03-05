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
    
    [Header("Animation Settings")]
    public string AnimationName = "";
    public bool UseCustomAnimation = false;
    public BossAnimationType AnimationType = BossAnimationType.Idle;
    public int AttackVariant = -1;
    public float DelayAnimation = 0f;

    [Header("Entry Conditions")]
    [SerializeReference, SR] public List<IBossCondition> EntryConditions = new List<IBossCondition>();

    protected BossController machine;
    public abstract BossSkillType SkillType { get; }
    public bool IsFinished { get; protected set; } = false;

    public virtual void Initialize(BossController bossMachine)
    {
        machine = bossMachine;
    }

    public virtual bool CanEnter()
    {
        // Always return true for testing
        return true;
    }

    public virtual void Enter()
    {
        IsFinished = false;
        Debug.Log($"Entering {SkillType} skill");
    }

    public virtual void ExecuteLogic()
    {
        // Override in derived classes for specific skill logic
        Debug.Log($"Executing {SkillType} skill");
    }

    public virtual void Exit()
    {
        IsFinished = true;
        Debug.Log($"Exiting {SkillType} skill");
    }

    public virtual void LogicUpdate()
    {
        // Override in derived classes for update logic
    }

    public override BossNode Execute(BossController boss)
    {
        if (!CanEnter()) return null;

        Enter();
        ExecuteLogic();
        Exit();

        // For testing, return default next node
        return boss.GetNode(NextNodeGuid);
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
