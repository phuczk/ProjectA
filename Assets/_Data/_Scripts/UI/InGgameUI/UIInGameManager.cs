using UnityEngine;
using UnityEngine.InputSystem;
using GlobalEnums;
using DG.Tweening;
using System.Linq;

public class UIInGameManager : Singleton<UIInGameManager>
{
    [SerializeField] private GameStateChannel _stateChannel;
    private IBackHandler _pauseBackHandler;

    [SerializeField] private BookUI _bookUI;

    [Header("Page Indices")]
    [SerializeField] private int _inventoryPageIndex = 0;

    [Header("UI Panels")]
    [SerializeField] private GameObject _pausePanel;
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
    }

    private void Start()
    {
        if (_playerInput != null && _playerInput.enabled)
        {
            _playerInput.SwitchCurrentActionMap("UI");
        }
    }

    private void OnEnable()
    {
        _playerInput?.ActivateInput();

        _playerInput?.SwitchCurrentActionMap("UI");

        if (_pauseAction != null)
            _pauseAction.performed += OnPausePerformed;
        if (_mapAction != null)
            _mapAction.performed += OnMapPerformed;
        if (_inventoryAction != null)
            _inventoryAction.performed += OnInventoryPerformed;
        if (_closeMenuUIAction != null)
            _closeMenuUIAction.performed += OnCloseMenuUIPerformed;

        if (_stateChannel != null)
            _stateChannel.OnStateRequested += HandleStateChange;
    }

    private void OnDisable()
    {
        if (_pauseAction != null)
            _pauseAction.performed -= OnPausePerformed;

        if (_mapAction != null)
            _mapAction.performed -= OnMapPerformed;

        if (_inventoryAction != null)
            _inventoryAction.performed -= OnInventoryPerformed;

        if (_closeMenuUIAction != null)
            _closeMenuUIAction.performed -= OnCloseMenuUIPerformed;

        if (_stateChannel != null)
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

    private void OnCloseMenuUIPerformed(InputAction.CallbackContext context)
    {
        if (!IsInputAllowed()) return;
        if (GameStateManager.Instance.GetCurrentState() != GameState.Pause) return;

        var allBackHandlers = FindObjectsOfType<MonoBehaviour>()
            .OfType<IBackHandler>()
            .Where(h => h as MonoBehaviour != null)
            .ToList();
        
        foreach (var handler in allBackHandlers)
        {
            var monoHandler = handler as MonoBehaviour;
            
            if (!monoHandler.gameObject.activeInHierarchy)
            {
                continue;
            }
            
            if (handler.OnBack())
            {
                return;
            }
        }

        _stateChannel.RaiseRequest(GameState.Playing);
    }

    private void OnMapPerformed(InputAction.CallbackContext context) 
    {
        if (!IsInputAllowed()) return;
        TogglePanel(_mapPanel);
    }

    private void OnInventoryPerformed(InputAction.CallbackContext context) 
    {
        if (!IsInputAllowed()) return;
        ToggleBookPage(_inventoryPageIndex);
    }

    private void OnDestroy() {
        DOTween.KillAll();
    }

    private void ToggleBookPage(int pageIndex)
    {
        if (_bookUI.gameObject.activeSelf)
        {
            _stateChannel.RaiseRequest(GameState.Playing);
        }
        else
        {
            _bookUI.OpenToPage(pageIndex);
            
            var selectionManager = _bookUI.GetComponent<UISelectionManager>();
            if (selectionManager != null)
            {
                selectionManager.InitializeFromChildren();
            }
            
            _stateChannel.RaiseRequest(GameState.Pause);
        }
    }

    private void HandleStateChange(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            _pausePanel.SetActive(false);
            _mapPanel.SetActive(false);
            _bookUI.gameObject.SetActive(false);
            _playerInput.SwitchCurrentActionMap("InGame");
        }
        else if (newState == GameState.Pause || newState == GameState.MainMenu)
        {
            _playerInput.SwitchCurrentActionMap("UI");
            if (newState == GameState.MainMenu)
            {
                _pausePanel.SetActive(false);
                _mapPanel.SetActive(false);
            }
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
            _bookUI.gameObject.SetActive(false); 
            panel.SetActive(true);
            
            var selectionManager = panel.GetComponent<UISelectionManager>();
            if (selectionManager != null)
            {
                selectionManager.InitializeFromChildren();
            }
            
            _stateChannel.RaiseRequest(GameState.Pause);
        }
    }
    
    public void DisableUI()
    {
        if (_pauseAction != null && _pauseAction.enabled)
            _pauseAction.Disable();
        if (_mapAction != null && _mapAction.enabled)
            _mapAction.Disable();
        if (_inventoryAction != null && _inventoryAction.enabled)
            _inventoryAction.Disable();
    }
    
    public void EnableUI()
    {
        if (_pauseAction != null && !_pauseAction.enabled)
            _pauseAction.Enable();
        if (_mapAction != null && !_mapAction.enabled)
            _mapAction.Enable();
        if (_inventoryAction != null && !_inventoryAction.enabled)
            _inventoryAction.Enable();
    }
}
