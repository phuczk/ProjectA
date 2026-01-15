using UnityEngine;

[System.Serializable]
public class HealEffect : Effect
{
    public int bonus;

    public override void OnHeal(PlayerController player)
    {
        player.Health?.Heal(bonus);
    }
}
