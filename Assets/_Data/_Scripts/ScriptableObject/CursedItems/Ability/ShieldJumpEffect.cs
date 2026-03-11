using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class ShieldJumpEffect : Effect
{
    public float cooldown = 3f;
    public GameObject shieldPrefab;
    public float shieldDuration = 5f;
    
    private static float _lastJumpTime = -999f;

    public override CursedObjectType EffectType => CursedObjectType.Ability;

    public override void OnJump(PlayerController player)
    {
        // Check cooldown
        if (Time.time - _lastJumpTime < cooldown)
        {
            return;
        }
        
        _lastJumpTime = Time.time;
        
        // Tạo shield tại vị trí jump
        if (shieldPrefab != null)
        {
            Vector3 spawnPosition = player.transform.position;
            GameObject shield = Object.Instantiate(shieldPrefab, spawnPosition, Quaternion.identity);
            
            // Auto destroy sau duration
            Object.Destroy(shield, shieldDuration);
        }
    }
}