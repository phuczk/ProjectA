using UnityEngine;

[System.Serializable]
public class AddDamageEffect : Effect
{
    public float bonus;

    public override void OnApply(PlayerController player)
    {
        //player.DamageMultiplier += bonus;
    }

    public override void OnRemove(PlayerController player)
    {
        //player.DamageMultiplier -= bonus;
    }

    public override void OnGunFire(PlayerController player, Vector2 direction)
    {
        Debug.Log($"AddDamageEffect: {bonus}");
    }
}
