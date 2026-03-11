using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class SparkJumpEffect : Effect
{
    public float cooldown = 1.5f;
    public GameObject sparkPrefab;
    public float sparkSpeed = 8f;
    public int sparkCount = 5;
    public float sparkInterval = 0.1f;
    public int sparkDamage = 1;
    
    private static float _lastJumpTime = -999f;
    private static bool _isJumping = false;

    public override CursedObjectType EffectType => CursedObjectType.Ability;

    public override void OnJump(PlayerController player)
    {
        // Check cooldown
        if (Time.time - _lastJumpTime < cooldown)
        {
            return;
        }
        
        _lastJumpTime = Time.time;
        _isJumping = true;
        
        // Bắt đầu coroutine spawn spark liên tục
        player.StartCoroutine(SparkRoutine(player));
    }
    
    private System.Collections.IEnumerator SparkRoutine(PlayerController player)
    {
        // Spawn spark liên tục khi player trên không
        while (_isJumping && player != null)
        {
            // Check xem player có trên không không
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.1f)
            {
                SpawnSparks(player);
            }
            else
            {
                // Player đã hạ cánh
                _isJumping = false;
                break;
            }
            
            yield return new WaitForSeconds(sparkInterval);
        }
        
        _isJumping = false;
    }
    
    private void SpawnSparks(PlayerController player)
    {
        if (sparkPrefab == null) return;
        
        // Spawn sparkCount xung quanh player
        for (int i = 0; i < sparkCount; i++)
        {
            // Tạo hướng ngẫu nhiên xuống dưới
            float angle = Random.Range(-45f, -135f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            Vector3 spawnPosition = player.transform.position;
            GameObject spark = Object.Instantiate(sparkPrefab, spawnPosition, Quaternion.identity);
            
            Rigidbody2D rb = spark.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * sparkSpeed;
            }
            
            // Set damage nếu có IProjectileDamage
            var projectileDamage = spark.GetComponent<IProjectileDamage>();
            if (projectileDamage != null)
            {
                projectileDamage.SetDamage(sparkDamage);
            }
        }
    }
}