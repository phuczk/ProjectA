using System.Collections;
using UnityEngine;

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

    private SpriteRenderer _spriteRenderer;
    private WaitForSeconds _blinkWait;
    private WaitForSeconds _invincibleWait;

    public event System.Action<PlayerSoundType, AudioClip> OnRequestSound;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _blinkWait = new WaitForSeconds(0.08f);
        _invincibleWait = new WaitForSeconds(invincibleDuration);
        var data = SaveSystemz.Load();
        if (data != null && data.player != null)
        {
            MaxHealth = data.player.maxHealth;
            MaxMana = data.player.maxMana;
        }
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
            Die();
            return;
        }

        OnRequestSound?.Invoke(PlayerSoundType.Damage, null);

        _invincibleTimer = Time.time + invincibleDuration;

        StopAllCoroutines();
        StartCoroutine(InvincibleVisualEffect());
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
        
        var inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.DisableInputForDuration(2f);
        }
        
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
{
    yield return new WaitForSeconds(1f);
    Vector3 respawnPosition = FindRespawnPosition();
    
    string targetScene = GetRespawnScene();
    
    if (SceneTransitionManager.Instance != null)
    {
        SceneTransitionManager.Instance.TransitionToScene(targetScene);
    }
    else
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
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
