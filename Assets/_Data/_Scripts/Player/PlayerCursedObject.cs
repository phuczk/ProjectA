using GlobalEnums;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursedObject: MonoBehaviour
{
    public bool isTestMode = false;
    private List<string> _unlocked = new();

    public bool Has(CursedObjectData item)
    {
        if (isTestMode) return true;
        return _unlocked.Contains(item.id);
    }
    public void OnUnlocked(CursedObjectData item)
    {
        var mgr = SaveManager.Instance;
        if (mgr != null)
        {
            mgr.SaveGame();
            return;
        }
        var data = SaveSystemz.Load();
        if (data.player == null) data.player = new PlayerData();
        data.player.unlockedCursedObjects.Add(item.id);
        SaveSystemz.Save(data);
    }
    
    public void SetCurrentCursedObject(CursedObjectData item)
    {
        var data = SaveSystemz.Load();
        if (data.player == null) data.player = new PlayerData();
        data.player.currentCursedObjects = new List<string>() { item.id};
        SaveSystemz.Save(data);
    }

    public void SaveData(SaveData data)
    {
        if (data.player == null) data.player = new PlayerData();
        data.player.unlockedCursedObjects = _unlocked.ToList();
    }

    public void LoadData(SaveData data)
    {
        _unlocked.Clear();
        if (data?.player?.unlockedCursedObjects == null) return;

        foreach (var a in data.player.unlockedCursedObjects)
            _unlocked.Add(a);
    }
}
