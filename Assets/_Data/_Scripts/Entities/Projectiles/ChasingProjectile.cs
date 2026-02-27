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
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _target = player.transform;
    }

    protected override void FixedUpdate()
    {
        if (!isLaunched || rb == null) return;

        _chaseTimer += Time.fixedDeltaTime;

        if (_target != null && _chaseTimer < chaseDuration)
        {
            Vector2 targetDir = ((Vector2)_target.position - rb.position).normalized;
            
            float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

            float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);
            
            direction = new Vector2(Mathf.Cos(nextAngle * Mathf.Deg2Rad), Mathf.Sin(nextAngle * Mathf.Deg2Rad));

            transform.rotation = Quaternion.Euler(0, 0, nextAngle);
        }

        Move();
    }
}
