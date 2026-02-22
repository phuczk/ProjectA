using UnityEngine;
using UnityEngine.UI;
using GlobalEnums;
using System.Collections.Generic;
using System;

public class GunSelectionUI : MonoBehaviour
{
    [Header("Gun Configuration")]
    [SerializeField] private GunSlotUI currentGunSlot;
    [SerializeField] private GameObject gunSlotPrefab;
    [SerializeField] private Transform gunListContainer;
    [SerializeField] private GameObject gunListPanel;
    [SerializeField] private Button currentGunButton; 
    [SerializeField] private GunConfigSet gunConfigSet;
    
    private List<GunType> _unlockedGuns = new List<GunType>();
    private GunType _currentGun;
    private bool _isGunListVisible = false;

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

    private void ToggleGunList()
    {
        _isGunListVisible = !_isGunListVisible;
        gunListPanel.SetActive(_isGunListVisible);

        if (_isGunListVisible)
        {
            BuildGunList();
        }
    }

    private void BuildGunList()
    {
        foreach (Transform child in gunListContainer)
        {
            Destroy(child.gameObject);
        }

        _unlockedGuns = GetUnlockedGuns();

        foreach (GunType gunType in _unlockedGuns)
        {
            GameObject slotObj = Instantiate(gunSlotPrefab, gunListContainer);
            GunSlotUI slotUI = slotObj.GetComponent<GunSlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Initialize(gunType, GetGunSprite(gunType));
                slotUI.OnGunClicked += HandleGunSelected;
            }
        }
    }

    private void HandleGunSelected(GunType gunType)
    {
        _currentGun = gunType;
        
        RefreshCurrentGunDisplay();
        
        SaveGunToData(gunType);
        
        OnGunChanged?.Invoke(gunType);
        
        _isGunListVisible = false;
        gunListPanel.SetActive(false);
        
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
            var gun = mgr.CurrentData.player.currentGun;
            return gun;
        }

        var saveData = SaveSystemz.Load();
        if (saveData?.player != null)
        {
            var gun = saveData.player.currentGun;
            return gun;
        }

        return GunType.Normal;
    }
    
    /// <summary>
    /// Public method to refresh the UI when data changes
    /// </summary>
    public void RefreshUI()
    {
        _unlockedGuns = GetUnlockedGuns();
        _currentGun = GetCurrentGun();
        RefreshCurrentGunDisplay();
    }
}
