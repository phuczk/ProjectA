using UnityEngine;
using GlobalEnums;

[CreateAssetMenu(menuName = "Events/GameStateChannel")]
public class GameStateChannel : ScriptableObject
{
    public System.Action<GameState> OnStateRequested;

    public void RaiseRequest(GameState newState)
    {
        OnStateRequested?.Invoke(newState);
    }
}