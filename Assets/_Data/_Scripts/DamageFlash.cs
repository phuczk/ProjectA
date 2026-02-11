using UnityEngine;
using DG.Tweening;

public class DamageFlash : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private Material _material;
    
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    
    private Tween _currentTween;

    private void Awake()
    {
        spriteRenderer ??= GetComponent<SpriteRenderer>();
        _material = spriteRenderer.material;
        
        _material.SetColor("_FlashColor", flashColor);
    }

    public void CallDamageFlash()
    {
        _currentTween?.Kill(complete: false);
        
        _material.SetFloat("_FlashAmount", 0f);

        _currentTween = DOTween.To(() => _material.GetFloat("_FlashAmount"),x => _material.SetFloat("_FlashAmount", x),1f,flashDuration)
        .SetEase(Ease.OutQuad)                                  
        .SetLoops(2, LoopType.Yoyo)                             
        .OnKill(() => _material.SetFloat("_FlashAmount", 0f));
    }

    private void OnDestroy()
    {
        _currentTween?.Kill();
    }
}
