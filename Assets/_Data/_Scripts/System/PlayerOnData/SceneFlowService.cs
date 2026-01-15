using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlowService
{
    private static string _pendingSpawnKey;

    static SceneFlowService()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void LoadScene(SaveData data)
    {
        string scene =
            string.IsNullOrEmpty(data.world.currentSceneName)
                ? "TestScene"
                : data.world.currentSceneName;

        SceneTransitionManager.Instance.TransitionToScene(scene);
    }

    public static void SetSpawnKey(string key)
    {
        _pendingSpawnKey = key;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(_pendingSpawnKey))
        {
            var target = GameObject.Find(_pendingSpawnKey);
            if (target != null)
                PlayerSpawnService.MoveTo(target.transform.position);

            _pendingSpawnKey = null;
        }
        else if (!PlayerSpawnService.Exists())
        {
            var pos = SaveManager.Instance.CurrentData.player.position;
            PlayerSpawnService.GetOrCreate(pos, true);
        }

        SaveableRegistry.ApplyAll(SaveManager.Instance.CurrentData);
    }
}
