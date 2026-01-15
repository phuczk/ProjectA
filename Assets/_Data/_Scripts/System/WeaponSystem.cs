using UnityEngine;
using GlobalEnums;
using DG.Tweening;

public class WeaponSystem : MonoBehaviour, ISoundEmitter
{
    [Header("Configurations")]
    [SerializeField] private GunConfigSet _gunSet;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _arm;
    [SerializeField] private float _armAngleOffset = 0f;
    [SerializeField] private float _armIdleDelay = 0.5f;

    [Header("References")]
    private GameObject _visuals;
    private GravityFlipManager _manager;
    private Camera _cachedMainCam;
    private PlayerController _cachedPlayerController;

    private GunType _currentGunType = GunType.Normal;
    private float _lastFireTime;
    private Quaternion _armDefaultRotation;
    private bool _isPlayerWeapon;

    // Events
    public event System.Action<PlayerController, Vector2> OnFireTriggered;
    public event System.Action<PlayerSoundType, AudioClip> OnRequestSound;

    private void Awake()
    {
        if (_arm != null) _armDefaultRotation = _arm.localRotation;
        
        _cachedMainCam = Camera.main;
        
        _cachedPlayerController = GetComponent<PlayerController>();
    }

    public void Configure(GunConfigSet gunSet, Transform firePoint, Transform arm, GameObject visuals, 
        float armAngleOffset = 0f, float armIdleDelay = 0.5f, GravityFlipManager manager = null, bool isPlayerWeapon = false)
    {
        if (gunSet != null) _gunSet = gunSet;
        if (firePoint != null) _firePoint = firePoint;
        if (arm != null)
        {
            _arm = arm;
            _armDefaultRotation = _arm.localRotation;
        }
        if (visuals != null) _visuals = visuals;
        
        _armAngleOffset = armAngleOffset;
        _armIdleDelay = Mathf.Max(0.1f, armIdleDelay);
        _manager = manager;
        _isPlayerWeapon = isPlayerWeapon;

        if (_cachedPlayerController == null) _cachedPlayerController = GetComponent<PlayerController>();
    }

    public void SetGunType(GunType type) => _currentGunType = type;

    public void RequestFire(Vector2 direction, Vector2 ownerVelocity, Vector2 playerVelocity)
    {
        var config = GetConfig();
        if (Time.time < _lastFireTime + config.cooldown) return;

        _lastFireTime = Time.time;
        UpdateArmRotation(direction);
        ExecuteShot(direction, ownerVelocity, config);
    }

    private void ExecuteShot(Vector2 dir, Vector2 ownerVel, GunConfig config)
    {
        int count = Mathf.Max(1, config.bulletCount);
        float spread = config.spreadAngle;

        for (int i = 0; i < count; i++)
        {
            float angle = (count > 1) ? Mathf.Lerp(-spread * 0.5f, spread * 0.5f, (float)i / (count - 1)) : 0;
            Vector2 finalDir = Quaternion.Euler(0, 0, angle) * dir.normalized;
            SpawnBullet(finalDir, ownerVel, config);
        }

        if (config.shakeCameraAmount > 0)
        {
            Camera targetCam = (_manager != null && _manager.cameraController != null && _manager.cameraController.cam != null) 
                ? _manager.cameraController.cam 
                : _cachedMainCam;

            if (targetCam != null)
            {
                targetCam.transform.DOKill(true);
                targetCam.transform.DOShakeRotation(0.1f, config.shakeCameraAmount, 14, 90f);
            }
        }

        OnRequestSound?.Invoke(PlayerSoundType.None, config.shootSound);
        
        OnFireTriggered?.Invoke(_cachedPlayerController, dir);
    }

    private void SpawnBullet(Vector2 dir, Vector2 ownerVel, GunConfig config)
    {
        Transform fp = _firePoint ?? transform;
        
        GameObject bulletObj = BulletPool.Instance != null 
            ? BulletPool.Instance.Get(config.bulletPrefab, fp.position, Quaternion.identity) 
            : Instantiate(config.bulletPrefab, fp.position, Quaternion.identity);

        if (bulletObj.TryGetComponent(out BulletController bc))
        {
            Vector2 bulletVel = dir;
            bc.Init(bulletVel, config, _isPlayerWeapon, ownerVel);
        }
    }

    public void UpdateArmRotation(Vector2 dir)
    {
        if (_arm == null || _visuals == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _arm.DOKill(); // Ngắt Tween idle khi đang bắn/xoay tay

        bool isFacingRight = _visuals.transform.localScale.x > 0;
        float finalAngle = isFacingRight ? (angle + _armAngleOffset) : (angle + 180f - _armAngleOffset);

        _arm.rotation = Quaternion.Euler(0, 0, finalAngle);
    }

    public void HandleIdleArm()
    {
        if (Time.time > _lastFireTime + _armIdleDelay)
        {
            _arm.DOLocalRotateQuaternion(_armDefaultRotation, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    private GunConfig GetConfig() => _currentGunType switch {
        GunType.Shotgun => _gunSet.Shotgun,
        GunType.Rapid => _gunSet.Rapid,
        GunType.Sniper => _gunSet.Sniper,
        _ => _gunSet.Normal
    };
}