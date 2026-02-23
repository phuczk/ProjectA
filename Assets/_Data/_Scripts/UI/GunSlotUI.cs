using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GlobalEnums;
using System;

public class GunSlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image gunIcon;
    [SerializeField] private Color selectedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Màu tối khi được chọn
    
    private Button _button;
    private RectTransform _rect;
    private GunType _gunType;
    
    public GunType GunType => _gunType;
    public Action<GunType> OnGunClicked;
    public Action<GunType, RectTransform> OnGunSelected; // Cho scroll snap

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rect = GetComponent<RectTransform>();

        if (_button != null)
            _button.onClick.AddListener(() => OnGunClicked?.Invoke(_gunType));
    }

    public void Initialize(GunType gunType, Sprite icon)
    {
        _gunType = gunType;
        if (gunIcon != null) 
            gunIcon.sprite = icon;
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        gunIcon.transform.localScale *= 1.1f;
        gunIcon.color = selectedColor;
        OnGunSelected?.Invoke(_gunType, _rect);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        gunIcon.transform.localScale /= 1.1f;
        gunIcon.color = Color.white;
    }
}
