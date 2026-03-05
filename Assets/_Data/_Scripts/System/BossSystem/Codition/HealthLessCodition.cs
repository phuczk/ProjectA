using UnityEngine;

[System.Serializable]
public class HpLessCondition : IBossCondition
{
    public float value;

    public bool Check(BossContext context)
    {
        return context.hp < value;
    }
}