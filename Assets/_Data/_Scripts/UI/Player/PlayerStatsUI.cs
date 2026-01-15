using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using GlobalEnums;

public class PlayerStatsUI : Singleton<PlayerStatsUI>
{
    [SerializeField] private GameStateChannel _stateChannel;
    [SerializeField] private Image avatar;
    [Header("Health Settings")]
    public GameObject HealthBarRoot;
    [SerializeField] private HealthNodeUI _healthNodePrefab;
    private readonly List<HealthNodeUI> _healthNodes = new();

    [Header("Mana Settings")]
    public GameObject ManaBarRoot;
    [SerializeField] private ManaNodeUI _manaNodePrefab;
    private readonly List<ManaNodeUI> _manaNodes = new();

    protected override void Awake()
    {
        base.Awake();
        HideStats();
    }

    private void OnEnable()
    {
        UIEventSystem.OnHealthChanged += HandleHealthChanged;
        UIEventSystem.OnManaChanged += HandleManaChanged;
        
        if (_stateChannel != null)
            _stateChannel.OnStateRequested += HandleStateChange;
    }

    private void OnDisable()
    {
        UIEventSystem.OnHealthChanged -= HandleHealthChanged;
        UIEventSystem.OnManaChanged -= HandleManaChanged;
        
        if (_stateChannel != null)
            _stateChannel.OnStateRequested -= HandleStateChange;
    }

    private void HandleStateChange(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            ShowStats();
        }
        else
        {
            HideStats();
        }
    }

    private void ShowStats()
    {
        avatar.gameObject.SetActive(true);
        HealthBarRoot.SetActive(true);
        ManaBarRoot.SetActive(true);
    }

    private void HideStats()
    {
        avatar.gameObject.SetActive(false);
        HealthBarRoot.SetActive(false);
        ManaBarRoot.SetActive(false);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        int max = Mathf.RoundToInt(maxHealth);
        int current = Mathf.RoundToInt(currentHealth);

        if (_healthNodes.Count != max)
        {
            RebuildHealthBar(max);
        }

        for (int i = 0; i < _healthNodes.Count; i++)
        {
            _healthNodes[i].SetFull(i < current);
        }
    }

    private void RebuildHealthBar(int max)
    {
        foreach (var n in _healthNodes) if(n != null) Destroy(n.gameObject);
        _healthNodes.Clear();

        for (int i = 0; i < max; i++)
        {
            var node = Instantiate(_healthNodePrefab, HealthBarRoot.transform);
            node.SetFull(true, false);
            _healthNodes.Add(node);
        }
    }

    private void HandleManaChanged(float currentMana, float maxMana)
    {
        int max = Mathf.RoundToInt(maxMana);

        if (_manaNodes.Count != max)
        {
            RebuildManaBar(max);
        }

        for (int i = 0; i < _manaNodes.Count; i++)
        {
            float nodeFill = Mathf.Clamp01(currentMana - i);
            _manaNodes[i].SetFillAmount(nodeFill);
        }
    }

    private void RebuildManaBar(int max)
    {
        foreach (var n in _manaNodes) if(n != null) Destroy(n.gameObject);
        _manaNodes.Clear();

        for (int i = 0; i < max; i++)
        {
            var node = Instantiate(_manaNodePrefab, ManaBarRoot.transform);
            node.SetFillAmount(0f, false);
            _manaNodes.Add(node);
        }
    }
}