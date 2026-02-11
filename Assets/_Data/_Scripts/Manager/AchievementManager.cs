using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : Singleton<AchievementManager>
{
    public List<AchievementData> allAchievements;
    private Dictionary<string, int> _progress = new();
    private HashSet<string> _unlockedIds = new();

    private void Start()
    {
        LoadProgress();
    }

    public void AddProgress(string id, int amount)
    {
        if (_unlockedIds.Contains(id)) return;

        if (!_progress.ContainsKey(id)) _progress[id] = 0;
        _progress[id] += amount;

        AchievementData data = allAchievements.Find(a => a.id == id);
        if (data != null && _progress[id] >= data.goalValue)
        {
            Unlock(id);
        }
    }

    private void Unlock(string id)
    {
        if (_unlockedIds.Contains(id)) return;
        
        _unlockedIds.Add(id);
        SaveProgress();
        
        Debug.Log($"UNLOCKED: {id}");
    }

    private void SaveProgress()
    {
        foreach (var id in _unlockedIds) PlayerPrefs.SetInt("Ach_Unlock_" + id, 1);
        foreach (var item in _progress) PlayerPrefs.SetInt("Ach_Prog_" + item.Key, item.Value);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        foreach (var a in allAchievements)
        {
            if (PlayerPrefs.HasKey("Ach_Unlock_" + a.id)) _unlockedIds.Add(a.id);
            _progress[a.id] = PlayerPrefs.GetInt("Ach_Prog_" + a.id, 0);
        }
    }
}