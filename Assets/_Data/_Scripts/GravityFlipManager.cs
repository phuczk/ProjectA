using UnityEngine;
using System.Collections;
using DG.Tweening;
using GlobalEnums;

public class GravityFlipManager : Singleton<GravityFlipManager>
{
    public GravityDirection gravityDirection = GravityDirection.North;

    public void FlipGravity(GravityDirection newDir)
    {
        if (newDir == gravityDirection) return;
        
        // Cập nhật hướng trọng lực vật lý của Unity (nếu bạn dùng Physics2D)
        UpdatePhysicsGravity(newDir);
        
        StartCoroutine(FlipGravityRoutine(newDir));
    }

    private IEnumerator FlipGravityRoutine(GravityDirection newDir)
    {
        gravityDirection = newDir;

        // Gọi CameraManager xoay TẤT CẢ các camera có trong Scene
        if (CameraManager.Instance != null)
        {
            // Chúng ta sẽ sửa hàm này trong CameraManager để trả về một list các Tween hoặc một Sequence
            yield return CameraManager.Instance.RotateAllCamerasRoutine(newDir);
        }
        
        Debug.Log($"Gravity flipped to: {newDir}");
    }

    private void UpdatePhysicsGravity(GravityDirection dir)
    {
        float gravityMag = Physics2D.gravity.magnitude;
        Physics2D.gravity = dir switch
        {
            GravityDirection.North => new Vector2(0, 9.81f),
            GravityDirection.South => new Vector2(0, -9.81f),
            GravityDirection.East  => new Vector2(9.81f, 0),
            GravityDirection.West  => new Vector2(-9.81f, 0),
            _ => Physics2D.gravity
        };
    }
}
