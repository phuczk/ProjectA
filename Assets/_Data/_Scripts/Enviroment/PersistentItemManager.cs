using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;

public class PersistentItemManager : Singleton<PersistentItemManager>
{
    [Header("Item Tracking")]
    private HashSet<string> _collectedAbilityItems = new HashSet<string>();
    private HashSet<string> _collectedCursedItems = new HashSet<string>();
    private HashSet<string> _collectedGunItems = new HashSet<string>();

    protected override void Awake()
    {
        base.Awake();
        LoadCollectedItems();
    }

    private void LoadCollectedItems()
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        
        if (save?.world == null) 
        {
            return;
        }

        if (save.world.collectedAbilityItems != null)
        {
            _collectedAbilityItems.Clear();
            foreach (var id in save.world.collectedAbilityItems)
                _collectedAbilityItems.Add(id);
        }

        if (save.world.collectedCursedItems != null)
        {
            _collectedCursedItems.Clear();
            foreach (var id in save.world.collectedCursedItems)
                _collectedCursedItems.Add(id);
        }

        if (save.world.collectedGunItems != null)
        {
            _collectedGunItems.Clear();
            foreach (var id in save.world.collectedGunItems)
                _collectedGunItems.Add(id);
        }
    }

    public bool IsAbilityItemCollected(string itemId)
    {
        return _collectedAbilityItems.Contains(itemId);
    }

    public bool IsCursedItemCollected(string itemId)
    {
        return _collectedCursedItems.Contains(itemId);
    }

    public bool IsGunItemCollected(string itemId)
    {
        return _collectedGunItems.Contains(itemId);
    }

    public void MarkAbilityItemCollected(string itemId)
    {
        if (_collectedAbilityItems.Add(itemId))
        {
            SaveCollectedItems();
        }
    }

    public void MarkCursedItemCollected(string itemId)
    {
        if (_collectedCursedItems.Add(itemId))
        {
            SaveCollectedItems();
        }
    }

    public void MarkGunItemCollected(string itemId)
    {
        if (_collectedGunItems.Add(itemId))
        {
            SaveCollectedItems();
        }
    }

    private void SaveCollectedItems()
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        if (save?.world == null) return;

        if (save.world.collectedAbilityItems == null)
            save.world.collectedAbilityItems = new List<string>();
        if (save.world.collectedCursedItems == null)
            save.world.collectedCursedItems = new List<string>();
        if (save.world.collectedGunItems == null)
            save.world.collectedGunItems = new List<string>();

        save.world.collectedAbilityItems = _collectedAbilityItems.ToList();
        save.world.collectedCursedItems = _collectedCursedItems.ToList();
        save.world.collectedGunItems = _collectedGunItems.ToList();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
        else
            SaveSystem.Save(save);
    }

    public void ResetCollectedItems()
    {
        _collectedAbilityItems.Clear();
        _collectedCursedItems.Clear();
        _collectedGunItems.Clear();
        SaveCollectedItems();
    }

    public bool PlayerHasAbility(AbilityType abilityType)
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        bool hasAbility = save?.player?.unlockedAbilities?.Contains(abilityType) ?? false;
        
        Debug.Log($"PersistentItemManager: Checking ability {abilityType}");
        Debug.Log($"SaveManager exists: {SaveManager.Instance != null}");
        if (save?.player?.unlockedAbilities != null)
        {
            Debug.Log($"Unlocked abilities: [{string.Join(", ", save.player.unlockedAbilities)}]");
        }
        Debug.Log($"Has ability {abilityType}: {hasAbility}");
        
        return hasAbility;
    }

    public bool PlayerHasCursedItem(string cursedId)
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        return save?.items?.unlockedCursedObjects?.Contains(cursedId) ?? false;
    }

    public bool PlayerHasGun(GunType gunType)
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        return save?.player?.unlockedGuns?.Contains(gunType) ?? false;
    }
}
