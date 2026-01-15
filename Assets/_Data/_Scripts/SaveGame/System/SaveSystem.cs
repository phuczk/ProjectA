using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;

public static class SaveSystemz
{
    private static int ActiveSlot = 1;
    private static string Dir => Application.persistentDataPath + "/saves";
    private static string Path => $"{Dir}/slot_{ActiveSlot}.json";
    private static readonly int MaxSlot = 4;

    public static void Save(SaveData data)
    {
        if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path, json);
    }

    public static SaveData Load()
    {
        try
        {
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
            if (!File.Exists(Path))
                return CreateNewSave();

            string json = File.ReadAllText(Path);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return CreateNewSave();
            if (data.player == null)
                data.player = new PlayerData { maxHealth = 5, maxMana = 3, position = Vector3.zero };
            if (data.world == null)
                data.world = new WorldSaveData { currentSceneName = "TestScene" };
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"SaveSystem load failed: {ex.Message}");
            return CreateNewSave();
        }
    }

    public static void SetActiveSlot(int slot)
    {
        ActiveSlot = Mathf.Clamp(slot, 1, MaxSlot);
    }

    public static int GetActiveSlot() => ActiveSlot;

    public static SaveData LoadFromSlot(int slot)
    {
        SetActiveSlot(slot);
        return Load();
    }

    public static void SaveToSlot(int slot, SaveData data)
    {
        SetActiveSlot(slot);
        Save(data);
    }

    public static List<SaveSlotInfo> ListSlots()
    {
        if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
        var result = new List<SaveSlotInfo>();
        for (int i = 1; i <= MaxSlot; i++)
        {
            string p = $"{Dir}/slot_{i}.json";
            bool exists = File.Exists(p);
            DateTime last = exists ? File.GetLastWriteTime(p) : DateTime.MinValue;
            SaveData data = null;
            if (exists)
            {
                try
                {
                    string json = File.ReadAllText(p);
                    data = JsonUtility.FromJson<SaveData>(json);
                }
                catch { }
            }
            result.Add(new SaveSlotInfo
            {
                slot = i,
                exists = exists,
                lastWriteTime = last,
                playerSummary = data != null ? new PlayerSummary
                {
                    maxHealth = data.player != null ? data.player.maxHealth : 0,
                    maxMana = data.player != null ? data.player.maxMana : 0,
                    position = data.player != null ? data.player.position : Vector3.zero,
                    currentMoney = data.player != null ? data.player.currentMoney : 0,
                } : default
            });
        }
        return result;
    }

    public static void DeleteSlot(int slot)
    {
        string p = $"{Dir}/slot_{slot}.json";
        if (File.Exists(p))
            File.Delete(p);
    }
    
    public static bool SlotExists(int slot)
    {
        string p = $"{Dir}/slot_{slot}.json";
        return File.Exists(p);
    }

    private static SaveData CreateNewSave()
    {
        return new SaveData
        {
            player = new PlayerData
            {
                maxHealth = 5,
                maxMana = 3,
                position = Vector3.zero,
                unlockedAbilities = new List<AbilityType> { AbilityType.Move},
            },
            world = new WorldSaveData
            {
                currentSceneName = "TestScene"
            },
        };
    }

    public class SaveSlotInfo
    {
        public int slot;
        public bool exists;
        public DateTime lastWriteTime;
        public PlayerSummary playerSummary;
    }

    public struct PlayerSummary
    {
        public int maxHealth;
        public int maxMana;
        public Vector3 position;
        public int currentMoney;
    }
}
