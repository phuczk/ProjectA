using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomNode : BossNode
{
    public List<RandomBranch> Branches = new List<RandomBranch>();
    
    public override BossStateType StateType => BossStateType.Random;
    
    public override void Enter()
    {
        base.Enter();
    }
    
    public override void ExecuteLogic()
    {
        
        if (Branches.Count == 0)
        {
            return;
        }
        
        float totalPercent = 0f;
        foreach (var branch in Branches)
        {
            totalPercent += branch.Percent;
        }
        
        
        float randomValue = UnityEngine.Random.Range(0f, totalPercent);
        float currentPercent = 0f;
        
        foreach (var branch in Branches)
        {
            currentPercent += branch.Percent;
            
            if (randomValue <= currentPercent)
            {
                if (string.IsNullOrEmpty(branch.NextNodeGuid))
                {
                    Debug.LogWarning("RandomNode.ExecuteLogic() - Selected branch has empty NextNodeGuid!");
                }
                else
                {
                    NextNodeGuid = branch.NextNodeGuid;
                    IsFinished = true;
                }
                return;
            }
        }
        
        var lastBranch = Branches[Branches.Count - 1];
        
        if (string.IsNullOrEmpty(lastBranch.NextNodeGuid))
        {
            Debug.LogWarning("RandomNode.ExecuteLogic() - Fallback branch has empty NextNodeGuid!");
        }
        else
        {
            NextNodeGuid = lastBranch.NextNodeGuid;
            IsFinished = true;
        }
    }
}