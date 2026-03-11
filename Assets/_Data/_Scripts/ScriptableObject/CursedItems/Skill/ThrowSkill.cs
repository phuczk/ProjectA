using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class ThrowSkill : Effect
{
    public int damage;
    public GameObject projectilePrefab;
    public float throwSpeed = 10f;
    public float skillDuration = 0.5f;
    
    [Header("Multi-Throw Settings")]
    public int projectileCount = 1;
    public float throwInterval = 0.2f;
    public ThrowType throwType = ThrowType.Line;
    
    [Header("Spawn Offset")]
    public Vector2 spawnOffset = Vector2.zero;
    
    public override CursedObjectType EffectType => CursedObjectType.Skill;

    public override void OnSkillUsed(PlayerController player, Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            return;
        }
        
        DisablePlayerMovement(player, skillDuration);
        
        Vector2 throwDirection = player.GetSkillDirection(direction);
        
        if (projectileCount == 1)
        {
            ThrowProjectile(player, throwDirection);
        }
        else
        {
            player.StartCoroutine(MultiThrowRoutine(player, throwDirection));
        }
    }
    
    private void ThrowProjectile(PlayerController player, Vector2 direction)
    {
        Vector3 spawnPosition = player.transform.position + (Vector3)spawnOffset;
        GameObject projectile = Object.Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (throwType == ThrowType.Line)
            {
                rb.linearVelocity = direction * throwSpeed;
            }
            else
            {
                Vector2 velocity = CalculateParabolVelocity(direction, throwSpeed);
                rb.linearVelocity = velocity;
            }
        }
        
        var projectileDamage = projectile.GetComponent<IProjectileDamage>();
        if (projectileDamage != null)
        {
            projectileDamage.SetDamage(damage);
        }
    }
    
    private Vector2 CalculateParabolVelocity(Vector2 direction, float speed)
    {
        Vector2 horizontalDir = new Vector2(direction.x, 0).normalized;
        float horizontalSpeed = speed * 0.7f;
        float verticalSpeed = speed * 0.5f;
        
        return new Vector2(horizontalDir.x * horizontalSpeed, verticalSpeed);
    }
    
    private System.Collections.IEnumerator MultiThrowRoutine(PlayerController player, Vector2 direction)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            ThrowProjectile(player, direction);
            
            if (i < projectileCount - 1)
            {
                yield return new WaitForSeconds(throwInterval);
            }
        }
    }
    
    private void DisablePlayerMovement(PlayerController player, float duration)
    {
        player.StartCoroutine(DisableMovementCoroutine(player, duration));
    }
    
    private System.Collections.IEnumerator DisableMovementCoroutine(PlayerController player, float duration)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
        
        var inputHandler = player.GetComponent<PlayerInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.DisableInput();
        }
        
        yield return new WaitForSeconds(duration);
        
        if (rb != null)
        {
            rb.gravityScale = 1f;
        }
        
        if (inputHandler != null)
        {
            inputHandler.EnableInput();
        }
    }
}

public interface IProjectileDamage
{
    void SetDamage(int damage);
}
