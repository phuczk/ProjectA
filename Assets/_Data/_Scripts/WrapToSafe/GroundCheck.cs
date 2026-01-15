using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float extraHeight = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    private Collider2D coll;

    private RaycastHit2D groundHit;

    private void Start() {
        coll = GetComponent<Collider2D>();
    }

    public bool IsGrounded()
    {
        groundHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        if (groundHit.collider != null) {
            return true;
        }
        return false;
    }
}
