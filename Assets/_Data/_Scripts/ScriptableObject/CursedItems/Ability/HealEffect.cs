using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class HealEffect : Effect
{
    public int bonus;
    public override CursedObjectType EffectType => CursedObjectType.Ability;

    public override void OnHeal(PlayerController player)
    {
        Debug.Log($"Heal {bonus}");
        player.Health?.Heal(bonus);
    }
}
