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
            playerController.EquipCursedObject(cursedId);
            Destroy(gameObject);
        }
    }
}
