using System;

[Serializable]
public class BossTransition
{
    public IBossCondition Condition;
    public BossStateType TargetStateType;
    
    public bool Check(BossContext context)
    {
        if (Condition == null) return false;
        return Condition.Check(context);
    }
}
