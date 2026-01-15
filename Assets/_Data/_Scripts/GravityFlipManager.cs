using UnityEngine;
using System.Collections;
using DG.Tweening;
using GlobalEnums;
public class GravityFlipManager : Singleton<GravityFlipManager>
{
    public GravityDirection gravityDirection = GravityDirection.North;
    public CameraController cameraController;
    
    private void Start() {
        if (cameraController == null) cameraController = FindFirstObjectByType<CameraController>();
    }

    public void FlipGravity(GravityDirection newDir)
    {
        if (newDir == gravityDirection) return;
        StartCoroutine(FlipGravityRoutine(newDir));
    }

    private IEnumerator FlipGravityRoutine(GravityDirection newDir)
    {
        gravityDirection = newDir;
        if (cameraController != null)
        {
            Tween t = cameraController.RotateCameraToGravity(newDir);
            yield return t.WaitForCompletion();
        }
    }
}
