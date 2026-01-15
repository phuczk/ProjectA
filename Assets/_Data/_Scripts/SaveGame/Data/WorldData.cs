using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class WorldSaveData
{
    public string currentSceneName = "";
    public string currentBench = "";
    public List<string> defeatedBossIds = new();
    public List<string> openedDoors = new();
    public List<Enemy> enemies = new();
    public Language language = Language.Vietnamese;
}

[System.Serializable]
public class Enemy
{
    public string id = "";
    public int numDeaths = 0;
}
