using UnityEngine;
using GlobalEnums;

public class GunItem : Interactable
{
    public GunType GunType;
    protected override void OnInteract(Transform player)
    {
        var playerController = player?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.UnlockGun(GunType);
            Destroy(gameObject);
        }
    }
}
