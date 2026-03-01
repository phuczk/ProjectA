
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string sceneName;
    public string spawnKey;
    public FadeDirection fadeDirection;
    public TransitionType transitionType;
    
    public void ChangeToScene(FadeDirection direction)
    {
        var mgr = SaveManager.Instance;
        if (mgr != null && !string.IsNullOrEmpty(spawnKey))
            SceneFlowService.SetSpawnKey(spawnKey);
        
        SceneTransitionManager.Instance.TransitionToScene(sceneName, transitionType, direction);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            ChangeToScene(fadeDirection);
        }
    }
}

public enum FadeDirection
{
    Up,
    Down,
    Left,
    Right
}
