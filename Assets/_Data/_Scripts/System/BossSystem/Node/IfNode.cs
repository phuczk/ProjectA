using System;
using System.Collections.Generic;

[Serializable]
public class IfNode : BossNode
{
    public List<ConditionBranch> Conditions = new List<ConditionBranch>();

    public string ElseNodeGuid;

    public override BossStateType StateType => BossStateType.If;

    public override BossNode Execute(BossController boss)
    {
        BossContext context = boss.Context;

        foreach (var branch in Conditions)
        {
            if (branch.Condition == null) continue;

            if (branch.Condition.Check(context))
            {
                return boss.GetNode(branch.NextNodeGuid);
            }
        }

        return boss.GetNode(ElseNodeGuid);
    }
}