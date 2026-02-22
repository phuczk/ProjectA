using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System;

public class GunSlotUI : MonoBehaviour
{
    [SerializeField] private Image gunIcon;
    [SerializeField] private Button button;
    
    private GunType _gunType;
    public GunType GunType => _gunType;
    public event Action<GunType> OnGunClicked;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(() => OnGunClicked?.Invoke(_gunType));
    }

    public void Initialize(GunType gunType, Sprite icon)
    {
        _gunType = gunType;
        if (gunIcon != null) gunIcon.sprite = icon;
    }
}
