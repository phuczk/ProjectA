using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GunConfigSet GunConfigSet => _gunSet;
    
    public static event Action<Vector3> OnPlayerRespawn;
    
    public void TriggerRespawnEvent(Vector3 respawnPosition)
    {
        OnPlayerRespawn?.Invoke(respawnPosition);
    }
    
    public static void TriggerRespawnStatic(Vector3 respawnPosition)
    {
        OnPlayerRespawn?.Invoke(respawnPosition);
    }
    
    private Rigidbody2D _rb;
    private Collider2D _col;
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
    [SerializeField] private PlayerEffectRunner _effectRunner;
    [SerializeField] private PlayerUI _playerUI;
    private bool _dashPressedFrame;
    private bool _canFlipGravity = true;

    [Header("Cursed System")]
    [SerializeField] private CursedList cursedList;
    [SerializeField] private CursedNotchManager cursedNotchManager;
    
    [Header("Gun Selection System")]
    [SerializeField] private GunSelectionUI gunSelectionUI;
    
    private HashSet<string> _unlockedSet = new();
    private HashSet<string> _equippedSet = new();

    private bool _isHealing;
    private WaitForSeconds _healWait;

    private FrameInput _frameInput; 
    private ScaleType _currentScale = ScaleType.Normal;

    public int Money { get; private set; } = 0;

    private float fallingSpeed;

    [SerializeField] private List<GunEntry> _guns;

    [System.Serializable]
    public class GunEntry
    {
        public GunType type;
        public GameObject obj;
    }

    private Dictionary<GunType, GameObject> _gunMap;


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
        
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.OnAllCamerasRotated += OnCameraRotationComplete;
        }
        if (Arm != null)
            _armDefaultRotation = Arm.localRotation;
        EnsureGunDefaults();
        SetupSystems();
        
        _healWait = new WaitForSeconds(1.5f);

        fallingSpeed = CameraManager.Instance.fallSpeed;
        SetupGunMap();
        LoadCurrentGun();
        LoadActiveCursedObjects();
    }

    private void SetupGunMap()
    {
        _gunMap = new();
        foreach(var g in _guns)
            _gunMap[g.type] = g.obj;
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
        _effectRunner = GetOrAdd<PlayerEffectRunner>();
        _playerUI = GetOrAdd<PlayerUI>();
        _cursedObject = GetOrAdd<PlayerCursedObject>();
        _weaponSystem = GetComponent<WeaponSystem>();
    }

    private T GetOrAdd<T>() where T : Component => GetComponent<T>() ?? gameObject.AddComponent<T>();

    private void SetupSystems()
    {
        _gravityFlip.Configure(_manager, _rb, _ability);
        _motor.Configure(_rb, _col, _stats, _visuals, _ability);
        _motor.OnGroundedChanged = (g, v) => { GroundedChanged?.Invoke(g, v); if (g) _canFlipGravity = true; };
        _motor.OnJumped = () => Jumped?.Invoke();

        if (_weaponSystem != null)
        {
            _weaponSystem.Configure(_gunSet, _firePoint, Arm, _visuals, _armAngleOffset, _armIdleDelay, _manager, true);
            _weaponSystem.SetGunType(_currentGunType);
            _weaponSystem.OnFireTriggered += HandleGunFireEffect;
        }
        
        _playerSkill.Configure(_ability, _health);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        GunSelectionUI.OnGunChanged += HandleGunChangedFromUI;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        GunSelectionUI.OnGunChanged -= HandleGunChangedFromUI;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SlotScene" || scene.name == "New Scene" || scene.name == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }
        
        CheckForPendingRespawn();
    }
    
    private void CheckForPendingRespawn()
    {
        var playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.CurrentHealth <= 0)
        {
            var saveData = SaveSystemz.Load();
            if (saveData?.player != null)
            {
                Vector3 savedPosition = saveData.player.position;
                
                transform.position = savedPosition;
                
                playerHealth.CurrentHealth = playerHealth.MaxHealth;
                
                StartCoroutine(InvulnerabilityCoroutine());
                
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.OnPlayerRespawn(savedPosition);
                }
            }
        }
    }
    
    private IEnumerator InvulnerabilityCoroutine()
    {
        var playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (_inputHandler != null)
            {
                _inputHandler.DisableInputForDuration(1f);
            }
            
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.OnAllCamerasRotated -= OnCameraRotationComplete;
        }
        
        PlayerSpawnService.Clear();
        if (_weaponSystem != null)
        {
            _weaponSystem.OnFireTriggered -= HandleGunFireEffect;
        }
    }

    private void GatherInput()
    {
        _frameInput.Move = _inputHandler.MoveInput;
        _frameInput.JumpDown = _inputHandler.JumpDown;
        _frameInput.JumpHeld = _inputHandler.JumpHeld;
        _frameInput.DashDown = _inputHandler.DashDown;
        _frameInput.FireHeld = _inputHandler.FireHeld;
        _frameInput.HealDown = _inputHandler.HealDown;
        _frameInput.ScaleBig = _inputHandler.ScaleBig();
        _frameInput.ScaleSmall = _inputHandler.ScaleSmall();
        _frameInput.ScaleNormal = _inputHandler.ScaleNormal();
        _frameInput.SpecialDown = _inputHandler.SkillInput;

        if (_inputHandler.TryGetGunSwitch(out var gunType))
        {
            SetGunType(gunType);
        }
    }

    private void Update()
    {
        if (_isHealing) return;
        _time += Time.deltaTime;

        _gravityFlip.HandleInput(_inputHandler, transform, ref _waitingForCamera, ref _pendingGravity, ref _canFlipGravity);

        GatherInput();

        _motor.SetJumpInput(_frameInput.JumpDown, _frameInput.JumpHeld, _time);

        if (_frameInput.FireHeld) TryFire(_inputHandler.AimInput);
        if (_frameInput.SpecialDown) TrySpecialSkill(_frameInput.Move);
        
        if (_frameInput.HealDown) HealPlayer();

        HandleScaling();
        
        if (_frameInput.DashDown) 
            _motor.TryStartDash(_time, _dashSpeed, _dashDuration, _dashCooldownTime);

        _weaponSystem?.HandleIdleArm();

        if (_rb.linearVelocity.y < fallingSpeed 
            && !CameraManager.Instance.IsLerpingYDamping 
            && !CameraManager.Instance.LerpedFromPlayerFalling)
        {
            CameraManager.Instance.LerpYDamping(true);
        }

        if (_rb.linearVelocity.y >= 0f 
            && !CameraManager.Instance.IsLerpingYDamping 
            && CameraManager.Instance.LerpedFromPlayerFalling)
        {
            CameraManager.Instance.LerpedFromPlayerFalling = false;
            CameraManager.Instance.LerpYDamping(false);
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
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

    private void HandleGunFireEffect(PlayerController player, Vector2 direction)
    {
        GameEventBus.Instance?.RaiseGunFire(player, direction);
    }

    private void FixedUpdate()
    {
        if (_waitingForCamera || _isHealing) return;

        _motor.CheckCollisions(_time);
        
        if (_motor.IsDashing)
        {
            _motor.HandleDashLogic(_time, _dashSpeed);
        }
        else
        {
            _motor.HandleJump(_time);
            _motor.HandleDirection(_frameInput.Move);
            _motor.HandleGravity();
        }
        
        _motor.ApplyMovement();
    }

    private void HandleScaling()
    {
        ScaleType targetScale = _currentScale;
        if (_frameInput.ScaleBig) targetScale = ScaleType.Big;
        else if (_frameInput.ScaleSmall) targetScale = ScaleType.Small;
        else if (_frameInput.ScaleNormal) targetScale = ScaleType.Normal;

        if (targetScale != _currentScale)
        {
            ScalePlayer(targetScale);
            _currentScale = targetScale;
        }
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
        _motor.ResetVelocity();
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
        
        if (_health != null && _health.CurrentMana >= 1)
        {
            StartCoroutine(HealRoutine());
        }
    }

    private IEnumerator HealRoutine()
    {
        _isHealing = true;
        
        _rb.linearVelocity = Vector2.zero;
        yield return _healWait;

        _health?.Heal(3);
        GameEventBus.Instance?.RaiseHeal(this);

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
        foreach (var g in _gunMap.Values)
            g.SetActive(false);

        if (_gunMap.TryGetValue(type, out var gun))
            gun.SetActive(true);

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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Money"))
        {
            LootManager.Instance.AddMoney(1);
            BulletPool.Instance.Release(other.gameObject);
        }
    }

    private void CacheFromSave(SaveData data) { if (data == null) return; if (data.items != null) CollectionSync.Cache(_unlockedSet, data.items.unlockedCursedObjects); if (data.player != null) CollectionSync.Cache(_equippedSet, data.player.currentCursedObjects); }

    public void UnlockCursedObject(string cursedId)
    {
        var data = cursedList.GetById(cursedId);
        if (data == null) return;

        var mgr = SaveManager.Instance;

        if (mgr != null)
        {
            if (mgr.CurrentData.items == null)
                mgr.CurrentData.items = new ItemData();

            CacheFromSave(mgr.CurrentData);

            if (_unlockedSet.Add(cursedId))
            {
                CollectionSync.SyncList(
                    _unlockedSet,
                    mgr.CurrentData.items.unlockedCursedObjects
                );

                mgr.SaveGame();
                _cursedObject?.OnUnlocked(data);
            }
            return;
        }

        var save = SaveSystemz.Load();
        if (save.items == null) save.items = new ItemData();

        CacheFromSave(save);

        if (_unlockedSet.Add(cursedId))
        {
            CollectionSync.SyncList(
                _unlockedSet,
                save.items.unlockedCursedObjects
            );

            SaveSystemz.Save(save);
            _cursedObject?.OnUnlocked(data);
        }
    }

    private void LoadActiveCursedObjects()
    {
        var save = SaveSystemz.Load();
        if (save == null) return;

        CacheFromSave(save);

        _effectRunner.RebuildEffects(_equippedSet);
        
        if (cursedNotchManager == null)
        {
            cursedNotchManager = FindAnyObjectByType<CursedNotchManager>();
        }
        
        if (gunSelectionUI == null)
        {
            gunSelectionUI = FindAnyObjectByType<GunSelectionUI>();
        }
    }

    public void EquipCursedObject(string cursedId)
    {
        var mgr = SaveManager.Instance;
        var data = mgr != null ? mgr.CurrentData : SaveSystemz.Load();
        
        if (data.player == null || data.items == null) return;

        if (!data.items.unlockedCursedObjects.Contains(cursedId))
        {
            return;
        }

        if (data.player.currentCursedObjects.Contains(cursedId))
        {
            data.player.currentCursedObjects.Remove(cursedId);
        }
        else
        {
            if (data.player.currentCursedObjects.Count < data.player.currentNotch)
            {
                data.player.currentCursedObjects.Add(cursedId);
            }
            else
            {
                return;
            }
        }

        if (mgr != null) mgr.SaveGame();
        else SaveSystemz.Save(data);

        cursedNotchManager?.RefreshNotchDisplay();
        LoadActiveCursedObjects();
        
        if (gunSelectionUI == null)
        {
            gunSelectionUI = FindAnyObjectByType<GunSelectionUI>();
        }
    }

    private void LoadCurrentGun()
    {
        var mgr = SaveManager.Instance;
        if (mgr?.CurrentData?.player == null)
        {
            SetGunType(GunType.Normal);
            return;
        }

        var currentGun = mgr.CurrentData.player.currentGun;
        var unlockedGuns = mgr.CurrentData.player.unlockedGuns ?? new List<GunType>();
        
        if (!unlockedGuns.Contains(currentGun))
        {
            currentGun = GunType.Normal;
        }
        
        SetGunType(currentGun);
    }

    public void SetCurrentGun(GunType gunType)
    {
        var mgr = SaveManager.Instance;
        if (mgr == null || mgr.CurrentData?.player == null) return;

        if (!mgr.CurrentData.player.unlockedGuns.Contains(gunType))
        {
            return;
        }

        mgr.CurrentData.player.currentGun = gunType;
        mgr.SaveGame();
        
        SetGunType(gunType);
        
        gunSelectionUI?.RefreshUI();
        
        _weaponSystem?.SetGunType(gunType);
    }

    private void DebugGunObjectsStatus()
    {
        foreach (var kvp in _gunMap)
        {
            var gun = kvp.Value;
            var isActive = gun != null && gun.activeInHierarchy;
        }
    }

    public void UnlockGun(GunType gunType)
    {
        var mgr = SaveManager.Instance;
        if (mgr == null) return;

        mgr.CurrentData.player ??= new PlayerData();
        mgr.CurrentData.player.unlockedGuns ??= new List<GunType>();

        if (!mgr.CurrentData.player.unlockedGuns.Contains(gunType))
        {
            mgr.CurrentData.player.unlockedGuns.Add(gunType);
            mgr.SaveGame();
        }
    }
    
    private void HandleGunChangedFromUI(GunType newGun)
    {
        var mgr = SaveManager.Instance;
        if (mgr?.CurrentData?.player != null && mgr.CurrentData.player.unlockedGuns.Contains(newGun))
        {
            SetGunType(newGun);
        }
    }
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public Vector2 FrameInput { get; }
}
