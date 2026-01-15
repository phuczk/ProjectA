using UnityEngine;
using System.Collections.Generic;
using GlobalEnums;

[System.Serializable]
public class AchievementListData
{
    public List<AchievementData> achievements = new();
}

[System.Serializable]
public class AchievementData
{
    public int id;
    public string name;
    public string description;
    public bool isUnlocked = false;
}
