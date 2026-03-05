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

    //public virtual void Initialize(BossController machine) => this.machine = machine;

    public virtual void Enter()
    {
        IsFinished = false;
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
        ExecuteLogic();
        return null; // Default implementation - override in derived classes
    }

    public bool CanEnter(BossController machine)
    {
        return true;
    }
}
