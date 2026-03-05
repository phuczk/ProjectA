using System;

[Serializable]
public class ConditionBranch
{
    public IBossCondition Condition;
    public string NextNodeGuid;
}
