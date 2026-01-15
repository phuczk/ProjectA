using UnityEngine;

public class BulletController : MonoBehaviour
{
    public GameObject _hitEffectPrefab;
    public GameObject _destroyEffectPrefab;
    private float _life;
    private float _spawnTime;
    private bool _isActive;
    private float _damage;
    private float _manaGain;
    private bool _isPlayerBullet;
    private float _speed;
    public Vector3 _direction;


    private PlayerHealth playerHealth;
    private Rigidbody2D _rb;

    private Vector3 _inheritedVel;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _rb.bodyType = RigidbodyType2D.Dynamic; 
        _rb.gravityScale = 0f;
        
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.None;
    }

    public void Init(Vector2 velocity, GunConfig config, bool isPlayerBullet, Vector2 playerVelocity)
    {
        // 1. Gán thông số
        _life = config.bulletLifetime;
        _damage = config.damage;
        _manaGain = config.manaGain;
        _spawnTime = Time.time;
        _isPlayerBullet = isPlayerBullet;
        _isActive = true;

        _speed = config.bulletSpeed;
        _direction = new Vector3(velocity.x, velocity.y, 0);

        _inheritedVel = (Vector3)playerVelocity;

        float ang = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, ang);

        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void Update()
    {
        if (!_isActive) return;

        if (Time.time > _spawnTime + _life)
        {
            ReleaseBullet();
            if (_destroyEffectPrefab != null)
            {
                _destroyEffectPrefab.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, 1);
                BulletPool.Instance.Get(_destroyEffectPrefab.gameObject, transform.position, Quaternion.identity);
            }
            return;
        }

        transform.position += (_direction * _speed + _inheritedVel) * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Ground") || other.CompareTag("Enemy")) && _isPlayerBullet)
        {
            if (other.CompareTag("Enemy"))
            {
                if (playerHealth == null) return;
                var beh = other.GetComponent<EntityBehaviour>();
                if (beh == null) beh = other.GetComponentInParent<EntityBehaviour>();
                if (beh != null)
                {
                    if (_hitEffectPrefab != null) // Bạn nên kéo Prefab hiệu ứng riêng vào đây
                    {
                        BulletPool.Instance.Get(_hitEffectPrefab.gameObject, transform.position, Quaternion.identity);
                    }   
                    int dmg = Mathf.Max(0, Mathf.RoundToInt(_damage));
                    beh.TakeDamage(dmg);
                    playerHealth.GainMana(_manaGain);
                }
            }
            ReleaseBullet();
        }else if (other.CompareTag("Player") && !_isPlayerBullet)
        {
            ReleaseBullet();
            playerHealth.TakeDamage(Mathf.Max(0, Mathf.RoundToInt(_damage)));
        }
    }

    private void ReleaseBullet()
    {
        _isActive = false;
        var pool = BulletPool.Instance;
        if (pool != null) pool.Release(gameObject);
        else Destroy(gameObject);
    }
}
