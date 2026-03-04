using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyListUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject enemyEntryPrefab;
    public RectTransform enemyListContainer;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI enemyDesText;
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Header("Scroll References")]
    public ScrollRect scrollRect;
    
    [Header("Enemy Data Reference")]
    public EnemyListData enemyListData;
    
    private Dictionary<string, GameObject> _enemyUIEntries = new Dictionary<string, GameObject>();
    private Tween _scrollTween;
    
    private void Start()
    {
        if (enemyListData != null)
        {
            UpdateEnemyUI();
        }
    }
    
    public void UpdateEnemyUI()
    {
        if (enemyListData == null || enemyListContainer == null || enemyEntryPrefab == null) return;
        
        foreach (var entry in _enemyUIEntries.Values)
        {
            if (entry != null) Destroy(entry);
        }
        _enemyUIEntries.Clear();
        
        var saveData = SaveSystem.Load();
        
        foreach (var enemyData in enemyListData.enemies)
        {
            Enemy savedEnemy = null;
            bool hasSaveData = false;
            
            if (saveData?.world != null)
            {
                savedEnemy = saveData.world.enemies.Find(e => e.id == enemyData.id);
                hasSaveData = savedEnemy != null;
            }
            
            int deathCount = savedEnemy?.numDeath ?? 0;
            bool isUnlocked = savedEnemy?.isUnlocked ?? false;
            
            CreateEnemyEntry(enemyData, deathCount, isUnlocked, hasSaveData);
        }
    }
    
    private void CreateEnemyEntry(EnemyData enemyData, int deathCount, bool isUnlocked, bool hasSaveData)
    {
        if (string.IsNullOrEmpty(enemyData.id)) return;
        
        GameObject entry = Instantiate(enemyEntryPrefab, enemyListContainer);
        _enemyUIEntries[enemyData.id] = entry;
        
        EnemyUI enemyUI = entry.GetComponent<EnemyUI>();
        if (enemyUI == null)
        {
            return;
        }
        
        enemyUI.OnItemSelected += (data, rect) => {
            ShowInfo(data);
            SnapTo(rect);
        };
        
        enemyUI.SetupEnemyUI(enemyData, deathCount, isUnlocked, hasSaveData);
    }
    
    public int GetEnemyDeathCount(string enemyId)
    {
        var saveData = SaveSystem.Load();
        if (saveData?.world != null)
        {
            var enemy = saveData.world.enemies.Find(e => e.id == enemyId);
            return enemy?.numDeath ?? 0;
        }
        return 0;
    }
    
    public bool IsEnemyUnlocked(string enemyId)
    {
        var saveData = SaveSystem.Load();
        if (saveData?.world != null)
        {
            var enemy = saveData.world.enemies.Find(e => e.id == enemyId);
            return enemy?.isUnlocked ?? false;
        }
        return false;
    }

    private void ShowInfo(EnemyData data)
    {
        var saveData = SaveSystem.Load();
        Enemy savedEnemy = null;
        bool hasSaveData = false;
        
        if (saveData?.world != null)
        {
            savedEnemy = saveData.world.enemies.Find(e => e.id == data.id);
            hasSaveData = savedEnemy != null;
        }
        
        if (hasSaveData)
        {
            enemyNameText.text = Localization.Get(data.name);
            enemyDesText.text = Localization.Get(data.description);
            
            int deathCount = savedEnemy?.numDeath ?? 0;
            bool isUnlocked = savedEnemy?.isUnlocked ?? false;
            
            if (isUnlocked)
            {
                progressText.text = "✓ UNLOCKED";
                progressText.color = Color.green;
            }
            else
            {
                progressText.text = $"{deathCount}/{data.numDeathToUnlock}";
                progressText.color = deathCount >= data.numDeathToUnlock ? Color.yellow : Color.white;
            }
        }
        else
        {
            enemyNameText.text = "???";
            enemyDesText.text = "???";
            progressText.text = "??? / ???";
        }
    }
    
    private void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = enemyListContainer;

        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
        Bounds viewBounds = new Bounds(viewport.rect.center, viewport.rect.size);

        float offset = 0f;
        if (itemBounds.max.y > viewBounds.max.y) offset = itemBounds.max.y - viewBounds.max.y;
        else if (itemBounds.min.y < viewBounds.min.y) offset = itemBounds.min.y - viewBounds.min.y;

        if (Mathf.Approximately(offset, 0f)) return;

        float targetNormalized = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + (offset / (content.rect.height - viewport.rect.height)));

        _scrollTween?.Kill();
        _scrollTween = DOTween.To(() => scrollRect.verticalNormalizedPosition, x => scrollRect.verticalNormalizedPosition = x, targetNormalized, 0.25f)
            .SetEase(Ease.OutCubic).SetUpdate(true);
    }
}
