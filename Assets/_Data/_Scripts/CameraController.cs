using UnityEngine;
using DG.Tweening;
using GlobalEnums;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    public System.Action OnCameraRotationComplete;
    public GameObject playerTarget;
    public Camera cam;
    
    [SerializeField] private CinemachineCamera _vcam;
    [SerializeField] private CinemachineConfiner2D _confiner2D;
    [SerializeField] private float rotationDurationPer90 = 0.15f;

    private Tween rotateTween;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        DOTween.Init();
        _vcam ??= GetComponent<CinemachineCamera>();
        if (_confiner2D == null && _vcam != null) _confiner2D = _vcam.GetComponent<CinemachineConfiner2D>();
        
        RefreshTarget();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SnapToPlayerRoutine());
    }

    private IEnumerator SnapToPlayerRoutine()
    {
        yield return new WaitForFixedUpdate();

        SnapToPlayerImmediate();
    }

    public void SnapToPlayerImmediate()
    {
        playerTarget = GameObject.FindGameObjectWithTag("CameraFollow");
        if (playerTarget == null) return;

        if (_vcam != null)
        {
            _vcam.Follow = playerTarget.transform;

            Vector3 targetPos = playerTarget.transform.position;
            targetPos.z = _vcam.transform.position.z;

            _vcam.transform.position = targetPos;
            _vcam.ForceCameraPosition(targetPos, _vcam.transform.rotation);

            _vcam.OnTargetObjectWarped(playerTarget.transform, Vector3.zero);
        }

        cam ??= Camera.main;
        if (cam != null)
        {
            Vector3 targetPos = playerTarget.transform.position;
            cam.transform.position = new Vector3(targetPos.x, targetPos.y, cam.transform.position.z);
        }
    }

    public void RefreshTarget()
    {
        SnapToPlayerImmediate();
    }

    public Tween RotateCameraToGravity(GravityDirection newDir)
    {
        float targetAngle = 0f;
        switch (newDir)
        {
            case GravityDirection.North: targetAngle = 0f; break;
            case GravityDirection.East:  targetAngle = -90f; break;
            case GravityDirection.South: targetAngle = 180f; break;
            case GravityDirection.West:  targetAngle = 90f; break;
        }

        float currentAngle = _vcam.transform.eulerAngles.z;
        float angleDelta = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
        float dynamicDuration = (angleDelta / 90f) * rotationDurationPer90;

        if (angleDelta < 0.1f) return null;

        if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();
        
        rotateTween = _vcam.transform
            .DORotate(new Vector3(0, 0, targetAngle), dynamicDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                OnCameraRotationComplete?.Invoke();
            });
        
        return rotateTween;
    }
}
