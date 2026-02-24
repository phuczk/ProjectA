using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class CursedListUI : MonoBehaviour
{
    public CursedList cursedList;
    public CursedItemUI itemPrefab;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public RectTransform contentRoot;
    public ScrollRect scrollRect;
    
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
    }

    void Refresh()
    {
        _instantiatedItems.Clear();
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystemz.Load();
        List<string> unlockedIds = save?.items?.unlockedCursedObjects ?? new List<string>();
        List<string> equippedIds = save?.player?.currentCursedObjects ?? new List<string>();

        foreach (var cursed in cursedList.CursedObjects)
        {
            if (unlockedIds.Contains(cursed.id))
            {
                var itemUI = Instantiate(itemPrefab, contentRoot);
                
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

    public void RefreshItemStates()
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystemz.Load();
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
        RectTransform content = contentRoot;

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

    IEnumerator ResetScroll()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
