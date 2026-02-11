using UnityEngine;

public class LineProjectile : EnemyProjectile
{   
    private Vector2 _spawnPosition;

    protected override void OnEnable()
    {
        base.OnEnable();
        _spawnPosition = transform.position;
    }

    protected override void Update() { }

    private void RotateToMovement(Vector2 nextPos)
    {
        Vector2 moveDir = nextPos - (Vector2)transform.position;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
