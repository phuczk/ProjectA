// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine.SceneManagement;
// using Unity.Cinemachine;
// using GlobalEnums;
// using DG.Tweening;

// /// <summary>
// /// Centralized camera system that manages all cameras across scenes
// /// Handles camera switching, rotation, and respawn logic
// /// </summary>
// public class CameraSystemManager : Singleton<CameraSystemManager>
// {
//     [Header("Camera Management")]
//     [SerializeField] private float cameraSwitchDuration = 1f;
//     [SerializeField] private float rotationDurationPer90 = 0.15f;
    
//     [Header("Cross-Scene Settings")]
//     [SerializeField] private bool enableCrossSceneRespawn = true;
//     [SerializeField] private string defaultRespawnScene = "MainScene"; // Scene name fallback
    
//     // Runtime data
//     private Dictionary<string, CameraZoneData> _cameraZones = new Dictionary<string, CameraZoneData>();
//     private string _currentZoneId = "";
//     private CinemachineCamera _currentActiveCamera;
//     private CameraController _currentController;
//     private Tween _cameraSwitchTween;
//     private Tween _rotationTween;
//     private string _lastKnownSceneName = "";

//     public System.Action<CameraZoneData> OnZoneChanged;
//     public System.Action OnCameraRotationComplete;
//     public System.Action<string> OnSceneCameraSwitch; // For cross-scene events

//     private void Awake()
//     {
//         base.Awake();
//         InitializeCameraSystem();
//     }

//     private void OnEnable()
//     {
//         SceneManager.sceneLoaded += OnSceneLoaded;
//         PlayerController.OnPlayerRespawn += HandlePlayerRespawn;
//     }

//     private void OnDisable()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//         PlayerController.OnPlayerRespawn -= HandlePlayerRespawn;
//     }

//     private void InitializeCameraSystem()
//     {
//         // Tự động tìm tất cả camera zones trong scene
//         var zones = FindObjectsByType<CameraZone>(FindObjectsSortMode.None);
        
//         foreach (var zone in zones)
//         {
//             RegisterCameraZone(zone);
//         }

//         // Tìm camera active hiện tại
//         FindCurrentActiveCamera();
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         string currentSceneName = scene.name;
//         Debug.Log($"CameraSystemManager: Scene loaded - {currentSceneName}");
        
//         // Reset lại system khi load scene mới
//         InitializeCameraSystem();
        
//         // Check for pending respawn after scene load
//         CheckForPendingRespawn();
        
//         // Track scene changes
//         if (_lastKnownSceneName != currentSceneName)
//         {
//             Debug.Log($"CameraSystemManager: Scene changed from {_lastKnownSceneName} to {currentSceneName}");
//             _lastKnownSceneName = currentSceneName;
            
//             // Trigger scene change event
//             OnSceneCameraSwitch?.Invoke(currentSceneName);
//         }
//     }

//     private void HandlePlayerRespawn(Vector3 respawnPosition)
//     {
//         Debug.Log($"CameraSystemManager handling respawn at: {respawnPosition}");
        
//         // Check if respawn position is in current scene
//         bool respawnInCurrentScene = IsPositionInCurrentScene(respawnPosition);
        
//         if (!respawnInCurrentScene && enableCrossSceneRespawn)
//         {
//             HandleCrossSceneRespawn(respawnPosition);
//             return;
//         }
        
//         // Normal respawn trong current scene
//         HandleNormalRespawn(respawnPosition);
//     }
    
//     private bool IsPositionInCurrentScene(Vector3 position)
//     {
//         // Simple check: if position is way too far from any camera zone, likely different scene
//         foreach (var kvp in _cameraZones)
//         {
//             var zone = kvp.Value;
//             float distance = Vector3.Distance(position, zone.bounds.center);
            
//             // If position is reasonable distance from a zone, assume same scene
//             if (distance < zone.bounds.size.magnitude * 2)
//             {
//                 return true;
//             }
//         }
        
//         return false;
//     }
    
//     private void HandleCrossSceneRespawn(Vector3 respawnPosition)
//     {
//         Debug.LogWarning($"Respawn position {respawnPosition} appears to be in different scene!");
        
//         // Try to determine which scene contains this respawn position
//         string targetSceneName = DetermineSceneForPosition(respawnPosition);
        
//         if (!string.IsNullOrEmpty(targetSceneName) && targetSceneName != SceneManager.GetActiveScene().name)
//         {
//             Debug.Log($"Switching to scene {targetSceneName} for respawn");
            
//             // Save respawn position for after scene load
//             PlayerPrefs.SetFloat("RespawnX", respawnPosition.x);
//             PlayerPrefs.SetFloat("RespawnY", respawnPosition.y);
//             PlayerPrefs.SetFloat("RespawnZ", respawnPosition.z);
//             PlayerPrefs.SetString("PendingRespawn", "true");
//             PlayerPrefs.Save();
            
