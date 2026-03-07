using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AddNode : BossNode
{
    [Header("Add Settings")]
    public List<AddBranch> InputBranches = new List<AddBranch>();
    
    [Header("Completion Tracking")]
    public List<bool> InputCompleted = new List<bool>();
    
    public override BossStateType StateType => BossStateType.Add;

    public override void ExecuteLogic()
    {
        // Check if all inputs are completed
        bool allInputsCompleted = true;
        for (int i = 0; i < InputCompleted.Count; i++)
        {
            if (!InputCompleted[i])
            {
                allInputsCompleted = false;
                break;
            }
        }
        
        if (allInputsCompleted && InputCompleted.Count > 0)
        {
            Debug.Log("AddNode: All inputs completed, finishing AddNode");
            IsFinished = true;
        }
        // Don't set IsFinished if not all inputs are completed
    }
    
    public override void Enter()
    {
        base.Enter();
        IsFinished = false;
        
        while (InputCompleted.Count < InputBranches.Count)
        {
            InputCompleted.Add(false);
        }
        
        while (InputCompleted.Count > InputBranches.Count)
        {
            InputCompleted.RemoveAt(InputCompleted.Count - 1);
        }
        
        Debug.Log($"AddNode Enter: {InputCompleted.Count} inputs, {InputBranches.Count} branches");
    }
    
    public override void Exit()
    {
        base.Exit();
        IsFinished = false;
        
        // Reset completion states
        for (int i = 0; i < InputCompleted.Count; i++)
        {
            InputCompleted[i] = false;
        }
    }
    
    public void MarkInputCompleted(int inputIndex)
    {
        if (inputIndex >= 0 && inputIndex < InputCompleted.Count)
        {
            InputCompleted[inputIndex] = true;
        }
    }
}
