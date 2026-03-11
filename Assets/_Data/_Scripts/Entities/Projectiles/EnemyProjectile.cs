using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected float lifetime = 1f;

    protected Rigidbody2D rb;
    protected Vector2 direction;
    protected bool isLaunched = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
    }

    protected virtual void FixedUpdate() 
    {
        if (isLaunched && rb != null) Move();
    }

    protected virtual void OnEnable()
    {
        Invoke(nameof(ReturnToPool), lifetime);
    }

    protected virtual void OnDisable()
    {
        CancelInvoke();
        isLaunched = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public virtual void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        isLaunched = true;
        RotateToDirection();
    }

    protected virtual void RotateToDirection()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    protected virtual void Move()
    {
        rb.linearVelocity = direction * speed;
    }

    protected void ReturnToPool()
    {
        if (gameObject.activeSelf)
        {
            BulletPool.Instance.Release(gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null && health.isNoTakeDamageTime)
            {
                return;
            }
            
            health?.TakeDamage(damage);
            ReturnToPool();
        }
        else if (other.CompareTag("Ground"))
        {
            ReturnToPool();
        }
    }
}
