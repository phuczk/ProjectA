using UnityEngine;
using System;

public class PlayerRestState : Singleton<PlayerRestState>
{
    [Header("Rest State Settings")]
    [SerializeField] private bool _isResting = false;
    [SerializeField] private float _restCheckInterval = 0.1f;
    [SerializeField] private float _restGracePeriod = 0.5f;
    
    private Vector2 _lastMoveInput;
    private float _lastCheckTime;
    private float _restStartTime;
    private PlayerController _playerController;
    private PlayerInputHandler _inputHandler;
    
    public bool IsResting => _isResting;
    
    public static event Action<bool> OnRestStateChanged;
    
    private void Update()
    {
        if (Time.time - _lastCheckTime < _restCheckInterval) return;
        _lastCheckTime = Time.time;
        
        if (_isResting)
        {
            CheckForActivity();
        }
    }
    
    public void StartRest()
    {
        if (_isResting) return;
        
        _isResting = true;
        _restStartTime = Time.time;
        _lastMoveInput = _inputHandler?.MoveInput ?? Vector2.zero;
        
        OnRestStateChanged?.Invoke(true);
    }
    
    public void EndRest()
    {
        if (!_isResting) return;
        
        _isResting = false;
        OnRestStateChanged?.Invoke(false);
    }
    
    private void CheckForActivity()
    {
        if (!_isResting) return;
        
        float timeSinceRestStart = Time.time - _restStartTime;
        if (timeSinceRestStart < _restGracePeriod)
        {
            return;
        }
        
        if (_playerController == null)
        {
            _playerController = FindAnyObjectByType<PlayerController>();
        }
        
        if (_inputHandler == null)
        {
            _inputHandler = FindAnyObjectByType<PlayerInputHandler>();
        }

        if (_inputHandler != null)
        {
            Vector2 currentMoveInput = _inputHandler.MoveInput;
            if (currentMoveInput.sqrMagnitude > 0.01f)
            {
                EndRest();
                return;
            }
            
            if (_inputHandler.JumpDown || _inputHandler.DashDown || _inputHandler.FireHeld || 
                _inputHandler.HealDown || _inputHandler.SkillInput)
            {
                EndRest();
                return;
            }
            
            _lastMoveInput = currentMoveInput;
        }
    }
    
    public bool CanInteractWithUI()
    {
        return _isResting;
    }
}
