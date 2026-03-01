using UnityEngine;
using UnityEngine.InputSystem;
using GlobalEnums;
using System.Collections;
using System.Collections.Generic;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private GameStateChannel _stateChannel;
    private PlayerInput _playerInput;

    private InputActionMap _actionMap;

    private InputAction _upAction;
    private InputAction _downAction;
    private InputAction _leftAction;
    private InputAction _rightAction;
    private InputAction _healAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _fireAction;
    private InputAction _gravityAction;
    private InputAction _scaleAction;
    private InputAction _skillAction;
    private InputAction _gunNormalAction;
    private InputAction _gunShotgunAction;
    private InputAction _gunRapidAction;

    private bool _inputDisabled = false;
    private Coroutine _disableInputCoroutine;

    public Vector2 MoveInput { get; private set; }
    public Vector2 AimInput { get; private set; }
    public bool MoveLeftHeld => _leftAction?.IsPressed() ?? false;
    public bool MoveRightHeld => _rightAction?.IsPressed() ?? false;
    public bool MoveLeftDown => _leftAction?.WasPressedThisFrame() ?? false;
    public bool MoveRightDown => _rightAction?.WasPressedThisFrame() ?? false;
    public bool UpHeld => _upAction?.IsPressed() ?? false;
    public bool UpDown => _upAction?.WasPressedThisFrame() ?? false;
    public bool DownHeld => _downAction?.IsPressed() ?? false;
    public bool DownDown => _downAction?.WasPressedThisFrame() ?? false;
    public bool JumpDown => _jumpAction?.WasPressedThisFrame() ?? false;
    public bool HealDown => _healAction?.WasPressedThisFrame() ?? false;
    public bool JumpHeld => _jumpAction?.IsPressed() ?? false;
    public bool DashDown => _dashAction?.WasPressedThisFrame() ?? false;
    public bool FireHeld => _fireAction?.IsPressed() ?? false;
    public bool GravityInput => _gravityAction?.IsPressed() ?? false;
    public bool ScaleInput => _scaleAction?.IsPressed() ?? false;
    public bool SkillInput => _skillAction?.IsPressed() ?? false;

    public bool IsFireHeld() => FireHeld;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            enabled = false;
            return;
        }

        _actionMap = _playerInput.actions.FindActionMap("InGame", true);

        if (_actionMap == null)
        {
            enabled = false;
            return;
        }

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        _upAction = _actionMap.FindAction("Up", true);
        _downAction = _actionMap.FindAction("Down", true);
        _leftAction = _actionMap.FindAction("Left", true);
        _rightAction = _actionMap.FindAction("Right", true);
        _healAction = _actionMap.FindAction("Heal", true);
        _jumpAction = _actionMap.FindAction("Jump", true);
        _dashAction = _actionMap.FindAction("Dash", true);
        _fireAction = _actionMap.FindAction("Shot", true);
        _gravityAction = _actionMap.FindAction("Gravity", true);
        _scaleAction = _actionMap.FindAction("Scale", true);
        _skillAction = _actionMap.FindAction("Skill", true);
    }

    private void OnEnable()
    {
        if (_stateChannel != null)
            _stateChannel.OnStateRequested += HandleStateChange;
    }

    private void OnDisable()
    {
        if (_stateChannel != null)
            _stateChannel.OnStateRequested -= HandleStateChange;
    }

    private void HandleStateChange(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            _actionMap.Enable();
        }
        else
        {
            _actionMap.Disable(); 
            ResetInputs();
        }
    }

    private void ResetInputs()
    {
        MoveInput = Vector2.zero;
        AimInput = Vector2.zero;
    }

    private void Update()
    {
        UpdateInput();
    }

    private void UpdateInput()
    {
        if (_inputDisabled)
        {
            ResetInputs();
            if (_actionMap.enabled)
            {
                _actionMap.Disable();
            }
            return;
        }
        
        float horizontal = 0f;
        float vertical = 0f;
        if (MoveLeftHeld) horizontal -= 1f;
        if (MoveRightHeld) horizontal += 1f;
        if (UpHeld) vertical += 1f;
        if (DownHeld) vertical -= 1f;
        MoveInput = new Vector2(horizontal, 0f);
        AimInput = new Vector2(horizontal, vertical).normalized;
    }
    
    public void DisableInputForDuration(float duration)
    {
        if (_disableInputCoroutine != null)
        {
            StopCoroutine(_disableInputCoroutine);
        }
        
        _disableInputCoroutine = StartCoroutine(DisableInputRoutine(duration));
    }
    
    public void DisableInput()
    {
        if (_disableInputCoroutine != null)
        {
            StopCoroutine(_disableInputCoroutine);
            _disableInputCoroutine = null;
        }
        
        _inputDisabled = true;
        
        if (_actionMap.enabled)
        {
            _actionMap.Disable();
        }
    }
    
    public void EnableInput()
    {
        _inputDisabled = false;
        
        if (!_actionMap.enabled)
        {
            _actionMap.Enable();
        }
    }
    
    private IEnumerator DisableInputRoutine(float duration)
    {
        _inputDisabled = true;
        yield return new WaitForSeconds(duration);
        _inputDisabled = false;
    }

    public bool TryGetGunSwitch(out GunType type)
    {
        type = GunType.Normal;
        if (_gunNormalAction?.WasPressedThisFrame() ?? false) { type = GunType.Normal; return true; }
        if (_gunShotgunAction?.WasPressedThisFrame() ?? false) { type = GunType.Shotgun; return true; }
        if (_gunRapidAction?.WasPressedThisFrame() ?? false) { type = GunType.Rapid; return true; }
        return false;
    }

    public bool ScaleBig() => ScaleInput && UpDown;
    public bool ScaleSmall() => ScaleInput && DownDown;
    public bool ScaleNormal() => ScaleInput && !UpDown && !DownDown;

    public bool FlipGravityUp() => GravityInput && UpHeld;
    public bool FlipGravityDown() => GravityInput && DownHeld;
    public bool FlipGravityLeft() => GravityInput && MoveLeftHeld;
    public bool FlipGravityRight() => GravityInput && MoveRightHeld;

    public bool IsInteract() => UpDown;
}

public struct FrameInput
{
    public Vector2 Move;
    public bool JumpDown;
    public bool JumpHeld;
    public bool DashDown;
    public bool HealDown;
    public bool FireHeld;
    public bool ScaleBig;
    public bool ScaleSmall;
    public bool ScaleNormal;
    public bool FlipUp;
    public bool FlipLeft;
    public bool FlipRight;
    public bool SpecialDown;

    public bool PauseInput;
    public bool MapInput;
    public bool InventoryInput;
}
