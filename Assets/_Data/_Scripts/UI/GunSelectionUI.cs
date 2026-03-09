using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class GunSelectionUI : MonoBehaviour, IBackHandler
{
    [Header("Gun Configuration")]
    [SerializeField] private GunSlotUI currentGunSlot;
    [SerializeField] private GameObject gunSlotPrefab;
    [SerializeField] private RectTransform gunListContainer;
    [SerializeField] private GameObject gunListPanel;
    [SerializeField] private Button currentGunButton; 
    [SerializeField] private GunConfigSet gunConfigSet;
    
    [Header("Scroll Settings")]
    [SerializeField] private RectTransform view;
    [SerializeField] private float scrollSnapDuration = 0.25f;
    
    [Header("UI References")]
    [SerializeField] private GameObject bookUI;
    [SerializeField] private GameStateChannel stateChannel;
    
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
        
        if (currentGunButton != null && PlayerRestState.Instance != null && PlayerRestState.Instance.IsResting)
        {
            currentGunButton.onClick.AddListener(ToggleGunList);
        }
    }
    
    private void ToggleGunList()
    {
        if (PlayerRestState.Instance != null && !PlayerRestState.Instance.IsResting)
        {
            ShowRestRequiredPanel();
            return;
        }
        
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
                    slotUI.OnGunSelected += (gunType, rect) => {
                        SnapToGun(rect);
                    }; 
                    _gunListSlots.Add(slotUI);
                }
            }
        }
    }
    
    private void SnapToGun(RectTransform target)
    {
        if (view == null || gunListContainer == null) return;
        if (target == null) return;
        
        Vector3 viewCenter = view.position;
        Vector3 itemCenter = target.position;
        
        float offsetX = viewCenter.x - itemCenter.x;
        
        Vector3 currentContainerPos = gunListContainer.position;
        Vector3 targetContainerPos = new Vector3(currentContainerPos.x + offsetX, currentContainerPos.y, currentContainerPos.z);
        
        _gunScrollTween?.Kill();
        _gunScrollTween = gunListContainer.DOMove(targetContainerPos, scrollSnapDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
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
        if (PlayerRestState.Instance != null && !PlayerRestState.Instance.IsResting)
        {
            ShowRestRequiredPanel();
            return;
        }
        
        if (!_isGunListVisible)
        {
            ToggleGunList();
        }
    }
    
    public bool OnBack()
    {
        if (_isGunListVisible)
        {
            _isGunListVisible = false;
            gunListPanel.SetActive(false);
            
            if (bookUI != null)
            {
                bookUI.SetActive(true);
            }
            
            return true;
        }
        
        return false;
    }

    private void ShowRestRequiredPanel()
    {
        var restPanel = FindAnyObjectByType<RestRequiredPanel>();
        if (restPanel != null)
        {
            restPanel.ShowPanel();
        }
    }
}
