using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class AddDamageEffect : Effect
{
    public int bonus;
    public override CursedObjectType EffectType => CursedObjectType.Passive;

    public override void OnApply(PlayerController player)
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        if (save?.player != null)
        {
            save.player.currentDamage += bonus;
            
            if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
            else SaveSystem.Save(save);
        }
    }

    public override void OnRemove(PlayerController player)
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        if (save?.player != null)
        {
            save.player.currentDamage = Mathf.Max(0, save.player.currentDamage - bonus);
            
            if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
            else SaveSystem.Save(save);
        }
    }

    public override void OnGunFire(PlayerController player, Vector2 direction)
    {
        Debug.Log($"AddDamageEffect: Fire with +{bonus} damage bonus");
    }
}
