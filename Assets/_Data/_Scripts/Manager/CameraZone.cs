// using UnityEngine;
// using Unity.Cinemachine;

// /// <summary>
// /// Component that defines a camera zone in the scene
// /// Attach this to GameObjects that represent camera areas
// /// </summary>
// public class CameraZone : MonoBehaviour
// {
//     [Header("Zone Configuration")]
//     [SerializeField] private string zoneId = "";
//     [SerializeField] private CinemachineCamera virtualCamera;
//     [SerializeField] private CameraController cameraController;
//     [SerializeField] private Bounds zoneBounds;
//     [SerializeField] private int priority = 1;
    
//     [Header("Multi-Zone Settings")]
//     [SerializeField] private bool isDefaultZone = false;
//     [SerializeField] private string sceneName = ""; // Scene mà zone này thuộc về
    
//     public string ZoneId => zoneId;
//     public CinemachineCamera VirtualCamera => virtualCamera;
//     public CameraController CameraController => cameraController;
//     public Bounds ZoneBounds => zoneBounds;
//     public int Priority => priority;
//     public bool IsDefaultZone => isDefaultZone;
//     public string SceneName => sceneName;

//     private void OnEnable()
//     {
//         // Auto-detect scene name if not set
//         if (string.IsNullOrEmpty(sceneName))
//         {
//             sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//         }
        
//         // Auto-register với CameraSystemManager
//         if (CameraSystemManager.Instance != null)
//         {
//             CameraSystemManager.Instance.RegisterCameraZone(this);
//         }
//     }

//     private void OnDisable()
//     {
//         // Auto-unregister
//         if (CameraSystemManager.Instance != null)
//         {
//             CameraSystemManager.Instance.UnregisterCameraZone(zoneId);
//         }
//     }

//     private void OnDrawGizmosSelected()
//     {
//         // Draw zone bounds in editor
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireCube(zoneBounds.center, zoneBounds.size);
        
//         // Draw camera position
//         if (virtualCamera != null)
//         {
//             Gizmos.color = Color.blue;
//             Gizmos.DrawWireSphere(virtualCamera.transform.position, 0.5f);
//         }
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         // Auto-switch khi player enter zone
//         if (other.CompareTag("Player"))
//         {
//             if (CameraSystemManager.Instance != null)
//             {
//                 CameraSystemManager.Instance.SwitchToZone(zoneId);
//             }
//         }
//     }

//     private void OnTriggerStay2D(Collider2D other)
//     {
//         // Ensure correct camera when player stays in zone
//         if (other.CompareTag("Player"))
//         {
//             if (CameraSystemManager.Instance != null && 
//                 CameraSystemManager.Instance.GetCurrentZoneId() != zoneId)
//             {
//                 CameraSystemManager.Instance.SwitchToZone(zoneId);
//             }
//         }
//     }

//     /// <summary>
//     /// Setup zone bounds automatically from camera settings
//     /// </summary>
//     [ContextMenu("Auto Setup Bounds")]
//     public void AutoSetupBounds()
//     {
//         if (virtualCamera != null)
//         {
//             // Lấy viewport size từ Cinemachine settings
//             var composer = virtualCamera.GetComponent<CinemachinePositionComposer>();
//             if (composer != null)
//             {
//                 // Estimate bounds based on camera settings
//                 float orthoSize = virtualCamera.Lens.OrthographicSize;
//                 float aspectRatio = (float)Screen.width / Screen.height;
                
//                 zoneBounds.center = virtualCamera.transform.position;
//                 zoneBounds.size = new Vector3(orthoSize * 2 * aspectRatio, orthoSize * 2, 1);
//             }
//         }
//     }

//     /// <summary>
//     /// Auto-find camera components if not assigned
//     /// </summary>
//     [ContextMenu("Auto Find Components")]
//     public void AutoFindComponents()
//     {
//         if (virtualCamera == null)
//         {
//             virtualCamera = GetComponentInChildren<CinemachineCamera>();
//         }
        
//         if (cameraController == null)
//         {
//             cameraController = GetComponentInChildren<CameraController>();
//         }
        
//         if (string.IsNullOrEmpty(zoneId))
//         {
//             zoneId = gameObject.name;
//         }
//     }
// }
