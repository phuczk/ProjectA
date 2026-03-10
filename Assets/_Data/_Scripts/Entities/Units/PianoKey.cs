using UnityEngine;
using System.Collections;

public class PianoKey : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color _originalColor;
    private Color _activeColor;
    private float _duration;
    private Piano _piano;
    private Coroutine _fadeRoutine;

    public void Initialize(int index, Color original, Color active, float duration, Piano master)
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalColor = original;
        _activeColor = active;
        _duration = duration;
        _piano = master;
        _renderer.color = _originalColor;
    }

    public void SpawnBullet()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FlashEffect());
        
        _piano.SpawnBulletFromKey(transform.position);
    }

    private IEnumerator FlashEffect()
    {
        _renderer.color = _activeColor;
        float elapsed = 0;
        while (elapsed < _duration)
        {
            _renderer.color = Color.Lerp(_activeColor, _originalColor, elapsed / _duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _renderer.color = _originalColor;
    }
}