using UnityEngine;

public class ChasingProjectile : EnemyProjectile
{
    [Header("Chasing Settings")]
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float chaseDuration = 2.5f;
    
    private Transform _target;
    private float _chaseTimer;

    protected override void OnEnable()
    {
        base.OnEnable();
        _chaseTimer = 0f;
        
        // Tự tìm Player bằng Tag (Hoặc bạn có thể dùng Singleton PlayerController.Instance)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _target = player.transform;
    }

    protected override void FixedUpdate()
    {
        if (!isLaunched || rb == null) return;

        _chaseTimer += Time.fixedDeltaTime;

        // Logic đuổi có giới hạn thời gian và quán tính
        if (_target != null && _chaseTimer < chaseDuration)
        {
            Vector2 targetDir = ((Vector2)_target.position - rb.position).normalized;
            
            // Lấy góc hiện tại từ direction (Vector2) của class cha
            float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

            // Xoay dần hướng (quán tính)
            float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);
            
            // Cập nhật lại direction cho class cha
            direction = new Vector2(Mathf.Cos(nextAngle * Mathf.Deg2Rad), Mathf.Sin(nextAngle * Mathf.Deg2Rad));

            // Xoay Sprite
            transform.rotation = Quaternion.Euler(0, 0, nextAngle);
        }

        // Gọi Move() của cha để thực hiện rb.linearVelocity = direction * speed
        Move();
    }
}
