using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class AddDamageEffect : Effect
{
    public float bonus;
    public override CursedObjectType EffectType => CursedObjectType.Passive;

    public override void OnApply(PlayerController player)
    {
    }

    public override void OnRemove(PlayerController player)
    {
        //player.DamageMultiplier -= bonus;
    }

    private void HandleFire(Vector2 direction)
    {
        Debug.Log($"AddDamageEffect: {bonus}");
    }

    public override void OnGunFire(PlayerController player, Vector2 direction)
    {
        //player._effectRunner?.RaiseGunFire(direction);
        Debug.Log($"AddDamageEffect: {bonus}");
    }
}
