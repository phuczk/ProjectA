// using UnityEngine;
// using DG.Tweening;
// using GlobalEnums;
// using Unity.Cinemachine;
// using UnityEngine.SceneManagement;
// using System.Collections;

// /// <summary>
// /// Improved CameraController that works with CameraSystemManager
// /// Maintains rotation ability across camera switches
// /// </summary>
// public class ImprovedCameraController : MonoBehaviour
// {
//     [Header("Camera Components")]
//     [SerializeField] private CinemachineCamera _vcam;
//     [SerializeField] private CinemachineConfiner2D _confiner2D;
//     [SerializeField] private float rotationDurationPer90 = 0.15f;

//     [Header("Target Management")]
//     [SerializeField] private string playerTag = "CameraFollow";
    
//     // Runtime
//     private Tween _rotateTween;
//     private GameObject _playerTarget;
//     private bool _isInitialized = false;

//     public System.Action<ImprovedCameraController> OnCameraActivated;
//     public System.Action OnCameraRotationComplete;

//     private void Awake()
//     {
//         InitializeComponents();
//     }

//     private void OnEnable()
//     {
//         // Subscribe to camera system events
//         if (CameraSystemManager.Instance != null)
//         {
//             CameraSystemManager.Instance.OnZoneChanged += HandleZoneChanged;
//         }
//     }

//     private void OnDisable()
//     {
//         // Unsubscribe from events
//         if (CameraSystemManager.Instance != null)
//         {
//             CameraSystemManager.Instance.OnZoneChanged -= HandleZoneChanged;
//         }
        
//         // Kill any active tweens
//         _rotateTween?.Kill();
//     }

//     private void InitializeComponents()
//     {
//         _vcam ??= GetComponent<CinemachineCamera>();
//         if (_confiner2D == null && _vcam != null) 
//             _confiner2D = _vcam.GetComponent<CinemachineConfiner2D>();
        
//         DOTween.Init();
//     }

//     private void HandleZoneChanged(CameraZoneData newZone)
//     {
//         // Khi zone thay đổi và camera này được activate
//         if (newZone.camera == _vcam)
//         {
//             OnCameraActivated?.Invoke(this);
//             RefreshTarget();
//         }
//     }

//     #region Target Management

//     public void RefreshTarget()
//     {
//         if (_playerTarget == null)
//         {
//             _playerTarget = GameObject.FindGameObjectWithTag(playerTag);
//         }

//         if (_playerTarget == null || _vcam == null) return;

//         // Set follow target
//         _vcam.Follow = _playerTarget.transform;

//         // Force immediate position update
//         Vector3 targetPos = _playerTarget.transform.position;
//         targetPos.z = _vcam.transform.position.z;

//         _vcam.transform.position = targetPos;
//         _vcam.ForceCameraPosition(targetPos, _vcam.transform.rotation);

//         _vcam.OnTargetObjectWarped(_playerTarget.transform, Vector3.zero);

//         _isInitialized = true;
//     }

//     public void SetTarget(GameObject newTarget)
//     {
//         _playerTarget = newTarget;
//         RefreshTarget();
//     }

//     #endregion

//     #region Camera Rotation

//     public Tween RotateCameraToGravity(GravityDirection newDir)
//     {
//         if (_vcam == null)
//         {
//             Debug.LogWarning("No CinemachineCamera assigned for rotation!");
//             return null;
//         }

//         float targetAngle = GetTargetAngle(newDir);
//         float currentAngle = _vcam.transform.eulerAngles.z;
//         float angleDelta = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
        
//         if (angleDelta < 0.1f) return null;

//         float dynamicDuration = (angleDelta / 90f) * rotationDurationPer90;

//         // Kill existing rotation
//         if (_rotateTween != null && _rotateTween.IsActive()) 
//             _rotateTween.Kill();
        
//         // Start new rotation
//         _rotateTween = _vcam.transform
//             .DORotate(new Vector3(0, 0, targetAngle), dynamicDuration)
//             .SetEase(Ease.OutQuad)
//             .OnComplete(() => 
//             {
//                 OnCameraRotationComplete?.Invoke();
//             });
        
//         return _rotateTween;
//     }

//     private float GetTargetAngle(GravityDirection direction)
//     {
//         switch (direction)
//         {
//             case GravityDirection.North: return 0f;
//             case GravityDirection.East:  return -90f;
//             case GravityDirection.South: return 180f;
//             case GravityDirection.West:  return 90f;
//             default: return 0f;
//         }
//     }

//     #endregion

//     #region Camera Effects

//     public void ShakeCamera(float duration, float strength)
//     {
//         if (_vcam == null) return;

//         // Implement camera shake using DOTween or Cinemachine Impulse
//         _vcam.transform
//             .DOShakePosition(duration, new Vector3(strength, strength, 0))
//             .SetEase(Ease.OutQuad);
//     }

//     public void ZoomCamera(float targetOrthoSize, float duration)
//     {
//         if (_vcam == null) return;

//         DOTween.To(() => _vcam.Lens.OrthographicSize, 
//             x => _vcam.Lens.OrthographicSize = x, 
//             targetOrthoSize, duration)
//             .SetEase(Ease.OutQuad);
//     }

//     #endregion

//     #region Public API

//     public CinemachineCamera GetCamera() => _vcam;
//     public GameObject GetTarget() => _playerTarget;
//     public bool IsInitialized() => _isInitialized;
//     public bool IsRotating() => _rotateTween != null && _rotateTween.IsActive();

//     /// <summary>
//     /// Force camera to snap to target immediately
//     /// </summary>
//     public void SnapToTargetImmediate()
//     {
//         RefreshTarget();
//     }

//     /// <summary>
//     /// Stop any ongoing rotation
//     /// </summary>
//     public void StopRotation()
//     {
//         if (_rotateTween != null && _rotateTween.IsActive())
//         {
//             _rotateTween.Kill();
//         }
//     }

//     #endregion

//     #region Editor Helpers

//     private void OnDrawGizmosSelected()
//     {
//         if (_vcam != null)
//         {
//             // Draw camera view area
//             Gizmos.color = Color.cyan;
//             float orthoSize = _vcam.Lens.OrthographicSize;
//             float aspectRatio = (float)Screen.width / Screen.height;
            
//             Vector3 size = new Vector3(orthoSize * 2 * aspectRatio, orthoSize * 2, 1);
//             Gizmos.DrawWireCube(_vcam.transform.position, size);
//         }
//     }

//     [ContextMenu("Find Player Target")]
//     public void FindPlayerTarget()
//     {
//         _playerTarget = GameObject.FindGameObjectWithTag(playerTag);
//         if (_playerTarget != null)
//         {
//             Debug.Log($"Found player target: {_playerTarget.name}");
//         }
//         else
//         {
//             Debug.LogWarning($"No object found with tag: {playerTag}");
//         }
//     }

//     #endregion
// }
