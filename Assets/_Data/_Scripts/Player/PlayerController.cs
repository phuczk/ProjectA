using System;
using UnityEngine;
using DG.Tweening;
using GlobalEnums;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayerController
{
    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private GravityFlipManager _manager;
    [SerializeField] private readonly float _dashSpeed = 28f;
    [SerializeField] private readonly float _dashDuration = 0.15f;
    [SerializeField] private readonly float _dashCooldownTime = 0.6f;
    [SerializeField] private GlobalEnums.GunType _currentGunType = GlobalEnums.GunType.Normal;
    [SerializeField] private GunConfigSet _gunSet;
    private Rigidbody2D _rb;
    private Collider2D _col;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private bool _waitingForCamera;
    private Vector2 _pendingGravity;
    private Quaternion _armDefaultRotation;
    [SerializeField] private GameObject _visuals;
    [SerializeField] private Transform _firePoint;
    [SerializeField] public Transform Arm;
    [SerializeField] private float _armAngleOffset = 0f;
    [SerializeField] private WeaponSystem _weaponSystem;

    private Vector2 _lastAimDir;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    [SerializeField] private float _armIdleDelay = 0.5f;

    #endregion
    private float _time;
    [SerializeField] private PlayerAbility _ability;
    [SerializeField] private PlayerCursedObject _cursedObject;
    [SerializeField] private PlayerHealth _health;
    public PlayerHealth Health => _health;
    [SerializeField] private PlayerMotor _motor;
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerGravityFlip _gravityFlip;
    [SerializeField] private PlayerSkill _playerSkill;
    [SerializeField] private PlayerUI _playerUI;
    private bool _dashPressedFrame;

    [Header("Cursed System")]
    [SerializeField] private CursedList cursedList;
    private readonly List<Effect> activeEffects = new();

    private Effect currentSkillEffect;
    private Effect currentUltimateEffect;

    private bool _isHealing;

    private void Awake()
    {
        if (FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        CacheComponents();
        if (_stats == null) _stats = ScriptableObject.CreateInstance<ScriptableStats>();
       
        _motor.Configure(_rb, _col, _stats, _visuals, _ability);

        if (_manager == null) _manager = FindFirstObjectByType<GravityFlipManager>();

        if (_manager != null && _manager.cameraController != null)
        {
            _manager.cameraController.OnCameraRotationComplete += OnCameraRotationComplete;
        }
        if (Arm != null)
            _armDefaultRotation = Arm.localRotation;
        EnsureGunDefaults();
        SetupSystems();
        
        LoadActiveCursedObjects();
    }

    private void CacheComponents()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        
        _ability = GetOrAdd<PlayerAbility>();
        _health = GetOrAdd<PlayerHealth>();
        _motor = GetOrAdd<PlayerMotor>();
        _inputHandler = GetOrAdd<PlayerInputHandler>();
        _gravityFlip = GetOrAdd<PlayerGravityFlip>();
        _playerSkill = GetOrAdd<PlayerSkill>();
        _playerUI = GetOrAdd<PlayerUI>();
        _weaponSystem = GetComponent<WeaponSystem>();
    }

    private T GetOrAdd<T>() where T : Component => GetComponent<T>() ?? gameObject.AddComponent<T>();

    private void SetupSystems()
    {
        _gravityFlip.Configure(_manager, _rb, _ability);
        _motor.Configure(_rb, _col, _stats, _visuals, _ability);
        _motor.OnGroundedChanged = (g, v) => GroundedChanged?.Invoke(g, v);
        _motor.OnJumped = () => Jumped?.Invoke();

        if (_weaponSystem != null)
        {
            _weaponSystem.Configure(_gunSet, _firePoint, Arm, _visuals, _armAngleOffset, _armIdleDelay, _manager, true);
            _weaponSystem.SetGunType(_currentGunType);
            _weaponSystem.OnFireTriggered += HandleGunFireEffect;
        }
        
        _playerSkill.Configure(_ability, _health);
        LoadActiveCursedObjects();
    }

    private void OnDestroy()
    {
        if (_weaponSystem != null)
        {
            _weaponSystem.OnFireTriggered -= HandleGunFireEffect;
        }
    }

    private void HandleGunFireEffect(PlayerController player, Vector2 direction)
    {
        activeEffects.ForEach(effect => effect.OnGunFire(this, direction));
    }

    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            Move       = _inputHandler.MoveInput,
            JumpDown   = _inputHandler.JumpDown,
            JumpHeld   = _inputHandler.JumpHeld,
            DashDown   = _inputHandler.DashDown,
            FireHeld   = _inputHandler.FireHeld,
            HealDown   = _inputHandler.HealDown,
            ScaleBig   = _inputHandler.ScaleBig(),
            ScaleSmall = _inputHandler.ScaleSmall(),
            ScaleNormal= _inputHandler.ScaleNormal(),
            SpecialDown= _inputHandler.SkillInput,
        };

        if (_inputHandler.TryGetGunSwitch(out var gunType))
        {
            SetGunType(gunType);
        }
    }

    private void Update()
    {
        if (_isHealing) return;
        _time += Time.deltaTime;

        _gravityFlip.HandleInput(_inputHandler, transform, ref _waitingForCamera, ref _pendingGravity);

        GatherInput();

        _motor.SetJumpInput(_frameInput.JumpDown, _frameInput.JumpHeld, _time);

        _dashPressedFrame |= _frameInput.DashDown;

        if (_frameInput.FireHeld) TryFire(_inputHandler.AimInput);
        if (_frameInput.SpecialDown) TrySpecialSkill(_frameInput.Move);
        if (_frameInput.HealDown) HealPlayer();

        if (_frameInput.ScaleBig) ScalePlayer(ScaleType.Big);
        else if (_frameInput.ScaleSmall) ScalePlayer(ScaleType.Small);
        else if (_frameInput.ScaleNormal) ScalePlayer(ScaleType.Normal);
        
        if (_frameInput.DashDown) _motor.TryStartDash(_time, _dashSpeed, _dashDuration, _dashCooldownTime);

        HandleArmIdleReturn();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector2 up = GetUpDir();
        Vector3 origin = transform.position;
        Vector3 end = origin + (Vector3)up * 1.2f;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, end);

        Vector3 right = Quaternion.Euler(0, 0, 20) * -up * 0.3f;
        Vector3 left  = Quaternion.Euler(0, 0, -20) * -up * 0.3f;
        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
    }
    #endif

    private void FixedUpdate()
    {
        if (_waitingForCamera || _isHealing) return;
        _motor.CheckCollisions(_time);
        //_motor.HandleJump(_time);
        //_motor.HandleDirection(_inputHandler.MoveInput);
        //_motor.HandleGravity();
        if (_motor.IsDashing)
        {
            _motor.HandleDashLogic(_time, _dashSpeed);
        }
        else
        {
            _motor.HandleJump(_time);
            _motor.HandleDirection(_inputHandler.MoveInput);
            _motor.HandleGravity();
        }
        //_dashPressedFrame = false;
        _motor.ApplyMovement();
    }

    private Vector2 GetUpDir()
    {
        var g = Physics2D.gravity;
        if (g.sqrMagnitude < 0.0001f) return Vector2.up;
        return -g.normalized;
    }

    private Vector2 GetRightDir(Vector2 up)
    {
        return new Vector2(up.y, -up.x);
    }

    private void OnCameraRotationComplete()
    {
        _gravityFlip.OnCameraRotationComplete(transform, ref _waitingForCamera, ref _frameVelocity, ref _pendingGravity);
    }

    private void TryFire(Vector2 inputDir)
    {
        if (_weaponSystem == null || !_ability.Has(AbilityType.Gun)) return;
        
        Vector2 up = GetUpDir();
        Vector2 right = GetRightDir(up);
        inputDir = inputDir.normalized;
        
        Vector2 dir = inputDir.sqrMagnitude > 0.1f 
            ? (inputDir.x * right + inputDir.y * up) 
            : (_visuals.transform.localScale.x < 0 ? -right : right);
        
        _lastAimDir = dir.normalized;

        Vector2 inheritedVelocity = new Vector2(_rb.linearVelocity.x, 0f); 

        Vector2 playerVel = _rb.linearVelocity;

        _weaponSystem.RequestFire(dir.normalized, inheritedVelocity, playerVel);
    }

    private void TrySpecialSkill(Vector2 inputDir)
    {
        _playerSkill?.TrySpecialSkill(inputDir);
    }

    private void HealPlayer()
    {
        if (_isHealing) return;
        StartCoroutine(HealRoutine());
    }

    private IEnumerator HealRoutine()
    {
        _isHealing = true;
        
        _rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(3f);

        _health?.Heal(3);
        activeEffects.ForEach(effect => effect.OnHeal(this));

        _isHealing = false;
    }

    private void HandleArmIdleReturn()
    {
        if (_weaponSystem == null) return;
        _weaponSystem.HandleIdleArm();
    }

    public void ScalePlayer(ScaleType scaleType)
    {
        if (!_ability.Has(AbilityType.Scale)) return;
        switch (scaleType)
        {
            case ScaleType.Small:
                transform.localScale = Vector3.one * 0.5f;
                break;
            case ScaleType.Normal:
                transform.localScale = Vector3.one;
                break;
            case ScaleType.Big:
                transform.localScale = Vector3.one * 1.5f;
                break;
        }
    }

    public void SetGunType(GlobalEnums.GunType type)
    {
        _currentGunType = type;
        _weaponSystem?.SetGunType(type);
    }

    private void EnsureGunDefaults()
    {
        var n = _gunSet.Normal;
        if (n.bulletSpeed <= 0) n.bulletSpeed = 30f;
        if (n.bulletLifetime <= 0) n.bulletLifetime = 2.5f;
        if (n.cooldown <= 0) n.cooldown = 0.15f;
        if (n.damage <= 0) n.damage = 10f;
        if (n.bulletCount <= 0) n.bulletCount = 1;
        if (n.spreadAngle < 0) n.spreadAngle = 0f;
        _gunSet.Normal = n;

        var s = _gunSet.Shotgun;
        if (s.bulletSpeed <= 0) s.bulletSpeed = 22f;
        if (s.bulletLifetime <= 0) s.bulletLifetime = 1.2f;
        if (s.cooldown <= 0) s.cooldown = 0.45f;
        if (s.damage <= 0) s.damage = 8f;
        if (s.bulletCount <= 0) s.bulletCount = 4;
        if (s.spreadAngle <= 0) s.spreadAngle = 18f;
        _gunSet.Shotgun = s;

        var r = _gunSet.Rapid;
        if (r.bulletSpeed <= 0) r.bulletSpeed = 26f;
        if (r.bulletLifetime <= 0) r.bulletLifetime = 1.6f;
        if (r.cooldown <= 0) r.cooldown = 0.08f;
        if (r.damage <= 0) r.damage = 6f;
        if (r.bulletCount <= 0) r.bulletCount = 1;
        if (r.spreadAngle < 0) r.spreadAngle = 0f;
        _gunSet.Rapid = r;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnvObstacle"))
        {
            _health ??= GetComponent<PlayerHealth>();
            if (_isHealing) 
            {
                StopAllCoroutines();
                _isHealing = false;
            }
            _health?.TakeDamage(1);
        }
    }

    private void LoadActiveCursedObjects()
    {
        var saveData = SaveSystemz.Load();
        if (saveData?.player?.currentCursedObjects == null || saveData.player.currentCursedObjects.Count == 0)
            return;

        ClearAllCursedEffects();

        foreach (var cursedId in saveData.player.currentCursedObjects)
        {
            var data = cursedList.GetById(cursedId);
            if (data == null)
            {
                Debug.LogWarning($"Không tìm thấy CursedObject với id: {cursedId}");
                continue;
            }

            ApplyCursedObject(data);
        }
    }

    private void ClearAllCursedEffects()
    {
        // Xóa Passive/Ability
        foreach (var effect in activeEffects)
            effect.OnRemove(this);
        activeEffects.Clear();

        // Xóa Skill
        currentSkillEffect?.OnRemove(this);
        currentSkillEffect = null;

        // Xóa Ultimate
        if (currentUltimateEffect != null)
        {
            currentUltimateEffect.OnRemove(this);
            currentUltimateEffect = null;
        }
    }

    private void ApplyCursedObject(CursedObjectData data)
    {
        if (data == null || data.Effects == null || data.Effects.Count == 0)
            return;

        switch (data.type)
        {
            case CursedObjectType.Passive:
            case CursedObjectType.Ability:
                foreach (var effect in data.Effects)
                {
                    activeEffects.Add(effect);
                    effect.OnApply(this);
                }
                break;

            case CursedObjectType.Skill:
                currentSkillEffect?.OnRemove(this);

                currentSkillEffect = data.Effects[0];
                currentSkillEffect.OnApply(this);
                break;

            case CursedObjectType.Ultimate:
                currentUltimateEffect?.OnRemove(this);

                currentUltimateEffect = data.Effects[0];
                currentUltimateEffect.OnApply(this);
                break;

            default:
                Debug.LogWarning($"Loại CursedObject {data.type} chưa được xử lý!");
                break;
        }
    }

    public void EquipCursedObject(string cursedId)
    {
        var data = cursedList.GetById(cursedId);
        if (data == null)
        {
            Debug.LogWarning($"Không tồn tại cursed object id: {cursedId}");
            return;
        }

        // Optional: Kiểm tra đã equip chưa (tránh trùng)
        var save = SaveSystemz.Load();
        if (save?.player?.currentCursedObjects?.Contains(cursedId) == true)
            return; // hoặc xử lý replace tùy game

        // Áp dụng ngay lập tức
        ApplyCursedObject(data);

        // Cập nhật save data
        if (save.player == null) save.player = new PlayerData();
        if (!save.player.currentCursedObjects.Contains(cursedId))
        {
            save.player.currentCursedObjects.Add(cursedId);
            SaveSystemz.Save(save);
        }
    }
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;

    public event Action Jumped;
    public Vector2 FrameInput { get; }
}
