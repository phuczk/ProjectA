using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GlobalEnums;

public class InteractableUI : Singleton<InteractableUI>
{
    [Header("UI Components")]
    public GameObject panel;
    public Image itemImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    
    [Header("Animation")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    
    private Canvas _canvas;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void ShowInteractableInfo(InteractableType type, Sprite icon, string name, string description = "")
    {
        // Set content
        if (itemImage != null)
        {
            itemImage.sprite = icon;
            itemImage.gameObject.SetActive(icon != null);
        }
        
        if (titleText != null)
        {
            titleText.text = name;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
        }
        
        // Show panel with fade in
        if (panel != null)
        {
            panel.SetActive(true);
            
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeIn());
        }
    }
    
    public void HideInteractableInfo()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeOut());
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        if (panel != null)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
            
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
    }
    
    private System.Collections.IEnumerator FadeOut()
    {
        if (panel != null)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
            
            float elapsed = 0f;
            canvasGroup.alpha = 1f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            panel.SetActive(false);
        }
    }
}

public enum InteractableType
{
    Bench,
    Ability,
    Gun,
    CursedItem
}
