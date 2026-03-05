using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomNode : BossNode
{
    public List<RandomBranch> Branches = new List<RandomBranch>();
    
    public override BossStateType StateType => BossStateType.Random;
    
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