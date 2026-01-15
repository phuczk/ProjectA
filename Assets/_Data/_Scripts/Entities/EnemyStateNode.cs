using UnityEngine;
using System;
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

    [SerializeReference, SR]
    public List<IStateDecision> EntryConditions = new List<IStateDecision>();

    [Header("Transition Settings")]
    [SerializeReference, SR] public List<StateTransition> Transitions = new List<StateTransition>();

    protected EnemyUniversalMachine machine;
    public abstract EnemyStateType StateType { get; }
    public bool IsFinished { get; protected set; } = false;

    public void ResetFinished() => IsFinished = false;

    public virtual void Initialize(EnemyUniversalMachine machine) => this.machine = machine;

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

    public abstract void Enter();
    public abstract void ExecuteLogic();
    public abstract void Exit();
}
