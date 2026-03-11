using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class SmokeJumpEffect : Effect
{
    public float cooldown = 2.5f;
    public GameObject smokePrefab;
    public float smokeDuration = 3f;
    
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
        
        // Tạo smoke khi jump
        if (smokePrefab != null)
        {
            Vector3 spawnPosition = player.transform.position;
            GameObject smoke = Object.Instantiate(smokePrefab, spawnPosition, Quaternion.identity);
            
            // Auto destroy sau duration
            Object.Destroy(smoke, smokeDuration);
        }
    }
}