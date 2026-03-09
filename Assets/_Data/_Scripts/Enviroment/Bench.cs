using UnityEngine;
using UnityEngine.SceneManagement;

public class Bench : Interactable
{
    private void OnPlayerRest()
    {
        MapManager.Instance?.OnSitAtBench();
        var mgr = SaveManager.Instance;
        var pos = transform.position;
        var rx = Mathf.Round(pos.x * 10f) / 10f;
        var ry = Mathf.Round(pos.y * 10f) / 10f;
        var scene = SceneManager.GetActiveScene().name;
        var money = LootManager.Instance.GetCurrentMoney();
        var bench = gameObject.name;
        if (mgr != null)
        {
            CheckpointService.Save(new Vector3(rx, ry, 0f), scene, bench);
        }
        else
        {
            var data = SaveSystem.Load();
            data.player.position = new Vector3(rx, ry, 0f);
            data.world.currentSceneName = scene;
            data.world.currentBench = bench;
            data.player.currentMoney = money;
            SaveSystem.Save(data);
        }
        
        if (PlayerRestState.Instance != null)
        {
            PlayerRestState.Instance.StartRest();
        }
    }

    protected override void OnInteract(Transform player)
    {
        if (player != null)
            player.position = transform.position;
        player.GetComponent<PlayerHealth>().ResetHealth();
        OnPlayerRest();
    }
}
