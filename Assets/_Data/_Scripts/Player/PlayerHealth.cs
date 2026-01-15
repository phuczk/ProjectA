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
    private float _invincibleTimer; // Thay thế Coroutine quản lý thời gian

    private SpriteRenderer _spriteRenderer;
    private WaitForSeconds _blinkWait; // Cache yield instruction
    private WaitForSeconds _invincibleWait;

    public event System.Action<PlayerSoundType, AudioClip> OnRequestSound;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Khởi tạo sẵn các lệnh chờ để tránh tạo rác (GC) trong lúc chơi
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
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnRequestSound?.Invoke(PlayerSoundType.Heal, null);
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
        Debug.Log("Player die");
        OnRequestSound?.Invoke(PlayerSoundType.Death, null);
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
