using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class ExploreJumpEffect : Effect
{
    public float cooldown = 2f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public int bulletDamage = 1;
    
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
        
        // Spawn 2 bullets về 2 phía trái phải
        SpawnBullet(player, Vector2.left);
        SpawnBullet(player, Vector2.right);
    }
    
    private void SpawnBullet(PlayerController player, Vector2 direction)
    {
        if (bulletPrefab == null) return;
        
        Vector3 spawnPosition = player.transform.position;
        GameObject bullet = Object.Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        
        // Set damage nếu có IProjectileDamage
        var projectileDamage = bullet.GetComponent<IProjectileDamage>();
        if (projectileDamage != null)
        {
            projectileDamage.SetDamage(bulletDamage);
        }
    }
}