//             // Load target scene
//             SceneManager.LoadScene(targetSceneName);
//         }
//         else
//         {
//             // Fallback: use default scene
//             Debug.LogWarning($"Could not determine scene for respawn position, using default: {defaultRespawnScene}");
//             SceneManager.LoadScene(defaultRespawnScene);
//         }
//     }
    
//     private void HandleNormalRespawn(Vector3 respawnPosition)
//     {
//         // Khi player respawn, tìm camera zone cho vị trí respawn
//         string respawnZoneId = GetZoneIdForPosition(respawnPosition);
        
//         if (!string.IsNullOrEmpty(respawnZoneId))
//         {
//             Debug.Log($"Switching to respawn camera zone: {respawnZoneId}");
//             SwitchToZone(respawnZoneId, instant: true); // Instant switch cho respawn
//         }
//         else
//         {
//             Debug.LogWarning($"No camera zone found for respawn position {respawnPosition}");
            
//             // Fallback: Tìm zone gần nhất
//             SwitchToNearestZone(respawnPosition);
//         }
        
//         // Force refresh camera sau respawn
//         if (_currentController != null)
//         {
//             _currentController.RefreshTarget();
//         }
//     }
    
//     private string DetermineSceneForPosition(Vector3 position)
//     {
//         // This is a simplified approach - you might want to implement:
//         // 1. Scene registry system
//         // 2. Position-based scene mapping
//         // 3. Save data that tracks which scene each bench belongs to
        
//         // For now, try to get scene info from save data
//         var saveData = SaveSystemz.Load();
//         if (saveData?.player != null)
//         {
//             // You might store scene name in save data
//             // For now, return current scene as fallback
//             return SceneManager.GetActiveScene().name;
//         }
        
//         return "";
//     }
    
//     private void CheckForPendingRespawn()
//     {
//         if (PlayerPrefs.GetString("PendingRespawn", "false") == "true")
//         {
//             Debug.Log("Processing pending respawn after scene load");
            
//             // Get saved respawn position
//             Vector3 savedPosition = new Vector3(
//                 PlayerPrefs.GetFloat("RespawnX", 0),
//                 PlayerPrefs.GetFloat("RespawnY", 0),
//                 PlayerPrefs.GetFloat("RespawnZ", 0)
//             );
            
//             // Clear pending respawn flag
//             PlayerPrefs.SetString("PendingRespawn", "false");
//             PlayerPrefs.Save();
            
//             // Handle respawn in new scene
//             HandleNormalRespawn(savedPosition);
//         }
//     }

//     #region Camera Zone Management

//     public void RegisterCameraZone(CameraZone zone)
//     {
//         if (zone == null || string.IsNullOrEmpty(zone.ZoneId)) return;

//         var zoneData = new CameraZoneData
//         {
//             zoneId = zone.ZoneId,
//             camera = zone.VirtualCamera,
//             controller = zone.CameraController,
//             bounds = zone.ZoneBounds,
//             priority = zone.Priority,
//             sceneName = zone.SceneName,
//             isDefaultZone = zone.IsDefaultZone
//         };

//         _cameraZones[zone.ZoneId] = zoneData;
        
//         Debug.Log($"Registered camera zone: {zone.ZoneId} in scene: {zone.SceneName}");
//     }

//     public void UnregisterCameraZone(string zoneId)
//     {
//         if (_cameraZones.ContainsKey(zoneId))
//         {
//             _cameraZones.Remove(zoneId);
//             Debug.Log($"Unregistered camera zone: {zoneId}");
//         }
//     }

//     #endregion

//     #region Camera Switching

//     public void SwitchToZone(string zoneId, bool instant = false)
//     {
//         if (!_cameraZones.ContainsKey(zoneId))
//         {
//             Debug.LogWarning($"Zone {zoneId} not found!");
//             return;
//         }

//         var targetZoneData = _cameraZones[zoneId];
        
//         if (targetZoneData.camera == _currentActiveCamera)
//         {
//             Debug.Log($"Already in zone {zoneId}");
//             return;
//         }

//         // Disable camera cũ
//         if (_currentActiveCamera != null)
//         {
//             _currentActiveCamera.Priority = 0;
//         }

//         // Enable camera mới
//         targetZoneData.camera.Priority = 10;
//         _currentActiveCamera = targetZoneData.camera;
//         _currentController = targetZoneData.controller;
//         _currentZoneId = zoneId;

//         // Update controller references
//         UpdateControllerReferences();

//         if (!instant)
//         {
//             PlayCameraSwitchAnimation();
//         }
//         else
//         {
//             // Instant switch - force immediate camera update
//             if (_currentController != null)
//             {
//                 _currentController.SnapToTargetImmediate();
//             }
//         }

