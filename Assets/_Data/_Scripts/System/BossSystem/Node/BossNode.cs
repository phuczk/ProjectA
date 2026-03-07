using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using SerializeReferenceEditor;

[Serializable]
public abstract class BossNode
{
    [HideInInspector] public string Guid = System.Guid.NewGuid().ToString();
    [HideInInspector] public Vector2 GraphPosition;
    public string NextNodeGuid = "";
    public abstract BossStateType StateType { get; }
    public bool IsFinished { get; protected set; } = false;
    
    [Header("Transition Settings")]
    [SerializeReference, SR] public List<BossTransition> Transitions = new List<BossTransition>();
    
    private bool hasExecuted = false;

    public float Delay = 0f;

    public virtual void Enter()
    {
        IsFinished = false;
        hasExecuted = false;
    }
    
    public virtual void ExecuteLogic(){}
    
    public virtual void Exit()
    {
        IsFinished = true;
    }
    
    public virtual void ResetFinished() => IsFinished = false;

    protected BossController machine;
    
    public virtual void Initialize(BossController bossMachine) => this.machine = bossMachine;

    public virtual BossNode Execute(BossController boss)
    {
        if (!hasExecuted)
        {
            ExecuteLogic();
            hasExecuted = true;
        }
        
        return null;
    }

    public bool CanEnter(BossController machine)
    {
        return true;
    }
}
