using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlowService
{
    private static string _pendingSpawnKey;
    
    public static event System.Action<Vector3> OnPlayerSpawned;

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

        SceneTransitionManager.Instance.TransitionToScene(scene, TransitionType.Death, FadeDirection.Right);
    }

    public static void SetSpawnKey(string key)
    {
        _pendingSpawnKey = key;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SlotScene" || scene.name == "New Scene" || scene.name == "MainMenu")
        {
            PlayerSpawnService.Clear();
            return;
        }

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
        {
            Debug.Log($"No save data loaded yet, skip spawn. savemanager: {SaveManager.Instance} SaveData: {SaveManager.Instance.CurrentData}");
            return;
        }

        Vector3 finalPlayerPos = Vector3.zero;

        if (!string.IsNullOrEmpty(_pendingSpawnKey))
        {
            var target = GameObject.Find(_pendingSpawnKey);
            if (target != null)
            {
                PlayerSpawnService.MoveTo(target.transform.position);
                finalPlayerPos = target.transform.position;
            }

            _pendingSpawnKey = null;
        }
        else if (!PlayerSpawnService.Exists())
        {
            var pos = SaveManager.Instance.CurrentData.player.position;
            PlayerSpawnService.GetOrCreate(pos, true);
            finalPlayerPos = pos;
        }

        SaveableRegistry.ApplyAll(SaveManager.Instance.CurrentData);
        
        // 🔥 THÔNG BÁO KHI PLAYER ĐÃ ĐƯỢC SPAWN XONG
        OnPlayerSpawned?.Invoke(finalPlayerPos);
    }
}
