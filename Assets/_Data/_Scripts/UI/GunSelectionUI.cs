using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class GunSelectionUI : MonoBehaviour
{
    [Header("Gun Configuration")]
    [SerializeField] private GunSlotUI currentGunSlot;
    [SerializeField] private GameObject gunSlotPrefab;
    [SerializeField] private Transform gunListContainer;
    [SerializeField] private GameObject gunListPanel;
    [SerializeField] private Button currentGunButton; 
    [SerializeField] private GunConfigSet gunConfigSet;
    
    [Header("Scroll Settings")]
    [SerializeField] private ScrollRect gunListScrollRect;
    [SerializeField] private float scrollSnapDuration = 0.25f;
    
    [Header("UI References")]
    [SerializeField] private GameObject bookUI;
    
    private List<GunType> _unlockedGuns = new List<GunType>();
    private GunType _currentGun;
    private bool _isGunListVisible = false;
    private int _currentGunListIndex = 0;
    private List<GunSlotUI> _gunListSlots = new List<GunSlotUI>();
    private Tween _gunScrollTween;

    public static event Action<GunType> OnGunChanged;

    private void Start()
    {
        if (gunListPanel != null) gunListPanel.SetActive(false);

        _unlockedGuns = GetUnlockedGuns();
        _currentGun = GetCurrentGun();
        
        RefreshCurrentGunDisplay();
        
        if (currentGunButton != null)
        {
            currentGunButton.onClick.AddListener(ToggleGunList);
        }
    }
    
    void Update()
    {
        HandleESCInput();
    }
    
    private void HandleESCInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _isGunListVisible)
        {
            _isGunListVisible = false;
            gunListPanel.SetActive(false);
            
            if (bookUI != null)
            {
                bookUI.SetActive(true);
            }
        }
    }
    
    private void ToggleGunList()
    {
        _isGunListVisible = !_isGunListVisible;
        gunListPanel.SetActive(_isGunListVisible);

        if (_isGunListVisible)
        {
            if (bookUI != null)
            {
                bookUI.SetActive(false);
            }
            
            BuildGunList();
        }
        else
        {
            if (bookUI != null)
            {
                bookUI.SetActive(true);
            }
        }
    }

    private void BuildGunList()
    {
        foreach (Transform child in gunListContainer)
        {
            Destroy(child.gameObject);
        }

        _gunListSlots.Clear();
        _currentGunListIndex = 0;

        _unlockedGuns = GetUnlockedGuns();

        if (gunConfigSet != null && gunSlotPrefab != null && gunListContainer != null)
        {
            foreach (GunType gunType in _unlockedGuns)
            {
                GameObject slotObj = Instantiate(gunSlotPrefab, gunListContainer);
                GunSlotUI slotUI = slotObj.GetComponent<GunSlotUI>();
                
                if (slotUI != null)
                {
                    slotUI.Initialize(gunType, GetGunSprite(gunType));
                    slotUI.OnGunClicked += HandleGunSelected;
                    slotUI.OnGunSelected += HandleGunSlotSelected; 
                    _gunListSlots.Add(slotUI);
                }
            }
        }
    }
    
    private void HandleGunSlotSelected(GunType gunType, RectTransform rect)
    {
        SnapToGun(rect);
    }
    
    private void SnapToGun(Transform target)
    {
        if (gunListScrollRect == null) return;
        
        Canvas.ForceUpdateCanvases();
        RectTransform viewport = gunListScrollRect.viewport;
        RectTransform content = gunListScrollRect.content;

        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
        Bounds viewBounds = new Bounds(viewport.rect.center, viewport.rect.size);

        float offset = 0f;
        if (itemBounds.max.y > viewBounds.max.y) offset = itemBounds.max.y - viewBounds.max.y;
        else if (itemBounds.min.y < viewBounds.min.y) offset = itemBounds.min.y - viewBounds.min.y;

        if (Mathf.Approximately(offset, 0f)) return;

        float targetNormalized = Mathf.Clamp01(gunListScrollRect.verticalNormalizedPosition + (offset / (content.rect.height - viewport.rect.height)));

        _gunScrollTween?.Kill();
        _gunScrollTween = DOTween.To(() => gunListScrollRect.verticalNormalizedPosition, x => gunListScrollRect.verticalNormalizedPosition = x, targetNormalized, scrollSnapDuration)
            .SetEase(Ease.OutCubic).SetUpdate(true);
    }

    private void HandleGunSelected(GunType gunType)
    {
        _currentGun = gunType;
        
        RefreshCurrentGunDisplay();
        
        SaveGunToData(gunType);
        
        OnGunChanged?.Invoke(gunType);
        
        _isGunListVisible = false;
        gunListPanel.SetActive(false);
        
        if (bookUI != null)
        {
            bookUI.SetActive(true);
        }
    }
    
    private void SaveGunToData(GunType gunType)
    {
        var mgr = SaveManager.Instance;
        if (mgr?.CurrentData?.player != null)
        {
            if (!mgr.CurrentData.player.unlockedGuns.Contains(gunType))
            {
                return;
            }
            
            mgr.CurrentData.player.currentGun = gunType;
            mgr.SaveGame();
        }
    }

    private void RefreshCurrentGunDisplay()
    {
        if (currentGunSlot != null)
        {
            var sprite = GetGunSprite(_currentGun);
            currentGunSlot.Initialize(_currentGun, sprite);
        }
    }

    private Sprite GetGunSprite(GunType gunType)
    {
        if (gunConfigSet == null) return null;
        switch (gunType)
        {
            case GunType.Normal: return gunConfigSet.Normal.gunSprite;
            case GunType.Shotgun: return gunConfigSet.Shotgun.gunSprite;
            case GunType.Rapid: return gunConfigSet.Rapid.gunSprite;
            default: return null;
        }
    }

    private List<GunType> GetUnlockedGuns()
    {
        if (SaveManager.Instance?.CurrentData?.player?.unlockedGuns != null)
            return SaveManager.Instance.CurrentData.player.unlockedGuns;

        return new List<GunType> { GunType.Normal };
    }

    private GunType GetCurrentGun()
    {
        var mgr = SaveManager.Instance;
        if (mgr?.CurrentData?.player != null)
        {
            return mgr.CurrentData.player.currentGun;
        }

        return GunType.Normal;
    }
    
    public void RefreshUI()
    {
        _unlockedGuns = GetUnlockedGuns();
        _currentGun = GetCurrentGun();
        RefreshCurrentGunDisplay();
    }
    
    public void FocusOnGunSelection()
    {
        if (!_isGunListVisible)
        {
            ToggleGunList();
        }
        
        if (currentGunSlot != null)
        {
            Debug.Log("Focused on gun selection");
        }
    }
}
