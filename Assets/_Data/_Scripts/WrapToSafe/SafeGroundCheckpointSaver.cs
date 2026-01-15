using UnityEngine;
using System.Collections;

public class SafeGroundCheckpointSaver : MonoBehaviour
{
    [SerializeField] private LayerMask checkpointLayer;
    public Vector2 SafeGroundLocation { get; private set; } = Vector2.zero;

    private void Start()
    {
        SafeGroundLocation = transform.position;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((checkpointLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            SafeGroundLocation = new Vector2(other.bounds.center.x, other.bounds.min.y);
        }
    }

    public void WrapToSafeGround()
    {
        transform.position = SafeGroundLocation;
    }
}
