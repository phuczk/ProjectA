using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthNodeUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite _fullSprite;
    [SerializeField] private Sprite _emptySprite;

    public void SetFull(bool full, bool animate = true)
    {
        _icon.sprite = full ? _fullSprite : _emptySprite;

        if (!animate) return;

        _icon.transform.DOKill();
        _icon.transform.localScale = Vector3.one * 0.8f;
        _icon.transform
            .DOScale(1f, 0.15f)
            .SetEase(Ease.OutBack);
    }
}
