using UnityEngine;
using System;

public class PlayerRestState : Singleton<PlayerRestState>
{
    [Header("Rest State Settings")]
    [SerializeField] private bool _isResting = false;
    [SerializeField] private float _restCheckInterval = 0.1f;
    [SerializeField] private float _restGracePeriod = 0.5f; // Grace period after starting rest
    
    private Vector2 _lastMoveInput;
    private float _lastCheckTime;
    private float _restStartTime; // Track when rest started
    private PlayerController _playerController;
    private PlayerInputHandler _inputHandler;
    
    public bool IsResting => _isResting;
    
    public static event Action<bool> OnRestStateChanged;
    
    private void Start()
    {
        // Don't find components in Start - find them when needed
        Debug.Log("PlayerRestState: Started, will find components when needed");
    }
    
    private void Update()
    {
        if (Time.time - _lastCheckTime < _restCheckInterval) return;
        _lastCheckTime = Time.time;
        
        if (_isResting)
        {
            Debug.Log("PlayerRestState: Checking for activity...");
            CheckForActivity();
        }
    }
    
    public void StartRest()
    {
        if (_isResting) return;
        
        _isResting = true;
        _restStartTime = Time.time;
        _lastMoveInput = _inputHandler?.MoveInput ?? Vector2.zero;
        
        Debug.Log($"PlayerRestState: Started resting with grace period {_restGracePeriod}s");
        OnRestStateChanged?.Invoke(true);
    }
    
    public void EndRest()
    {
        if (!_isResting) return;
        
        _isResting = false;
        Debug.Log("PlayerRestState: Ended resting due to activity");
        OnRestStateChanged?.Invoke(false);
    }
    
    private void CheckForActivity()
    {
        if (!_isResting) return;
        
        // Check if we're still in grace period
        float timeSinceRestStart = Time.time - _restStartTime;
        if (timeSinceRestStart < _restGracePeriod)
        {
            Debug.Log($"PlayerRestState: In grace period ({timeSinceRestStart:F2}s < {_restGracePeriod}s), skipping activity check");
            return;
        }
        
        // Find components if not already found
        if (_playerController == null)
        {
            _playerController = FindAnyObjectByType<PlayerController>();
            Debug.Log($"PlayerRestState: Found PlayerController: {_playerController != null}");
        }
        
        if (_inputHandler == null)
        {
            _inputHandler = FindAnyObjectByType<PlayerInputHandler>();
            Debug.Log($"PlayerRestState: Found PlayerInputHandler: {_inputHandler != null}");
        }
        
        // Check for position change (movement)
        
        // Check for input activity
        if (_inputHandler != null)
        {
            Vector2 currentMoveInput = _inputHandler.MoveInput;
            if (currentMoveInput.sqrMagnitude > 0.01f)
            {
                Debug.Log($"PlayerRestState: Input detected - moveInput: {currentMoveInput}");
                EndRest();
                return;
            }
            
            // Check for other inputs (jump, dash, fire, etc.)
            if (_inputHandler.JumpDown || _inputHandler.DashDown || _inputHandler.FireHeld || 
                _inputHandler.HealDown || _inputHandler.SkillInput)
            {
                Debug.Log($"PlayerRestState: Action input detected - jump:{_inputHandler.JumpDown}, dash:{_inputHandler.DashDown}, fire:{_inputHandler.FireHeld}");
                EndRest();
                return;
            }
            
            // Update last input after check
            _lastMoveInput = currentMoveInput;
        }
        else
        {
            Debug.LogWarning("PlayerRestState: _inputHandler is null!");
        }
    }
    
    public bool CanInteractWithUI()
    {
        return _isResting;
    }
}
