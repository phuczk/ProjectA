using UnityEngine;

public class SinusoidalProjectile : EnemyProjectile
{
    [Header("Sin Settings")]
    [SerializeField] private float frequency = 8f;
    [SerializeField] private float magnitude = 0.6f;
    [SerializeField] private float phaseOffset;

    private Vector2 _forwardDir;
    private Vector2 _sideDir;
    private float _time;

    protected override void OnEnable()
    {
        base.OnEnable();

        _time = 0f;

        _forwardDir = direction.normalized;
        _sideDir = new Vector2(-_forwardDir.y, _forwardDir.x);

        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected override void Update()
    {
        if (!isLaunched) return;

        _time += Time.deltaTime;

        float sin = Mathf.Sin((_time * frequency) + phaseOffset);

        Vector2 velocity =
            (_forwardDir * speed) +
            (_sideDir * sin * magnitude);

        transform.position += (Vector3)(velocity * Time.deltaTime);

        RotateToVelocity(velocity);
    }

    private void RotateToVelocity(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    protected override void FixedUpdate() { }
    protected override void Move() { }
}
