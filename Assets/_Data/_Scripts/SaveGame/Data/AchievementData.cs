using UnityEngine;
using System.Collections.Generic;
using GlobalEnums;

[CreateAssetMenu(menuName = "Achievement/New Achievement")]
public class AchievementListData : ScriptableObject
{
    public List<AchievementData> achievements = new();
}

[System.Serializable]
public class AchievementData
{
    public string id;
    public string titleKey;
    public string descKey;
    public Sprite icon;
    public int goalValue;
}
