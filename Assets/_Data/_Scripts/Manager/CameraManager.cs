using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private CinemachineCamera[] allCameras;
    [SerializeField] private float _fallPanAmount = 0.5f;
    [SerializeField] private float _fallYPanTime = 0.5f;
    private float _normalYDamping;

    public float fallSpeed = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYDampingCoroutine;
    private Coroutine _panCameraCoroutine;

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer positionComposer;

    private Vector2 _startingTrackedObjectOffset;

    // ================================
    // UNITY LIFECYCLE
    // ================================

    protected override void Awake()
    {
        base.Awake();
        FindActiveCamera();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Scan lại camera khi load scene mới
        FindActiveCamera();
    }

    // ================================
    // CAMERA SETUP
    // ================================

    private void FindActiveCamera()
    {
        allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        for (int i = 0; i < allCameras.Length; i++)
        {
            if (allCameras[i] && allCameras[i].enabled)
            {
                UpdateCurrentCameraReferences(allCameras[i]);
                break;
            }
        }

        if (positionComposer != null)
        {
            _startingTrackedObjectOffset =
                new Vector2(positionComposer.TargetOffset.x, positionComposer.TargetOffset.y);
        }
    }

    private void UpdateCurrentCameraReferences(CinemachineCamera newCamera)
    {
        if (!newCamera) return;

        currentCamera = newCamera;

        positionComposer = currentCamera.GetComponent<CinemachinePositionComposer>();
        if (positionComposer == null)
            positionComposer = currentCamera.GetComponentInChildren<CinemachinePositionComposer>();

        if (positionComposer != null)
        {
            _normalYDamping = positionComposer.Damping.y;
            Debug.Log($"Camera {currentCamera.name} uses PositionComposer, Y damping: {_normalYDamping}");
        }
        else
        {
            Debug.LogWarning($"Camera {currentCamera.name} has no PositionComposer or FramingTransposer Body component.");
        }
    }

    // ================================
    // PAN CAMERA
    // ================================

    public void PanCameraContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartPos)
    {
        if (_panCameraCoroutine != null)
            StopCoroutine(_panCameraCoroutine);

        _panCameraCoroutine = StartCoroutine(
            PanCameraCoroutine(panDistance, panTime, panDirection, panToStartPos));
    }

    private IEnumerator PanCameraCoroutine(float panDistance, float panTime, PanDirection panDirection, bool panToStartPos)
    {
        if (positionComposer == null)
            yield break;

        Vector2 endPos = Vector2.zero;
        Vector2 startingPos;

        if (panToStartPos)
        {
            switch (panDirection)
            {
                case PanDirection.Left: endPos = Vector2.left; break;
                case PanDirection.Right: endPos = Vector2.right; break;
                case PanDirection.Up: endPos = Vector2.up; break;
                case PanDirection.Down: endPos = Vector2.down; break;
            }

            endPos *= panDistance;
            startingPos = _startingTrackedObjectOffset;
        }
        else
        {
            startingPos = new Vector2(positionComposer.TargetOffset.x, positionComposer.TargetOffset.y);
            endPos = _startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;

        while (elapsedTime < panTime)
        {
            elapsedTime += Time.fixedDeltaTime;

            Vector2 lerp = Vector2.Lerp(startingPos, endPos, elapsedTime / panTime);
            var current = positionComposer.TargetOffset;
            positionComposer.TargetOffset = new Vector3(lerp.x, lerp.y, current.z);

            yield return null;
        }
    }

    // ================================
    // Y DAMPING
    // ================================

    public void LerpYDamping(bool isPlayerFalling)
    {
        if (_lerpYDampingCoroutine != null)
            StopCoroutine(_lerpYDampingCoroutine);

        _lerpYDampingCoroutine = StartCoroutine(LerpYDampingCoroutine(isPlayerFalling));
    }

    private IEnumerator LerpYDampingCoroutine(bool isPlayerFalling)
    {
        if (positionComposer == null)
            yield break;

        IsLerpingYDamping = true;

        float startDamping =
            positionComposer != null
                ? positionComposer.Damping.y
                : _normalYDamping;

        float endDampingAmount = isPlayerFalling ? _fallPanAmount : _normalYDamping;
        float elapsedTime = 0f;

        if (isPlayerFalling)
            LerpedFromPlayerFalling = true;

        while (elapsedTime < _fallYPanTime)
        {
            elapsedTime += Time.fixedDeltaTime;

            float lerpPanAmount =
                Mathf.Lerp(startDamping, endDampingAmount, elapsedTime / _fallYPanTime);

            if (positionComposer != null)
            {
                var d = positionComposer.Damping;
                positionComposer.Damping = new Vector3(d.x, lerpPanAmount, d.z);
            }
            else
            {
                _normalYDamping = lerpPanAmount;
            }

            yield return null;
        }

        if (positionComposer != null)
        {
            var d = positionComposer.Damping;
            positionComposer.Damping = new Vector3(d.x, endDampingAmount, d.z);
        }
        else
        {
            _normalYDamping = endDampingAmount;
        }

        IsLerpingYDamping = false;
    }

    // ================================
    // SWAP CAMERA (SAFE VERSION)
    // ================================

    public void SwapCamera(
        CinemachineCamera cameraFromLeft,
        CinemachineCamera cameraFromRight,
        Vector2 triggerExitDirection)
    {
        // 🔥 Unity destroyed object safe check
        if (!cameraFromLeft || !cameraFromRight)
            return;

        if (cameraFromLeft.enabled && triggerExitDirection.x > 0f)
        {
            cameraFromLeft.enabled = false;
            cameraFromRight.enabled = true;
            UpdateCurrentCameraReferences(cameraFromRight);
        }
        else if (cameraFromRight.enabled && triggerExitDirection.x < 0f)
        {
            cameraFromRight.enabled = false;
            cameraFromLeft.enabled = true;
            UpdateCurrentCameraReferences(cameraFromLeft);
        }
    }
}
