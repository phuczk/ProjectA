using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DamageFlash : MonoBehaviour
{
    [Header("Sprite Renderers")]
    public SpriteRenderer[] spriteRenderers;
    
    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;
    
    private List<Material> _materials = new List<Material>();
    private List<Tween> _currentTweens = new List<Tween>();

    private void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
        
        foreach (var renderer in spriteRenderers)
        {
            if (renderer != null)
            {
                _materials.Add(renderer.material);
                renderer.material.SetColor("_FlashColor", flashColor);
            }
        }
    }

    public void CallDamageFlash()
    {
        foreach (var tween in _currentTweens)
        {
            tween?.Kill(complete: false);
        }
        _currentTweens.Clear();
        
        foreach (var material in _materials)
        {
            if (material != null)
            {
                material.SetFloat("_FlashAmount", 0f);
                
                var tween = DOTween.To(() => material.GetFloat("_FlashAmount"), 
                    x => material.SetFloat("_FlashAmount", x), 1f, flashDuration)
                    .SetEase(Ease.OutQuad)                                  
                    .SetLoops(2, LoopType.Yoyo)                             
                    .OnKill(() => material.SetFloat("_FlashAmount", 0f));
                    
                _currentTweens.Add(tween);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var tween in _currentTweens)
        {
            tween?.Kill();
        }
        _currentTweens.Clear();
    }
}
