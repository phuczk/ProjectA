using UnityEngine;
using UnityEngine.SceneManagement;

public class Bench : Interactable
{
    private void OnPlayerRest()
    {
        var mgr = SaveManager.Instance;
        var pos = transform.position;
        var rx = Mathf.Round(pos.x * 10f) / 10f;
        var ry = Mathf.Round(pos.y * 10f) / 10f;
        var scene = SceneManager.GetActiveScene().name;
        var bench = gameObject.name;
        if (mgr != null)
        {
            CheckpointService.Save(new Vector3(rx, ry, 0f), scene, bench);
        }
        else
        {
            var data = SaveSystemz.Load();
            data.player.position = new Vector3(rx, ry, 0f);
            data.world.currentSceneName = scene;
            data.world.currentBench = bench;
            SaveSystemz.Save(data);
        }
    }

    protected override void OnInteract(Transform player)
    {
        if (player != null)
            player.position = transform.position;
        OnPlayerRest();
    }
}
