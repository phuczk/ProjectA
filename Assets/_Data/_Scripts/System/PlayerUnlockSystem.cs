using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerUnlockSystem<TEnum> : MonoBehaviour, ISaveable where TEnum : struct, System.Enum
{
    [Header("Debug")]
    public bool isTestMode = false;

    protected HashSet<TEnum> _unlocked = new();

    public bool Has(TEnum type)
    {
        if (isTestMode) return true;
        return _unlocked.Contains(type);
    }

    public void Unlock(TEnum type)
    {
        if (_unlocked.Add(type))
            OnUnlocked(type);
    }

    public void ResetAll()
    {
        _unlocked.Clear();
    }

    protected virtual void OnUnlocked(TEnum type) { }

    // === ISaveable ===
    public abstract void SaveData(SaveData data);
    public abstract void LoadData(SaveData data);
}
