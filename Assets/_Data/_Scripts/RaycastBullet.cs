using UnityEngine;

public class RaycastBullet : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Transform _visualChild; // Kéo Sprite con vào đây
    [SerializeField] private float _baseScaleX = 1f;
    [SerializeField] private bool _useSmear = true;

    private float _speed;
    private float _damage;
    private float _lifeTime;
    private float _timer;
    private bool _isPlayerBullet;
    private LayerMask _hitLayers;

    public void Init(Vector2 direction, GunConfig config, bool isPlayerBullet)
    {
        _speed = config.bulletSpeed;
        _damage = config.damage;
        _lifeTime = config.bulletLifetime;
        _isPlayerBullet = isPlayerBullet;
        _timer = 0;

        transform.right = direction;
        _hitLayers = LayerMask.GetMask("Ground", "Enemy", "Player");

        // Đảm bảo Sprite ban đầu ở kích thước chuẩn
        if (_visualChild != null) _visualChild.localScale = new Vector3(_baseScaleX, 1, 1);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _lifeTime) { ReleaseBullet(); return; }

        float distanceThisFrame = _speed * Time.deltaTime;
        Vector2 startPos = transform.position;
        Vector2 direction = transform.right;

        // Bắn Raycast kiểm tra va chạm
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distanceThisFrame, _hitLayers);

        if (hit.collider != null)
        {
            // Hiệu ứng kéo dãn nốt phần cuối trước khi biến mất
            if (_useSmear && _visualChild != null) 
                ApplySmear(Vector2.Distance(startPos, hit.point));

            transform.position = hit.point;
            HandleCollision(hit.collider);
        }
        else
        {
            // Hiệu ứng kéo dãn theo vận tốc (Smear Frame)
            if (_useSmear && _visualChild != null) 
                ApplySmear(distanceThisFrame);

            transform.Translate(Vector3.right * distanceThisFrame);
        }
    }

    private void ApplySmear(float distance)
    {
        // Bí quyết Cuphead: Kéo dài Sprite bằng đúng quãng đường nó vừa đi được
        // Cộng thêm một chút baseScale để không bị mất hình dạng gốc
        float newScaleX = _baseScaleX + (distance * 10f); // Chỉnh hệ số 2f tùy độ dài muốn kéo
        _visualChild.localScale = new Vector3(newScaleX, 1, 1);
        
        // Đẩy sprite lùi lại một nửa quãng đường kéo để đầu đạn luôn ở đúng vị trí thực
        _visualChild.localPosition = new Vector3(-distance * 0.5f, 0, 0);
    }

    private void HandleCollision(Collider2D other)
    {
        // Giữ nguyên logic gây damage của bạn
        if (other.CompareTag("Enemy") && _isPlayerBullet)
        {
            var beh = other.GetComponent<EntityBehaviour>() ?? other.GetComponentInParent<EntityBehaviour>();
            beh?.TakeDamage(Mathf.RoundToInt(_damage));
        }
        ReleaseBullet();
    }

    private void ReleaseBullet() { Destroy(gameObject); }
}