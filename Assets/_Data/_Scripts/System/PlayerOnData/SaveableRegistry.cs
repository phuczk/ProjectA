using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SaveableRegistry
{
    private static List<ISaveable> _cache;

    public static void ApplyAll(SaveData data)
    {
        Cache();
        foreach (var s in _cache)
            s.LoadData(data);
    }

    public static void SaveAll(SaveData data)
    {
        Cache();
        foreach (var s in _cache)
            s.SaveData(data);
    }

    private static void Cache()
    {
        _cache = Object
            .FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .ToList();
    }
}
