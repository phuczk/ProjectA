using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ManaNodeUI : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _tweenDuration = 0.2f;

    public void SetFillAmount(float amount, bool animate = true)
    {
        if (!animate)
        {
            _fillImage.fillAmount = amount;
            return;
        }

        _fillImage.DOKill();
        _fillImage.DOFillAmount(amount, _tweenDuration).SetEase(Ease.OutQuad);

        if (amount >= 1f && _fillImage.fillAmount < 1f)
        {
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }
    }
    
    private void OnDestroy()
    {
        _fillImage?.DOKill();
        transform.DOKill();
    }
}