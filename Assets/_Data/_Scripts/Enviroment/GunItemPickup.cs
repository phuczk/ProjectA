using UnityEngine;
using GlobalEnums;

public class GunItemPickup : Interactable
{
    [SerializeField] private GunType gunType;
    [SerializeField] private string itemId;
    [SerializeField] private Sprite gunIcon;
    
    public GunType GunType => gunType;

    private void Awake()
    {   
        if (PersistentItemManager.Instance != null && PersistentItemManager.Instance.PlayerHasGun(gunType))
        {
            Destroy(gameObject);
        }
    }

    protected override void OnInteract(Transform player)
    {
        var playerController = player?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.UnlockGun(gunType);
            
            if (PersistentItemManager.Instance != null)
            {
                PersistentItemManager.Instance.MarkGunItemCollected(itemId);
            }
            
            Destroy(gameObject);
            ShowInfoUi();
        }
    }

    private void ShowInfoUi()
    {
        InteractableUI.Instance.ShowInteractableInfo(InteractableType.Gun, gunIcon, Localization.Get($"item.gun.{itemId}.name"), Localization.Get($"item.gun.{itemId}.desc"));
    }
}
