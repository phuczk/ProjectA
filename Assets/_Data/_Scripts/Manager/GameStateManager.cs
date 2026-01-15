using GlobalEnums;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    [SerializeField] private GameStateChannel _stateChannel;

    private GameState _currentState;

    protected override void Awake() {
        base.Awake();
        _currentState = GameState.MainMenu;
    }

    private void OnEnable()
    {
        _stateChannel.OnStateRequested += HandleStateChange;
    }

    private void OnDisable()
    {
        _stateChannel.OnStateRequested -= HandleStateChange;
    }

    public GameState GetCurrentState()
    {
        return _currentState;
    }

    private void HandleStateChange(GameState newState)
    {
        Time.timeScale = (newState == GameState.Pause) ? 0f : 1f;
        _currentState = newState;
        Debug.Log($"State changed to: {newState}");
    }
}