using UnityEngine;
using GlobalEnums;

public class CursedItemPickup : Interactable
{
    [SerializeField] private string cursedId;
    [SerializeField] private string itemId;
    
    public string CursedId => cursedId;

    private void Awake()
    {
        if (PersistentItemManager.Instance != null && PersistentItemManager.Instance.IsCursedItemCollected(itemId))
        {
            Destroy(gameObject);
            return;
        }
        
        if (PersistentItemManager.Instance != null && PersistentItemManager.Instance.PlayerHasCursedItem(cursedId))
        {
            Destroy(gameObject);
        }
    }

    protected override void OnInteract(Transform player)
    {
        var cursedObject = player?.GetComponent<PlayerCursedObject>();
        if (cursedObject != null)
        {
            cursedObject.UnlockCursedObject(cursedId);
            
            if (PersistentItemManager.Instance != null)
            {
                PersistentItemManager.Instance.MarkCursedItemCollected(itemId);
            }
            
            Destroy(gameObject);
        }
    }
}
