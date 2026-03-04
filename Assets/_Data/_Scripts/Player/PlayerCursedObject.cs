using GlobalEnums;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursedObject: MonoBehaviour, ISaveable
{
    public bool isTestMode = false;
    private HashSet<string> _unlocked = new();

    public bool Has(CursedObjectData item)
    {
        if (isTestMode) return true;
        return _unlocked.Contains(item.id);
    }
    public void OnUnlocked(CursedObjectData item)
    {
        if (!_unlocked.Contains(item.id))
        {
            _unlocked.Add(item.id);
        }

        var mgr = SaveManager.Instance;
        if (mgr != null)
        {
            mgr.SaveGame();
            return;
        }
        var data = SaveSystem.Load();
        if (data.items == null) data.items = new ItemData();
        if (!data.items.unlockedCursedObjects.Contains(item.id))
        {
            data.items.unlockedCursedObjects.Add(item.id);
        }
        SaveSystem.Save(data);
    }

    // 🔥 NEW: Unlock cursed object by ID
    public void UnlockCursedObject(string cursedId)
    {
        // Tạo CursedObjectData tạm thời
        var tempData = new CursedObjectData { id = cursedId };
        OnUnlocked(tempData);
    }
    
    public void SetCurrentCursedObject(CursedObjectData item)
    {
        var mgr = SaveManager.Instance;
        if (mgr != null)
        {
            if (mgr.CurrentData.player == null) mgr.CurrentData.player = new PlayerData();
            mgr.CurrentData.player.currentCursedObjects = new List<string>() { item.id };
            mgr.SaveGame();
            return;
        }

        var data = SaveSystem.Load();
        if (data.player == null) data.player = new PlayerData();
        data.player.currentCursedObjects = new List<string>() { item.id};
        SaveSystem.Save(data);
    }

    public void SaveData(SaveData data)
    {
        // 🔥 FIXED: Không clear unlockedCursedObjects, chỉ sync missing items
        if (data.items?.unlockedCursedObjects == null)
        {
            if (data.items == null) data.items = new ItemData();
            data.items.unlockedCursedObjects = new List<string>();
        }

        // Chỉ add items từ _unlocked set mà chưa có trong list
        foreach (var id in _unlocked)
        {
            if (!data.items.unlockedCursedObjects.Contains(id))
            {
                data.items.unlockedCursedObjects.Add(id);
            }
        }
    }

    public void LoadData(SaveData data)
    {
        _unlocked.Clear();

        if (data?.items?.unlockedCursedObjects == null)
            return;

        foreach (var id in data.items.unlockedCursedObjects)
            _unlocked.Add(id);
    }
}
