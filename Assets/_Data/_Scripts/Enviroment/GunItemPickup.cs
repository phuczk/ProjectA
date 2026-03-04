using UnityEngine;
using GlobalEnums;

public class GunItemPickup : Interactable
{
    [SerializeField] private GunType gunType;
    [SerializeField] private string itemId;
    
    public GunType GunType => gunType;

    private void Awake()
    {
        if (PersistentItemManager.Instance != null && PersistentItemManager.Instance.IsGunItemCollected(itemId))
        {
            Destroy(gameObject);
            return;
        }
        
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
        }
    }
}
