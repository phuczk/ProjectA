using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class EnemyUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("UI Components")]
    public Image enemyImage;

    private EnemyData data;
    private RectTransform _rect;

    public Action<EnemyData, RectTransform> OnItemSelected;
    
    public void SetupEnemyUI(EnemyData enemyData, int deathCount, bool isUnlocked, bool hasSaveData)
    {
        if (enemyData == null) return;

        data = enemyData;
        _rect = GetComponent<RectTransform>();
        
        if (enemyImage != null)
        {
            if (hasSaveData && enemyData.sprite != null)
            {
                enemyImage.sprite = enemyData.sprite;
                enemyImage.color = Color.white;
            }
            else
            {
                enemyImage.sprite = enemyData.sprite;
                enemyImage.color = Color.black;
            }
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        enemyImage.transform.localScale = Vector3.one * 1.1f;
        OnItemSelected?.Invoke(data, _rect);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        enemyImage.transform.localScale = Vector3.one;
    }
}
