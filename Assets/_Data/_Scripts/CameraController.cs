using UnityEngine;
using System.Collections;
using DG.Tweening;
using GlobalEnums;

public class CameraController : Singleton<CameraController>
{
    //public static CameraController Instance { get; private set; }

	public System.Action OnCameraRotationComplete;
	public Vector3 centerTarget;
	public GameObject playerTarget;
	public static bool followPlayer = true;
	public Camera cam;

	public int minZoom = 5;
	public int maxZoom = 20;

	private Tween moveTween;
	private Tween zoomTween;
    private Tween rotateTween;

	private void Start()
	{
		DOTween.Init();
	}

	void LateUpdate()
    {
		HandleFollow();
	}

    [SerializeField] private float rotationDurationPer90 = 0.15f;

    public Tween RotateCameraToGravity(GravityDirection newDir)
    {
        float targetAngle = 0f;

        switch (newDir)
        {
            case GravityDirection.North: targetAngle = 0f; break;
            case GravityDirection.East:  targetAngle = -90f; break;
            case GravityDirection.South: targetAngle = 180f; break;
            case GravityDirection.West:  targetAngle = 90f; break;
        }

        float currentAngle = transform.eulerAngles.z;
        float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
        float duration = Mathf.Abs(delta) / 90f * rotationDurationPer90;
        duration = Mathf.Max(0.1f, duration);

        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill(true);
        }

        rotateTween = transform
            .DORotate(new Vector3(0, 0, targetAngle), duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                OnCameraRotationComplete?.Invoke();
            });
        
        return rotateTween;
    }

	void HandleFollow()
	{
        if (!followPlayer) return;
        if (playerTarget == null) playerTarget = GameObject.FindGameObjectWithTag("Player");

		transform.position = new Vector3(
			playerTarget.transform.position.x,
			playerTarget.transform.position.y,
			-10
		);
	}
}
