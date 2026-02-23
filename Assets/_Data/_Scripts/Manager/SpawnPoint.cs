using UnityEngine;

/// <summary>
/// Component that marks a spawn point for player respawn
/// Place this in your scene at desired spawn locations
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Configuration")]
    [SerializeField] private string spawnPointId = "";
    [SerializeField] private bool isDefaultSpawn = false;
    [SerializeField] private bool isActive = true;
    
    public string SpawnPointId => spawnPointId;
    public bool IsDefaultSpawn => isDefaultSpawn;
    public bool IsActive => isActive;

    private void Awake()
    {
        // Auto-generate ID if not set
        if (string.IsNullOrEmpty(spawnPointId))
        {
            spawnPointId = $"Spawn_{gameObject.name}_{GetInstanceID()}";
        }
    }

    private void OnDrawGizmos()
    {
        // Draw spawn point indicator
        Gizmos.color = isDefaultSpawn ? Color.green : Color.blue;
        if (!isActive) Gizmos.color = Color.gray;
        
        // Draw spawn icon
        Vector3 pos = transform.position;
        Gizmos.DrawWireSphere(pos, 0.5f);
        
        // Draw upward arrow
        Gizmos.DrawLine(pos, pos + Vector3.up * 1f);
        Gizmos.DrawLine(pos + Vector3.up * 1f, pos + Vector3.up * 0.8f + Vector3.left * 0.2f);
        Gizmos.DrawLine(pos + Vector3.up * 1f, pos + Vector3.up * 0.8f + Vector3.right * 0.2f);
        
        // Draw label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(pos + Vector3.up * 1.5f, spawnPointId);
        #endif
    }

    /// <summary>
    /// Get the spawn position
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// Activate or deactivate this spawn point
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
    }

    /// <summary>
    /// Set as default spawn point
    /// </summary>
    public void SetAsDefault(bool isDefault)
    {
        isDefaultSpawn = isDefault;
    }
}
