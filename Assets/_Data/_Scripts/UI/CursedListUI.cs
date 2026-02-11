using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    private Tween _scrollTween;

    void Start()
    {
        itemName.text = "";
        itemDescription.text = "";
        Refresh();
        StartCoroutine(ResetScroll());
    }

    void Refresh()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var cursed in cursedList.CursedObjects)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Bind(cursed);

            item.OnItemSelected += (data, rect) => {
                ShowInfo(data);
                SnapTo(rect);
            };

            item.OnItemClicked += (data) => {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.EquipCursedObject(data.id);
                }
            };
        }
    }

    private float _targetY;
    private bool _isInitialized;

    private void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        RectTransform viewport = scrollRect.viewport;
        RectTransform content = contentRoot;

        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
        Bounds viewBounds = new Bounds(viewport.rect.center, viewport.rect.size);

        float offset = 0f;

        if (itemBounds.max.y > viewBounds.max.y)
            offset = itemBounds.max.y - viewBounds.max.y;
        else if (itemBounds.min.y < viewBounds.min.y)
            offset = itemBounds.min.y - viewBounds.min.y;

        if (Mathf.Approximately(offset, 0f))
            return;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
            return;

        float normalizedDelta = offset / (contentHeight - viewportHeight);

        float targetNormalized = Mathf.Clamp01(
            scrollRect.verticalNormalizedPosition + normalizedDelta
        );

        _scrollTween?.Kill();
        _scrollTween = DOTween.To(
            () => scrollRect.verticalNormalizedPosition,
            x => scrollRect.verticalNormalizedPosition = x,
            targetNormalized,
            0.25f
        )
        .SetEase(Ease.OutCubic)
        .SetUpdate(true);
    }

    void ShowInfo(CursedObjectData data)
    {
        itemName.text = Localization.Get(data.NameKey);
        itemDescription.text = Localization.Get(data.DescKey);
    }

    IEnumerator ResetScroll()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
