using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using GlobalEnums;
using DG.Tweening;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Camera Settings")]
    public CinemachineCamera[] allCameras;
    public System.Action OnAllCamerasRotated;
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
    [SerializeField] private float rotationDurationPer90 = 0.15f;

    public string CAMERA_TAG = "CameraFollow";

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
        SceneFlowService.OnPlayerSpawned += OnPlayerSpawned;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneFlowService.OnPlayerSpawned -= OnPlayerSpawned;
    }
    
    private void OnPlayerSpawned(Vector3 playerPos)
    {
        RefreshCameraSystemImmediate(playerPos);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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

    public void SwapCamera(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight, Vector2 triggerExitDirection)
    {
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

    public void RotateAllCameras(GravityDirection newDir)
    {
        allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        foreach (var vcam in allCameras)
        {
            float targetAngle = newDir switch
            {
                GravityDirection.North => 0f,
                GravityDirection.East => -90f,
                GravityDirection.South => 180f,
                GravityDirection.West => 90f,
                _ => 0f
            };

            vcam.transform.DORotate(new Vector3(0, 0, targetAngle), 0.15f).SetEase(Ease.OutQuad);
        }
    }

    public IEnumerator RotateAllCamerasRoutine(GravityDirection newDir)
    {
        var allVCams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        float targetAngle = GetAngleFromDirection(newDir);

        Sequence seq = DOTween.Sequence();

        foreach (var vcam in allVCams)
        {
            float currentAngle = vcam.transform.eulerAngles.z;
            float angleDelta = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

            float dynamicDuration = (angleDelta / 90f) * rotationDurationPer90;

            seq.Join(vcam.transform.DORotate(new Vector3(0, 0, targetAngle), dynamicDuration)
                .SetEase(Ease.OutQuad));
        }

        yield return seq.WaitForCompletion();
        OnAllCamerasRotated?.Invoke();
    }

    public float GetAngleFromDirection(GravityDirection dir)
    {
        return dir switch
        {
            GravityDirection.North => 0f,
            GravityDirection.East  => -90f,
            GravityDirection.South => 180f,
            GravityDirection.West  => 90f,
            _ => 0f
        };
    }

    public void OnPlayerRespawn(Vector3 respawnPos)
    {
        RefreshCameraSystem(respawnPos);
    }
    
    public void RefreshCameraSystem(Vector3 playerPos)
    {
        RefreshCameraSystemImmediate(playerPos);
    }
    
    public void RefreshCameraSystemImmediate(Vector3 playerPos)
    {
        allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        
        GravityDirection currentDir = GravityFlipManager.Instance.gravityDirection;
        float targetAngle = GetAngleFromDirection(currentDir);

        CinemachineCamera closestCam = null;
        float minDistance = float.MaxValue;

        foreach (var vcam in allCameras)
        {
            vcam.enabled = false; 
            
            float dist = Vector3.Distance(vcam.transform.position, playerPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestCam = vcam;
            }
        }

        if (closestCam != null)
        {
            closestCam.transform.position = new Vector3(playerPos.x, playerPos.y, closestCam.transform.position.z);
            closestCam.transform.rotation = Quaternion.Euler(0, 0, targetAngle);

            GameObject player = GameObject.FindGameObjectWithTag(CAMERA_TAG);
            if (player != null) closestCam.Follow = player.transform;

            closestCam.enabled = true;
            UpdateCurrentCameraReferences(closestCam);

            closestCam.ForceCameraPosition(closestCam.transform.position, closestCam.transform.rotation);
            
            var confiner = closestCam.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
            {
                confiner.InvalidateBoundingShapeCache();
            }

            StartCoroutine(DelayedCameraRefresh(closestCam));
        }
    }
    
    private IEnumerator DelayedCameraRefresh(CinemachineCamera cam)
    {
        yield return new WaitForFixedUpdate();
        
        GameObject player = GameObject.FindGameObjectWithTag(CAMERA_TAG);
        if (player != null && cam != null)
        {
            cam.Follow = player.transform;
            cam.ForceCameraPosition(player.transform.position, cam.transform.rotation);
        }
    }
}
