using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GlobalEnums;
using System;

public class GunSlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image gunIcon;
    public float GunScale = 1.1f;
    [SerializeField] private Color delectedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public bool isCurrentButton = false;
    public bool isEquipped = false;
    
    private Button _button;
    private RectTransform _rect;
    private GunType _gunType;
    
    public GunType GunType => _gunType;
    public Action<GunType> OnGunClicked;
    public Action<GunType, RectTransform> OnGunSelected;
    private Vector3 OriginalScale;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rect = GetComponent<RectTransform>();

        if (_button != null && PlayerRestState.Instance != null && PlayerRestState.Instance.IsResting && !isCurrentButton)
            _button.onClick.AddListener(() => OnGunClicked?.Invoke(_gunType));
        OriginalScale = gunIcon.transform.localScale;
        if (!isCurrentButton)
        {
            gunIcon.transform.localScale = OriginalScale / GunScale;
            OriginalScale = gunIcon.transform.localScale;
            gunIcon.color = delectedColor;
        }
    }

    public void Initialize(GunType gunType, Sprite icon)
    {
        _gunType = gunType;
        if (gunIcon != null) 
            gunIcon.sprite = icon;
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        gunIcon.transform.localScale = OriginalScale * GunScale;
        gunIcon.color = Color.white;
        OnGunSelected?.Invoke(_gunType, _rect);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        gunIcon.transform.localScale = OriginalScale;
        if (!isCurrentButton) gunIcon.color = delectedColor;
    }
}
