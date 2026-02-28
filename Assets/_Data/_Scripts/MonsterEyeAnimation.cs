using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MonsterEyeAnimation : MonoBehaviour
{
    [Header("Settings")]
    public string clipPropertyName = "_Clip";

    public bool affectAllChildren = false;

    public bool includeInactive = false;

    [Header("List Object & Target Clip Value riêng biệt")]
    public ObjectClipPair[] specificObjectClips;
    
    [Header("Animation Settings")]
    public float animationDuration = 1.0f;
    public Ease animationEase = Ease.OutQuad;

    [System.Serializable]
    public class ObjectClipPair
    {
        public GameObject targetObject;
        public float targetClipValue;
        public float defaultClipValue;
        
        public float delay = 0f;
        public float duration = 1f;
        public Ease ease = Ease.OutQuad;
    }

    public GameObject Pupil;

    void Start()
    {
        ResetAllMaterialsToDefault();
        ApplyClipToAll();
    }
    
    public void ResetAllMaterialsToDefault()
    {
        if (specificObjectClips != null && specificObjectClips.Length > 0)
        {
            foreach (var pair in specificObjectClips)
            {
                if (pair.targetObject == null) continue;
                
                Renderer rend = pair.targetObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    SetClipOnMaterial(rend.material, pair.defaultClipValue);
                    foreach (Material mat in rend.materials)
                    {
                        SetClipOnMaterial(mat, pair.defaultClipValue);
                    }
                }
            }
        }
        else if (affectAllChildren)
        {
            float defaultClipValue = 0.5f;
            if (specificObjectClips != null && specificObjectClips.Length > 0)
            {
                defaultClipValue = specificObjectClips[0].defaultClipValue;
            }
            
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(includeInactive);
            foreach (Renderer rend in allRenderers)
            {
                if (rend.material != null)
                {
                    SetClipOnMaterial(rend.material, defaultClipValue);
                }
                foreach (Material mat in rend.materials)
                {
                    SetClipOnMaterial(mat, defaultClipValue);
                }
            }
        }
    }

    public void ApplyClipToAll()
    {
        int count = 0;

        if (specificObjectClips != null && specificObjectClips.Length > 0)
        {
            foreach (var pair in specificObjectClips)
            {
                if (pair.targetObject == null) continue;

                Renderer rend = pair.targetObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    AnimateMaterialWithDelay(rend.material, pair.targetClipValue, pair.delay, pair.duration, pair.ease);
                    foreach (Material mat in rend.materials)
                    {
                        AnimateMaterialWithDelay(mat, pair.targetClipValue, pair.delay, pair.duration, pair.ease);
                    }
                    count++;
                }
            }
        }

        else if (affectAllChildren)
        {
            float defaultClipValue = 0.5f;
            
            if (specificObjectClips != null && specificObjectClips.Length > 0)
            {
                defaultClipValue = specificObjectClips[0].defaultClipValue;
            }
            
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(includeInactive);
            foreach (Renderer rend in allRenderers)
            {
                if (rend.material != null)
                {
                    AnimateMaterial(rend.material, defaultClipValue);
                }
                foreach (Material mat in rend.materials)
                {
                    AnimateMaterial(mat, defaultClipValue);
                }
                count++;
            }
        }
    }

    private void SetClipOnMaterial(Material mat, float clipValue)
    {
        if (mat.HasProperty(clipPropertyName))
        {
            mat.SetFloat(clipPropertyName, clipValue);
        }
    }
    
    private void AnimateMaterialWithDelay(Material mat, float targetClipValue, float delay, float duration, Ease ease)
    {
        if (mat.HasProperty(clipPropertyName))
        {
            float currentValue = mat.GetFloat(clipPropertyName);
            
            mat.DOKill();
            
            mat.DOFloat(targetClipValue, clipPropertyName, duration)
                .SetDelay(delay)
                .SetEase(ease);
        }
    }
    
    private void AnimateMaterial(Material mat, float targetClipValue)
    {
        AnimateMaterialWithDelay(mat, targetClipValue, 0f, animationDuration, animationEase);
    }

    public void SetClipForObject(GameObject targetObj, float clipValue)
    {
        Renderer rend = targetObj.GetComponent<Renderer>();
        if (rend != null)
        {
            AnimateMaterial(rend.material, clipValue);
            foreach (Material mat in rend.materials)
            {
                AnimateMaterial(mat, clipValue);
            }
        }
    }
    
    public void AnimateClipForObject(GameObject targetObj, float clipValue, float delay = 0f, float duration = 1f, Ease ease = Ease.OutQuad)
    {
        Renderer rend = targetObj.GetComponent<Renderer>();
        if (rend != null)
        {
            AnimateMaterialWithDelay(rend.material, clipValue, delay, duration, ease);
            foreach (Material mat in rend.materials)
            {
                AnimateMaterialWithDelay(mat, clipValue, delay, duration, ease);
            }
        }
    }
    
    public void AnimateClipForObject(GameObject targetObj, float clipValue, ObjectClipPair settings)
    {
        Renderer rend = targetObj.GetComponent<Renderer>();
        if (rend != null)
        {
            AnimateMaterialWithDelay(rend.material, clipValue, settings.delay, settings.duration, settings.ease);
            foreach (Material mat in rend.materials)
            {
                AnimateMaterialWithDelay(mat, clipValue, settings.delay, settings.duration, settings.ease);
            }
        }
    }
    
    public void AnimateBackToDefault()
    {
        if (specificObjectClips == null || specificObjectClips.Length == 0) return;
        
        var sortedPairs = new List<ObjectClipPair>(specificObjectClips);
        sortedPairs.Sort((a, b) => b.delay.CompareTo(a.delay));
        
        foreach (var pair in sortedPairs)
        {
            if (pair.targetObject == null) continue;
            
            Renderer rend = pair.targetObject.GetComponent<Renderer>();
            if (rend != null)
            {
                AnimateMaterialWithDelay(rend.material, pair.defaultClipValue, pair.delay, pair.duration, pair.ease);
                foreach (Material mat in rend.materials)
                {
                    AnimateMaterialWithDelay(mat, pair.defaultClipValue, pair.delay, pair.duration, pair.ease);
                }
            }
        }
    }
    
    public void AnimateBackToDefault(float reverseDelayMultiplier = 0.5f)
    {
        if (specificObjectClips == null || specificObjectClips.Length == 0) return;
        
        var sortedPairs = new List<ObjectClipPair>(specificObjectClips);
        sortedPairs.Sort((a, b) => b.delay.CompareTo(a.delay));
        
        float maxDelay = 0f;
        if (sortedPairs.Count > 0)
        {
            maxDelay = sortedPairs[0].delay;
        }
        
        foreach (var pair in sortedPairs)
        {
            if (pair.targetObject == null) continue;
            
            Renderer rend = pair.targetObject.GetComponent<Renderer>();
            if (rend != null)
            {
                float reverseDelay = (maxDelay - pair.delay) * reverseDelayMultiplier;
                
                AnimateMaterialWithDelay(rend.material, pair.defaultClipValue, reverseDelay, pair.duration, pair.ease);
                foreach (Material mat in rend.materials)
                {
                    AnimateMaterialWithDelay(mat, pair.defaultClipValue, reverseDelay, pair.duration, pair.ease);
                }
            }
        }
    }
}
