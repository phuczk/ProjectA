using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ManaNodeUI : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _tweenDuration = 0.2f;
    [SerializeField] private float _fullOutlineSize = 1.4f;
    [SerializeField] private float _emptyOutlineSize = 0f;

    private void Awake()
    {
        if (_fillImage.material != null)
        {
            _fillImage.material = new Material(_fillImage.material);
        }
    }

    public void SetFillAmount(float amount, bool animate = true)
    {
        if (!animate)
        {
            _fillImage.fillAmount = amount;
            SetOutlineSize(amount >= 1f);
            return;
        }

        _fillImage.DOKill();
        _fillImage.DOFillAmount(amount, _tweenDuration).SetEase(Ease.OutQuad);

        if (amount >= 1f && _fillImage.fillAmount < 1f)
        {
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }

        SetOutlineSize(amount >= 1f);
    }
    
    private void SetOutlineSize(bool isFull)
    {
        if (_fillImage.material != null)
        {
            float targetSize = isFull ? _fullOutlineSize : _emptyOutlineSize;
            _fillImage.material.SetFloat("_OutlineSize", targetSize);
        }
    }
    
    private void OnDestroy()
    {
        _fillImage?.DOKill();
        transform.DOKill();
        
        if (_fillImage.material != null)
        {
            DestroyImmediate(_fillImage.material);
        }
    }
}
