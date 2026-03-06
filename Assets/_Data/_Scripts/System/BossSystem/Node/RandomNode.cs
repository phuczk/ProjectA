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
        Debug.Log("RandomNode.Enter() - Preparing random selection");
    }
    
    public override void ExecuteLogic()
    {
        
        if (Branches.Count == 0)
        {
            Debug.LogWarning("RandomNode.ExecuteLogic() - No branches available");
            return;
        }
        
        // Calculate total percentage
        float totalPercent = 0f;
        foreach (var branch in Branches)
        {
            totalPercent += branch.Percent;
        }
        
        
        // Random selection
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
                    Debug.Log($"RandomNode.ExecuteLogic() - Set NextNodeGuid to: {NextNodeGuid}");
                }
                return;
            }
        }
        
        // Fallback to last branch
        var lastBranch = Branches[Branches.Count - 1];
        
        if (string.IsNullOrEmpty(lastBranch.NextNodeGuid))
        {
            Debug.LogWarning("RandomNode.ExecuteLogic() - Fallback branch has empty NextNodeGuid!");
        }
        else
        {
            NextNodeGuid = lastBranch.NextNodeGuid;
            IsFinished = true;
            Debug.Log($"RandomNode.ExecuteLogic() - Set NextNodeGuid to: {NextNodeGuid}");
        }
    }
    
    public override BossNode Execute(BossController boss)
    {
        if (Branches.Count == 0) return null;
        
        // Calculate total percentage
        float totalPercent = 0f;
        foreach (var branch in Branches)
        {
            totalPercent += branch.Percent;
        }
        
        // Random selection
        float randomValue = UnityEngine.Random.Range(0f, totalPercent);
        float currentPercent = 0f;
        
        foreach (var branch in Branches)
        {
            currentPercent += branch.Percent;
            if (randomValue <= currentPercent)
            {
                return boss.GetNode(branch.NextNodeGuid);
            }
        }
        
        // Fallback to last branch
        return boss.GetNode(Branches[Branches.Count - 1].NextNodeGuid);
    }
}