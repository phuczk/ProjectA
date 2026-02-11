using UnityEngine;
using GlobalEnums;

public class CursedItem : Interactable
{
    public string cursedId;

    protected override void OnInteract(Transform player)
    {
        var playerController = player?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.UnlockCursedObject(cursedId);
            Destroy(gameObject);
        }
    }
}
