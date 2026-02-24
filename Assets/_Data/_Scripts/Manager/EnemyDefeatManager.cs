using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class EnemyDefeatManager : Singleton<EnemyDefeatManager>
{
    [Header("Enemy Data Reference")]
    public EnemyListData enemyListData;
    
    [Header("UI Reference")]
    public EnemyListUI enemyListUI;
    
    // Event để enemy báo khi chết
    public System.Action<string> OnEnemyDied;
    
    private void OnEnable()
    {
        OnEnemyDied += HandleEnemyDefeat;
    }
    
    private void OnDisable()
    {
        OnEnemyDied -= HandleEnemyDefeat;
    }
    
    // 🔥 XỬ LÝ KHI ENEMY CHẾT
    private void HandleEnemyDefeat(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId)) return;
        
        var saveData = SaveSystemz.Load();
        if (saveData?.world != null)
        {
            saveData.world.OnEnemyDefeated(enemyId, enemyListData);
            SaveSystemz.Save(saveData);
            
            Debug.Log($"Enemy {enemyId} defeated! Total deaths: {GetEnemyDeathCount(enemyId)}");
        }
        
        // 🔥 CẬP NHẬT UI
        if (enemyListUI != null)
        {
            enemyListUI.UpdateEnemyUI();
        }
    }
    
    // 🔥 PUBLIC METHOD ĐỂ ENEMY GỌI
    public static void ReportEnemyDeath(string enemyId)
    {
        Instance?.OnEnemyDied?.Invoke(enemyId);
    }
    
    // 🔥 LẤY SỐ LẦN DEATH CỦA ENEMY
    public int GetEnemyDeathCount(string enemyId)
    {
        var saveData = SaveSystemz.Load();
        if (saveData?.world != null)
        {
            var enemy = saveData.world.enemies.Find(e => e.id == enemyId);
            return enemy?.numDeath ?? 0;
        }
        return 0;
    }
    
    // 🔥 CHECK ENEMY ĐÃ UNLOCK CHƯA
    public bool IsEnemyUnlocked(string enemyId)
    {
        var saveData = SaveSystemz.Load();
        if (saveData?.world != null)
        {
            var enemy = saveData.world.enemies.Find(e => e.id == enemyId);
            return enemy?.isUnlocked ?? false;
        }
        return false;
    }
}
