using UnityEngine;
using UnityEngine.InputSystem;
using GlobalEnums;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private GameStateChannel _stateChannel;
    private PlayerInput _playerInput;

    // Sử dụng InputActionMap để quản lý các action
    private InputActionMap _actionMap;

    // Các action quan trọng
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

    // Properties public
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
            Debug.LogError("PlayerInput component not found!");
            enabled = false;
            return;
        }

        _actionMap = _playerInput.actions.FindActionMap("InGame", true);

        if (_actionMap == null)
        {
            Debug.LogError("Không tìm thấy ActionMap 'InGame'!");
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
        float horizontal = 0f;
        float vertical = 0f;
        if (MoveLeftHeld) horizontal -= 1f;
        if (MoveRightHeld) horizontal += 1f;
        if (UpHeld) vertical += 1f;
        if (DownHeld) vertical -= 1f;
        MoveInput = new Vector2(horizontal, 0f);
        AimInput = new Vector2(horizontal, vertical).normalized;
    }

    public bool TryGetGunSwitch(out GunType type)
    {
        type = GunType.Normal;
        if (_playerInput.actions.FindAction("GunNormal")?.WasPressedThisFrame() ?? false) { type = GunType.Normal; return true; }
        if (_playerInput.actions.FindAction("GunShotgun")?.WasPressedThisFrame() ?? false) { type = GunType.Shotgun; return true; }
        if (_playerInput.actions.FindAction("GunRapid")?.WasPressedThisFrame() ?? false) { type = GunType.Rapid; return true; }
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
