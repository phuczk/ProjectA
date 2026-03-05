using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using System;
using GlobalEnums;

public class CursedListUI : MonoBehaviour
{
    public CursedList cursedList;
    public CursedItemUI itemPrefab;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public ScrollRect scrollRect;
    public RectTransform contentRoot;

    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private TextMeshProUGUI passiveText;

    [Header("Type Lists")]
    [SerializeField] private RectTransform SkillList;
    [SerializeField] private RectTransform AbilityList;
    [SerializeField] private RectTransform PassiveList;
    
    [Header("Notch Connection")]
    [SerializeField] private CursedNotchManager notchManager;
    private List<CursedItemUI> _instantiatedItems = new List<CursedItemUI>();
    private Tween _scrollTween;

    void Start()
    {
        itemName.text = "";
        itemDescription.text = "";
        Refresh();
        UpdateNotchUI();
        StartCoroutine(ResetScroll());
        
        CursedNotchUI.OnNotchClicked += HandleNotchClicked;
        
        if (skillText != null)
            skillText.text = Localization.Get("ui.cursed.skill");
        if (abilityText != null)
            abilityText.text = Localization.Get("ui.cursed.ability");
        if (passiveText != null)
            passiveText.text = Localization.Get("ui.cursed.passive");
    }
    
    private void OnDestroy()
    {
        CursedNotchUI.OnNotchClicked -= HandleNotchClicked;
    }
    
    private void HandleNotchClicked(string itemId)
    {
        try
        {
            UpdateNotchUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error handling notch click: {e.Message}");
        }
    }

    void Refresh()
    {
        _instantiatedItems.Clear();
        
        ClearList(SkillList);
        ClearList(AbilityList);
        ClearList(PassiveList);

        var save = SaveManager.Instance?.CurrentData;
        List<string> unlockedIds = save?.items?.unlockedCursedObjects ?? new List<string>();
        List<string> equippedIds = save?.player?.currentCursedObjects ?? new List<string>();

        foreach (var cursed in cursedList.CursedObjects)
        {
            if (unlockedIds.Contains(cursed.id))
            {
                RectTransform targetList = GetListByType(cursed.type);
                
                var itemUI = Instantiate(itemPrefab, targetList);
                
                bool isEquipped = equippedIds.Contains(cursed.id);
                itemUI.Bind(cursed, isEquipped);

                itemUI.OnItemSelected += (data, rect) => {
                    ShowInfo(data);
                    SnapTo(rect);
                };

                itemUI.OnItemClicked += (data) => {
                    var player = FindFirstObjectByType<PlayerController>();
                    if (player != null)
                    {
                        player.EquipCursedObject(data.id);
                        UpdateNotchUI();
                        RefreshItemStates();
                    }
                };
                
                _instantiatedItems.Add(itemUI);
            }
        }
    }

    private RectTransform GetListByType(CursedObjectType type)
    {
        switch (type)
        {
            case CursedObjectType.Skill:
                return SkillList;
            case CursedObjectType.Ability:
                return AbilityList;
            case CursedObjectType.Passive:
                return PassiveList;
            default:
                return SkillList;
        }
    }

    private void ClearList(RectTransform listRoot)
    {
        if (listRoot != null)
        {
            foreach (Transform child in listRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void RefreshItemStates()
    {
        var save = SaveManager.Instance?.CurrentData;
        List<string> equippedIds = save?.player?.currentCursedObjects ?? new List<string>();

        foreach (var itemUI in _instantiatedItems)
        {
            bool isEquipped = equippedIds.Contains(itemUI.ItemId);
            itemUI.Bind(cursedList.GetById(itemUI.ItemId), isEquipped);
        }
    }

    public void UpdateNotchUI()
    {
        if (notchManager != null)
        {
            notchManager.RefreshNotchDisplay();
            RefreshItemStates();
        }
    }

    void ShowInfo(CursedObjectData data)
    {
        itemName.text = Localization.Get(data.NameKey);
        itemDescription.text = Localization.Get(data.DescKey);
    }

    private void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();
        RectTransform viewport = scrollRect.viewport;
        RectTransform contentRoot = this.contentRoot;

        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
        Bounds viewBounds = new Bounds(viewport.rect.center, viewport.rect.size);

        float topPadding = 0f;
        Transform parentList = target.parent;
        
        if (parentList != null)
        {
            int listIndex = parentList.GetSiblingIndex();
            if (listIndex > 0)
            {
                Transform titleElement = contentRoot.GetChild(listIndex - 1);
                if (titleElement != null)
                {
                    RectTransform titleRect = titleElement.GetComponent<RectTransform>();
                    if (titleRect != null)
                    {
                        topPadding = titleRect.rect.height + 10f;
                    }
                }
            }
        }

        float offset = 0f;
        
        if (itemBounds.max.y > viewBounds.max.y - topPadding) 
            offset = itemBounds.max.y - (viewBounds.max.y - topPadding);
        else if (itemBounds.min.y < viewBounds.min.y) 
            offset = itemBounds.min.y - viewBounds.min.y;

        if (Mathf.Approximately(offset, 0f)) return;

        float targetNormalized = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + (offset / (contentRoot.rect.height - viewport.rect.height)));

        _scrollTween?.Kill();
        _scrollTween = DOTween.To(() => scrollRect.verticalNormalizedPosition, x => scrollRect.verticalNormalizedPosition = x, targetNormalized, 0.25f)
            .SetEase(Ease.OutCubic).SetUpdate(true);
    }

    IEnumerator ResetScroll()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
