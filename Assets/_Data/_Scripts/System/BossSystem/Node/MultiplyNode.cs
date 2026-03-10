using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MultiplyNode : BossNode
{
    [Header("Multiply Settings")]
    public List<MultiplyBranch> Branches = new List<MultiplyBranch>();
    
    public override BossStateType StateType => BossStateType.Multiply;

    public override void Enter()
    {
        base.Enter();
    }
    
    public override void ExecuteLogic()
    {
        if (Branches.Count > 0 && !string.IsNullOrEmpty(Branches[0].NextNodeGuid))
        {
            NextNodeGuid = Branches[0].NextNodeGuid;
            IsFinished = true;
        }
    }
}
