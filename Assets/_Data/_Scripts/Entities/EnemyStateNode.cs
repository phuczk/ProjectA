using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using SerializeReferenceEditor;

[Serializable]
public class StateTransition
{
    [SerializeReference, SR] public IStateDecision Decision;
    public EnemyStateType TargetState;
    public string TargetNodeGuid;
}

[Serializable]
public abstract class EnemyStateNode
{
    [HideInInspector] public string Guid = System.Guid.NewGuid().ToString();
    [HideInInspector] public Vector2 GraphPosition;

    [Header("Base Settings")]
    public float Weight = 1f;
    
    [Header("Animation Settings")]
    public string animationName = "";
    public bool useCustomAnimation = false;
    public EnemyAnimationType animationType = EnemyAnimationType.Idle;
    public int attackVariant = -1;

    public float delayAnimation = 0f;

    [SerializeReference, SR]
    public List<IStateDecision> EntryConditions = new List<IStateDecision>();

    [Header("Transition Settings")]
    [SerializeReference, SR] public List<StateTransition> Transitions = new List<StateTransition>();

    protected EnemyUniversalMachine machine;
    public abstract EnemyStateType StateType { get; }
    public bool IsFinished { get; protected set; } = false;

    public void ResetFinished() => IsFinished = false;

    public virtual void Initialize(EnemyUniversalMachine machine) => this.machine = machine;

    public virtual void Enter()
    {
        if (machine.Animation != null)
        {
            machine.StartCoroutine(DelayedPlayAnimation());
        }
    }
    
    private IEnumerator DelayedPlayAnimation()
    {
        if (delayAnimation > 0f)
        {
            yield return new WaitForSeconds(delayAnimation);
        }
        PlayStateAnimation();
    }
    
    public virtual void ExecuteLogic(){}
    
    public virtual void Exit()
    {
        if (machine.Animation != null && useCustomAnimation)
        {
            ResetStateAnimation();
        }
    }

    public virtual void LogicUpdate()
    {
        foreach (var transition in Transitions)
        {
            if (transition.Decision.Decide(machine))
            {
                machine.TransitionToState(transition.TargetState);
                return;
            }
        }

        if (!IsFinished)
        {
            ExecuteLogic();
        }
    }

    public bool CanEnter(EnemyUniversalMachine machine)
    {
        if (EntryConditions.Count == 0) return false;
        foreach (var cond in EntryConditions)
        {
            if (!cond.Decide(machine)) return false;
        }
        return true;
    }
    
    protected virtual void PlayStateAnimation()
    {
        if (machine.Animation == null) return;

        if (useCustomAnimation && !string.IsNullOrEmpty(animationName))
        {
            machine.Animation.PlayAnimationByName(animationName);
        }
        else
        {
            switch (animationType)
            {
                case EnemyAnimationType.Idle: machine.Animation.PlayIdle(); break;
                case EnemyAnimationType.Move: machine.Animation.PlayMove(); break;
                case EnemyAnimationType.Attack: machine.Animation.PlayAttack(attackVariant); break;
                case EnemyAnimationType.Death: machine.Animation.PlayDeath(); break;
                case EnemyAnimationType.Hurt: machine.Animation.PlayHurt(); break;
            }
        }
    }
    
    protected virtual void ResetStateAnimation()
    {
        if (animationType != EnemyAnimationType.Death && machine.Animation != null)
        {
            machine.Animation.ResetToIdle();
        }
    }
}
