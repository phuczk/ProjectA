using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class WorldSaveData : ISerializationCallbackReceiver
{
    public string currentSceneName = "";
    public string currentBench = "";
    public List<string> defeatedBossIds = new();
    public List<string> openedDoors = new();
    public List<Enemy> enemies = new();
    public Language language = Language.Vietnamese;

    [System.NonSerialized] public HashSet<string> visitedRooms = new();
    
    [SerializeField] 
    [UnityEngine.Serialization.FormerlySerializedAs("visitedRooms")]
    private List<string> _visitedRoomsList = new();

    public List<AreaType> unlockedMaps = new();

    // 🔥 NEW: Track collected items in world
    public List<string> collectedAbilityItems = new();
    public List<string> collectedCursedItems = new();
    public List<string> collectedGunItems = new();

    public void OnBeforeSerialize()
    {
        _visitedRoomsList.Clear();
        _visitedRoomsList.AddRange(visitedRooms);
    }

    public void OnAfterDeserialize()
    {
        visitedRooms = new HashSet<string>(_visitedRoomsList);
    }
    
    // 🔥 XỬ LÝ ENEMY DEFEAT
    public void OnEnemyDefeated(string enemyId, EnemyListData enemyListData)
    {
        // Tìm enemy trong list
        Enemy enemy = enemies.Find(e => e.id == enemyId);
        
        if (enemy == null)
        {
            // Nếu chưa có, tạo mới
            enemy = new Enemy { id = enemyId, numDeath = 0, isUnlocked = false };
            enemies.Add(enemy);
        }
        
        // Tăng số lần defeat
        enemy.numDeath++;
        
        // Tìm EnemyData để check unlock
        EnemyData enemyData = enemyListData?.enemies?.Find(e => e.id == enemyId);
        
        if (enemyData != null && enemy.numDeath >= enemyData.numDeathToUnlock)
        {
            enemy.isUnlocked = true;
            enemyData.isUnlocked = true;
        }
    }
    
    // 🔥 CHECK ENEMY ĐÃ UNLOCK CHƯA
    public bool IsEnemyUnlocked(string enemyId)
    {
        Enemy enemy = enemies.Find(e => e.id == enemyId);
        return enemy?.isUnlocked ?? false;
    }
}
