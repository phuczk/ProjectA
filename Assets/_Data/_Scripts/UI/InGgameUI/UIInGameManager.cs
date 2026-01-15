using UnityEngine;
using UnityEngine.InputSystem;
using GlobalEnums;

public class UIInGameManager : Singleton<UIInGameManager>
{
    [SerializeField] private GameStateChannel _stateChannel;
    private IBackHandler _pauseBackHandler;

    [Header("UI Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _mapPanel;

    private PlayerInput _playerInput;
    private InputAction _pauseAction;
    private InputAction _mapAction;
    private InputAction _inventoryAction;
    private InputAction _closeMenuUIAction;

    private float _lastInputTime;
    private const float INPUT_DELAY = 0.2f;

    protected override void Awake()
    {
        base.Awake();
        _playerInput = GetComponent<PlayerInput>();
        
        var inGameMap = _playerInput.actions.FindActionMap("InGame", true);
        _pauseAction = inGameMap.FindAction("MenuOpen", true);
        _mapAction = inGameMap.FindAction("Map", true);
        _inventoryAction = inGameMap.FindAction("Inventory", true);

        var uiMap = _playerInput.actions.FindActionMap("UI", true);
        _closeMenuUIAction = uiMap.FindAction("Cancel", true);

        _pauseBackHandler = _pausePanel.GetComponent<IBackHandler>();

        _playerInput.SwitchCurrentActionMap("UI");
        // Debug.Log($"current action map: {_playerInput.currentActionMap.name}");

    }

    private void OnEnable()
    {
        _pauseAction.performed += OnPausePerformed;
        _mapAction.performed += OnMapPerformed;
        _inventoryAction.performed += OnInventoryPerformed;
        _closeMenuUIAction.performed += OnCloseMenuUIPerformed;

        _stateChannel.OnStateRequested += HandleStateChange;
    }

    private void OnDisable()
    {
        _pauseAction.performed -= OnPausePerformed;
        _mapAction.performed -= OnMapPerformed;
        _inventoryAction.performed -= OnInventoryPerformed;
        _closeMenuUIAction.performed -= OnCloseMenuUIPerformed;

        _stateChannel.OnStateRequested -= HandleStateChange;
    }

    private bool IsInputAllowed()
    {
        if (Time.unscaledTime - _lastInputTime < INPUT_DELAY) return false;
        
        _lastInputTime = Time.unscaledTime;
        return true;
    }

    private void OnPausePerformed(InputAction.CallbackContext context) 
    {
        if (!IsInputAllowed()) return;
        TogglePanel(_pausePanel);
    }

    private void OnMapPerformed(InputAction.CallbackContext context) 
    {
        if (!IsInputAllowed()) return;
        TogglePanel(_mapPanel);
    }

    private void OnInventoryPerformed(InputAction.CallbackContext context) 
    {
        if (!IsInputAllowed()) return;
        TogglePanel(_inventoryPanel);
    }

    private void OnCloseMenuUIPerformed(InputAction.CallbackContext context)
    {
        if (!IsInputAllowed()) return;
        if (GameStateManager.Instance.GetCurrentState() != GameState.Pause) return;

        if (_pausePanel.activeSelf && _pauseBackHandler != null)
        {
            if (_pauseBackHandler.OnBack()) 
            {
                return;
            }
        }

        _stateChannel.RaiseRequest(GameState.Playing);
    }

    private void HandleStateChange(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            InternalCloseAll();
            _playerInput.SwitchCurrentActionMap("InGame");
        }
        else if (newState == GameState.Pause || newState == GameState.MainMenu)
        {
            _playerInput.SwitchCurrentActionMap("UI");
        }
    }

    private void TogglePanel(GameObject panel)
    {
        if (panel == null || GameStateManager.Instance.GetCurrentState() == GameState.MainMenu) return;

        if (panel.activeSelf)
        {
            _stateChannel.RaiseRequest(GameState.Playing);
        }
        else
        {
            InternalCloseAll(); 
            panel.SetActive(true);
            _stateChannel.RaiseRequest(GameState.Pause);
        }
    }

    private void InternalCloseAll()
    {
        if (_pausePanel) _pausePanel.SetActive(false);
        if (_inventoryPanel) _inventoryPanel.SetActive(false);
        if (_mapPanel) _mapPanel.SetActive(false);
    }

    public void CloseAllPanels() => _stateChannel.RaiseRequest(GameState.Playing);
}