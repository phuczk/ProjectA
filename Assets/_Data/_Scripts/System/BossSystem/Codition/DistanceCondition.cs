using UnityEngine;

[System.Serializable]
public class DistanceCondition : IBossCondition
{
    public float distance;

    public bool Check(BossContext context)
    {
        if (context.player == null) return false;

        float d = Vector3.Distance(
            context.boss.transform.position,
            context.player.position);

        return d < distance;
    }
}