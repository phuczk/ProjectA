using GlobalEnums;
using System.Linq;

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
        var data = SaveSystemz.Load();
        if (data.player == null) data.player = new PlayerData();
        data.player.unlockedAbilities = _unlocked.ToList();
        SaveSystemz.Save(data);
    }

    public override void SaveData(SaveData data)
    {
        if (data.player == null) data.player = new PlayerData();
        data.player.unlockedAbilities = _unlocked.ToList();
    }

    public override void LoadData(SaveData data)
    {
        _unlocked.Clear();
        if (data?.player?.unlockedAbilities == null) return;

        foreach (var a in data.player.unlockedAbilities)
            _unlocked.Add(a);
    }
}
