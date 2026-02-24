using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Data")]
public class EnemyListData : ScriptableObject
{
    public List<EnemyData> enemies = new();
}

[System.Serializable]
public class EnemyData
{
    public string id = "";
    public string name => $"enemies.{id}.name";
    public Sprite sprite;
    public string description => $"enemies.{id}.desc";
    public float lootPercent = 0f;
    public int numDeathToUnlock = 20;
    public bool isUnlocked = false;
}

[System.Serializable]
public class Enemy
{
    public string id = "";
    public int numDeath = 0;
    public bool isUnlocked = false;
}