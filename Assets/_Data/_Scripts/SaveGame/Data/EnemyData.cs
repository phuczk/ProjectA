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
    public string name = "";
    public Sprite sprite;
    public string description = "";
    public float lootPercent = 0f;
}