//         OnZoneChanged?.Invoke(targetZoneData);
//         Debug.Log($"Switched to camera zone: {zoneId} (instant: {instant})");
//     }

//     public void SwitchToNearestZone(Vector3 position)
//     {
//         string nearestZoneId = GetZoneIdForPosition(position);
        
//         if (!string.IsNullOrEmpty(nearestZoneId) && nearestZoneId != _currentZoneId)
//         {
//             SwitchToZone(nearestZoneId);
//         }
//     }

//     private void FindCurrentActiveCamera()
//     {
//         var allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        
//         foreach (var cam in allCameras)
//         {
//             if (cam.enabled && cam.Priority > 0)
//             {
//                 _currentActiveCamera = cam;
//                 _currentController = cam.GetComponent<CameraController>();
                
//                 // Tìm zone ID cho camera này
//                 _currentZoneId = GetZoneIdForCamera(cam);
                
//                 UpdateControllerReferences();
//                 break;
//             }
//         }
        
//         // If no active camera found, try to find default zone for current scene
//         if (_currentActiveCamera == null)
//         {
//             ActivateDefaultZoneForCurrentScene();
//         }
//     }
    
//     private void ActivateDefaultZoneForCurrentScene()
//     {
//         string currentScene = SceneManager.GetActiveScene().name;
        
//         foreach (var kvp in _cameraZones)
//         {
//             var zone = kvp.Value;
//             if (zone.isDefaultZone && zone.sceneName == currentScene)
//             {
//                 Debug.Log($"Activating default zone {zone.zoneId} for scene {currentScene}");
//                 SwitchToZone(zone.zoneId, instant: true);
//                 return;
//             }
//         }
        
//         Debug.LogWarning($"No default zone found for scene {currentScene}");
//     }

//     private void UpdateControllerReferences()
//     {
//         if (_currentController != null)
//         {
//             // Đảm bảo controller có đúng camera reference
//             _currentController.RefreshTarget();
//         }
//     }

//     private void RefreshCurrentCamera()
//     {
//         if (_currentController != null)
//         {
//             _currentController.RefreshTarget();
//         }
//         else if (_currentActiveCamera != null)
//         {
//             // Tìm lại controller nếu lost
//             _currentController = _currentActiveCamera.GetComponent<CameraController>();
//             if (_currentController != null)
//             {
//                 _currentController.RefreshTarget();
//             }
//         }
//     }

//     #endregion

//     #region Camera Rotation

//     public Tween RotateCameraToGravity(GravityDirection newDir)
//     {
//         if (_currentController == null)
//         {
//             Debug.LogWarning("No active camera controller for rotation!");
//             return null;
//         }

//         // Sử dụng controller hiện tại để rotate
//         var rotationTween = _currentController.RotateCameraToGravity(newDir);
        
//         // Setup completion callback
//         if (rotationTween != null)
//         {
//             rotationTween.OnComplete(() => OnCameraRotationComplete?.Invoke());
//         }

//         return rotationTween;
//     }

//     #endregion

//     #region Zone Detection

//     private string GetZoneIdForPosition(Vector3 position)
//     {
//         foreach (var kvp in _cameraZones)
//         {
//             if (kvp.Value.bounds.Contains(position))
//             {
//                 return kvp.Key;
//             }
//         }
//         return "";
//     }

//     private string GetZoneIdForCamera(CinemachineCamera camera)
//     {
//         foreach (var kvp in _cameraZones)
//         {
//             if (kvp.Value.camera == camera)
//             {
//                 return kvp.Key;
//             }
//         }
//         return "";
//     }

//     #endregion

//     #region Animations

//     private void PlayCameraSwitchAnimation()
//     {
//         // Có thể thêm fade effect hoặc transition animation ở đây
//         // Hiện tại chỉ đơn giản là switch
//     }

//     #endregion

//     #region Public API

//     public string GetCurrentZoneId() => _currentZoneId;
    
//     public CameraZoneData GetCurrentZoneData()
//     {
//         if (!string.IsNullOrEmpty(_currentZoneId) && _cameraZones.ContainsKey(_currentZoneId))
//         {
//             return _cameraZones[_currentZoneId];
//         }
//         return null;
//     }

//     public List<CameraZoneData> GetAllZones()
//     {
//         return _cameraZones.Values.ToList();
//     }

//     #endregion
// }

// /// <summary>
// /// Data structure for camera zone information
// /// </summary>
// [System.Serializable]
// public class CameraZoneData
// {
//     public string zoneId;
//     public CinemachineCamera camera;
//     public CameraController controller;
//     public Bounds bounds;
//     public int priority;
//     public string sceneName;
//     public bool isDefaultZone;
// }
