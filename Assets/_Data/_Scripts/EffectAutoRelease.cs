using UnityEngine;

public class EffectAutoRelease : MonoBehaviour
{
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        float duration = _ps.main.duration + _ps.main.startLifetime.constantMax;
        Invoke(nameof(ReturnToPool), duration);
    }

    private void ReturnToPool()
    {
        if (BulletPool.Instance != null)
            BulletPool.Instance.Release(gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
