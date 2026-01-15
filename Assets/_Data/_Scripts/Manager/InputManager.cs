using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private PlayerInput _playerInput;

    public Vector2 NavigationInput { get; set; }
    private InputAction _navigationInput;
    public static PlayerInput PlayerInput { get; set; }
    protected override void Awake()
    {
        base.Awake();
        PlayerInput = GetComponent<PlayerInput>();
        _navigationInput = PlayerInput.actions["Navigate"];
    }

    private void Update() {
        NavigationInput = _navigationInput.ReadValue<Vector2>();
    }
}
