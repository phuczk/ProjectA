using UnityEngine;

public static class CheckpointService
{
    public static void Save(Vector3 pos, string scene, string bench)
    {
        var data = SaveManager.Instance.CurrentData;

        data.player.position = new Vector3(
            Mathf.Round(pos.x * 10f) / 10f,
            Mathf.Round(pos.y * 10f) / 10f,
            0f
        );

        data.world.currentSceneName = scene;
        data.world.currentBench = bench;

        SaveSystemz.Save(data);
    }
}
