using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour, ISaveable, ISoundEmitter
{
    [Header("PlayerStats")]
    private int _currentHealth;
    public int CurrentHealth 
    {
        get => _currentHealth;
        set {
            if (_currentHealth == value) return;
            _currentHealth = value;
            UIEventSystem.OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }
    }
    public int MaxHealth = 5;

    private float _currentMana;
    public float CurrentMana 
    {
        get => _currentMana;
        set {
            if (_currentMana == value) return;
            _currentMana = value;
            UIEventSystem.OnManaChanged?.Invoke(_currentMana, MaxMana);
        }
    }
    public int MaxMana;

    [SerializeField] private float invincibleDuration = 0.5f;
    private float _invincibleTimer;
    
    [Header("Death Animation Settings")]
    [SerializeField] private float deathSlowMoDuration = 0.5f;
    [SerializeField] private float deathSlowMoTimeScale = 0.3f;
    [SerializeField] private float deathFlyUpDuration = 2f;
    [SerializeField] private float deathFlyUpForce = 10f;
    [SerializeField] private float deathFloatDuration = 2f;
    [SerializeField] private float deathFloatAmplitude = 0.5f;
    [SerializeField] private float deathFloatFrequency = 2f;
    [SerializeField] private float deathTargetRotation = 45f;

    private SpriteRenderer _spriteRenderer;
    private WaitForSeconds _blinkWait;
    private WaitForSeconds _invincibleWait;
    public Rigidbody2D _rb2d;
    private PlayerInputHandler _inputHandler;
    
    private static bool _isRespawning = false;

    public event System.Action<PlayerSoundType, AudioClip> OnRequestSound;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_rb2d == null) _rb2d = GetComponent<Rigidbody2D>();
        _blinkWait = new WaitForSeconds(0.08f);
        _invincibleWait = new WaitForSeconds(invincibleDuration);
        _inputHandler = GetComponent<PlayerInputHandler>();
        var data = SaveSystemz.Load();
        if (data != null && data.player != null)
        {
            MaxHealth = data.player.maxHealth;
            MaxMana = data.player.maxMana;
        }
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (_isRespawning)
        {
            ResetPlayerState();
            _isRespawning = false;
        }
    }
    
    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CurrentHealth = MaxHealth;
        CurrentMana = 0f;
    }
    
    public void GainMana(float amount)
    {
        CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
    }

    public bool TryUseMana(int amount)
    {
        if (CurrentMana >= amount)
        {
            CurrentMana -= amount;
            OnRequestSound?.Invoke(PlayerSoundType.ManaUsed, null);
            return true;
        }
        return false;
    }

    public void TakeDamage(int damage)
    {
        if (Time.time < _invincibleTimer) return;

        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            
            if (_rb2d != null)
            {
                _rb2d.linearVelocity = Vector2.zero;
                _rb2d.angularVelocity = 0f;
                _rb2d.simulated = false;
            }
            
            Die();
            return;
        }

        OnRequestSound?.Invoke(PlayerSoundType.Damage, null);

        _invincibleTimer = Time.time + invincibleDuration;

        StopAllCoroutines();
        StartCoroutine(InvincibleVisualEffect());
    }
    
    private void DisablePlayerCollider()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
    }
    
    private void EnablePlayerCollider()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = true;
        }
    }

    public void Heal(int amount)
    {
        if (TryUseMana(1))
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            OnRequestSound?.Invoke(PlayerSoundType.Heal, null); 
        }
    }
    
    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }

    private IEnumerator InvincibleVisualEffect()
    {
        while (Time.time < _invincibleTimer)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            yield return _blinkWait;
        }
        
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = true;
    }

    public void Die()
    {
        OnRequestSound?.Invoke(PlayerSoundType.Death, null);
        
        _isRespawning = true;
        
        if (_inputHandler != null)
        {
            _inputHandler.DisableInput();
        }
        
        var uiManager = UIInGameManager.Instance;
        if (uiManager != null)
        {
            uiManager.DisableUI();
        }
        
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        yield return StartCoroutine(DeathAnimationSequence());
        
        Vector3 respawnPosition = FindRespawnPosition();
        string targetScene = GetRespawnScene();
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetScene, TransitionType.Death, FadeDirection.Up);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }
    
    private IEnumerator DeathAnimationSequence()
    {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, deathSlowMoTimeScale, deathSlowMoDuration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true); 
        
        yield return new WaitForSecondsRealtime(deathSlowMoDuration);
        
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 0.2f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        yield return StartCoroutine(FlyUpAndRotateAnimation());
        
        yield return StartCoroutine(FloatingAnimation());
    }
    
    private void ResetPlayerState()
    {
        if (_rb2d != null)
        {
            _rb2d.simulated = true;
            _rb2d.bodyType = RigidbodyType2D.Dynamic;
            _rb2d.linearVelocity = Vector2.zero;
            _rb2d.angularVelocity = 0f;
        }
        
        transform.rotation = Quaternion.identity;
        
        EnablePlayerCollider();
        
        if (_inputHandler != null)
        {
            _inputHandler.EnableInput();
        }
        
        var uiManager = UIInGameManager.Instance;
        if (uiManager != null)
        {
            uiManager.EnableUI();
        }
    }
    
    private IEnumerator FlyUpAndRotateAnimation()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        
        while (elapsedTime < deathFlyUpDuration)
        {
            float flyProgress = elapsedTime / deathFlyUpDuration;
            Vector3 flyPosition = startPosition + Vector3.up * (deathFlyUpForce * flyProgress);
            
            float targetRotation = transform.localScale.x > 0 ? 30f : -30f;
            float currentRotation = Mathf.Lerp(0, targetRotation, flyProgress);
            Quaternion rotation = Quaternion.Euler(0, 0, currentRotation);
            
            transform.position = flyPosition;
            transform.rotation = rotation;
            
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }
    
    private IEnumerator FloatingAnimation()
    {
        Vector3 floatStartPosition = transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < deathFloatDuration)
        {
            float floatY = Mathf.Sin(elapsedTime * deathFloatFrequency * 2f * Mathf.PI) * deathFloatAmplitude;
            Vector3 floatPosition = floatStartPosition + Vector3.up * floatY;
            
            transform.position = floatPosition;
            
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private string GetRespawnScene()
    {
        var saveData = SaveSystemz.Load();
        if (saveData?.world != null && !string.IsNullOrEmpty(saveData.world.currentSceneName))
        {
            return saveData.world.currentSceneName;
        }
        
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return currentScene;
    }
    
    private Vector3 FindRespawnPosition()
    {
        var saveData = SaveSystemz.Load();
        if (saveData?.player != null)
        {
            Vector3 savedPosition = saveData.player.position;
            if (savedPosition != Vector3.zero)
            {
                return savedPosition;
            }
        }
        
        var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        if (spawnPoints.Length > 0)
        {
            foreach (var spawn in spawnPoints)
            {
                if (spawn.IsDefaultSpawn && spawn.IsActive)
                {
                    return spawn.GetSpawnPosition();
                }
            }
            
            SpawnPoint closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (var spawn in spawnPoints)
            {
                if (!spawn.IsActive) continue;
                
                float distance = Vector3.Distance(transform.position, spawn.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = spawn;
                }
            }
            
            if (closest != null)
            {
                return closest.GetSpawnPosition();
            }
        }
        
        return Vector3.zero;
    }

    public void SaveData(SaveData data)
    {
        if (data == null) return;
        if (data.player == null) data.player = new PlayerData();
        data.player.maxHealth = MaxHealth;
        data.player.maxMana = MaxMana;
    }

    public void LoadData(SaveData data)
    {
        if (data == null || data.player == null) return;
        MaxHealth = data.player.maxHealth;
        MaxMana = data.player.maxMana;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        CurrentMana = Mathf.Clamp(CurrentMana, 0f, MaxMana);
    }
}
