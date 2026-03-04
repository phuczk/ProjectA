using GlobalEnums;
using System.Linq;
using System.Collections.Generic;

public class PlayerAbility: PlayerUnlockSystem<AbilityType>
{   
    protected override void OnUnlocked(AbilityType type)
    {
        var mgr = SaveManager.Instance;
        if (mgr != null)
        {
            mgr.SaveGame();
            return;
        }
        var data = SaveSystem.Load();
        if (data.player == null) data.player = new PlayerData();
        data.player.unlockedAbilities = _unlocked.ToList();
        SaveSystem.Save(data);
    }

    public override void SaveData(SaveData data)
    {
        if (data.player == null) data.player = new PlayerData();
        
        if (data.player.unlockedAbilities == null)
            data.player.unlockedAbilities = new List<AbilityType>();

        foreach (var ability in _unlocked)
        {
            if (!data.player.unlockedAbilities.Contains(ability))
            {
                data.player.unlockedAbilities.Add(ability);
            }
        }
    }

    public override void LoadData(SaveData data)
    {
        _unlocked.Clear();
        if (data?.player?.unlockedAbilities == null) return;

        foreach (var a in data.player.unlockedAbilities)
            _unlocked.Add(a);
    }
}
