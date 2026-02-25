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
    
    public System.Action<string> OnEnemyDied;
    
    private void OnEnable()
    {
        OnEnemyDied += HandleEnemyDefeat;
    }
    
    private void OnDisable()
    {
        OnEnemyDied -= HandleEnemyDefeat;
    }
    
    private void HandleEnemyDefeat(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId)) return;
        
        var saveData = SaveSystemz.Load();
        if (saveData?.world != null)
        {
            saveData.world.OnEnemyDefeated(enemyId, enemyListData);
            SaveSystemz.Save(saveData);
        }
        
        if (enemyListUI != null)
        {
            enemyListUI.UpdateEnemyUI();
        }
    }
    
    public static void ReportEnemyDeath(string enemyId)
    {
        Instance?.OnEnemyDied?.Invoke(enemyId);
    }
    
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
