using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IfNode : BossNode
{
    public List<ConditionBranch> Conditions = new List<ConditionBranch>();

    public string ElseNodeGuid;

    public override BossStateType StateType => BossStateType.If;

    public override void Enter()
    {
        base.Enter();
        Debug.Log("IfNode.Enter() - Evaluating conditions");
    }

    public override void ExecuteLogic()
    {
        BossContext context = machine.Context;

        foreach (var branch in Conditions)
        {
            if (branch.Condition == null) continue;

            Debug.Log($"IfNode.ExecuteLogic() - Checking condition: {branch.Condition.GetType().Name}");

            if (branch.Condition.Check(context))
            {
                Debug.Log($"IfNode.ExecuteLogic() - Condition true, transitioning to {branch.NextNodeGuid}");
                NextNodeGuid = branch.NextNodeGuid;
                IsFinished = true;
                return;
            }
        }

        Debug.Log($"IfNode.ExecuteLogic() - No conditions true, transitioning to else: {ElseNodeGuid}");
        NextNodeGuid = ElseNodeGuid;
        IsFinished = true;
    }

    // public override BossNode Execute(BossController boss)
    // {
    //     BossContext context = boss.Context;

    //     // Check conditions in order
    //     foreach (var branch in Conditions)
    //     {
    //         if (branch.Condition == null) continue;

    //         Debug.Log($"IfNode.Execute() - Checking condition: {branch.Condition.GetType().Name}");

    //         if (branch.Condition.Check(context))
    //         {
    //             Debug.Log($"IfNode.Execute() - Condition true, returning node: {branch.NextNodeGuid}");
    //             return boss.GetNode(branch.NextNodeGuid);
    //         }
    //     }

    //     // No conditions true, return else node
    //     Debug.Log($"IfNode.Execute() - No conditions true, returning else node: {ElseNodeGuid}");
    //     return boss.GetNode(ElseNodeGuid);
    // }
}