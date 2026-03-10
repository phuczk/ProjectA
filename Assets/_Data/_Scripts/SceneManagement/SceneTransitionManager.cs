using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform _transitionPanel;
    [SerializeField] private RectTransform _deathTransition;
    [SerializeField] private RectTransform _horizontalFadePanel;

    [Header("Settings")]
    [SerializeField] private float _transitionTime = 0.5f;
    [SerializeField] private Ease _easeType = Ease.InOutQuad;

    private Vector2 _panelStartPos;
    private Vector2 _panelCenterPos;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupPositions();
    }

    private void SetupPositions()
    {
        float screenWidth = GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.width;
        float screenHeight = GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.height;
        
        _panelCenterPos = Vector2.zero;
        
        _transitionPanel.anchoredPosition = _panelCenterPos;
        _transitionPanel.gameObject.SetActive(false);
        
        if (_horizontalFadePanel != null)
        {
            _horizontalFadePanel.gameObject.SetActive(false);
        }
    }
    
    private Vector2 GetStartPosition(FadeDirection direction)
    {
        float screenWidth = GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.width;
        float screenHeight = GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.height;
        
        switch (direction)
        {
            case FadeDirection.Up:
                return new Vector2(0, screenHeight / 2);
            case FadeDirection.Down:
                return new Vector2(0, -screenHeight / 2);
            case FadeDirection.Left:
                return new Vector2(-screenWidth / 2, 0);
            case FadeDirection.Right:
                return new Vector2(screenWidth / 2, 0);
            default:
                return Vector2.zero;
        }
    }

    public void TransitionToScene(string sceneName, TransitionType transitionType, FadeDirection direction, bool isChangeScene = true)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, transitionType, direction, isChangeScene));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, TransitionType transitionType, FadeDirection direction, bool isChangeScene = true)
    {
        if (transitionType == TransitionType.Death)
        {
            yield return StartCoroutine(DeathTransitionRoutine(sceneName, isChangeScene));
        }
        else
        {
            yield return StartCoroutine(MoveTransitionRoutine(sceneName, direction));
        }
    }
    
    public void HorizontalFadeTransition(string sceneName, FadeDirection direction)
    {
        StartCoroutine(HorizontalFadeRoutine(sceneName, direction));
    }
    
    private IEnumerator HorizontalFadeRoutine(string sceneName, FadeDirection direction)
    {
        _horizontalFadePanel.gameObject.SetActive(true);
        
        CanvasGroup canvasGroup = _horizontalFadePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = _horizontalFadePanel.gameObject.AddComponent<CanvasGroup>();
        }
        
        if (direction == FadeDirection.Left)
        {
            canvasGroup.alpha = 0f;
            yield return canvasGroup.DOFade(1f, _transitionTime).SetEase(_easeType).WaitForCompletion();
            
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
            {
                yield return null;
            }
            
            yield return canvasGroup.DOFade(0f, _transitionTime).SetEase(_easeType).WaitForCompletion();
        }
        else
        {
            canvasGroup.alpha = 0f;
            yield return canvasGroup.DOFade(1f, _transitionTime).SetEase(_easeType).WaitForCompletion();
            
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
            {
                yield return null;
            }
            
            yield return canvasGroup.DOFade(0f, _transitionTime).SetEase(_easeType).WaitForCompletion();
        }
        
        _horizontalFadePanel.gameObject.SetActive(false);
    }
    
    private IEnumerator DeathTransitionRoutine(string sceneName, bool isChangeScene)
    {
        _deathTransition.gameObject.SetActive(true);
        
        CanvasGroup canvasGroup = _deathTransition.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = _deathTransition.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
        
        yield return canvasGroup.DOFade(1f, _transitionTime).SetEase(_easeType).WaitForCompletion();

        if (isChangeScene)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
            {
                yield return null;
            }
        }

        yield return canvasGroup.DOFade(0f, _transitionTime).SetEase(_easeType).WaitForCompletion();
        _deathTransition.gameObject.SetActive(false);
    }
    
    private IEnumerator MoveTransitionRoutine(string sceneName, FadeDirection direction)
{
    _transitionPanel.gameObject.SetActive(true);
    _panelStartPos = GetStartPosition(direction);
    _transitionPanel.anchoredPosition = _panelStartPos;

    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
    operation.allowSceneActivation = false;

    yield return _transitionPanel.DOAnchorPos(_panelCenterPos, _transitionTime)
        .SetEase(_easeType)
        .SetUpdate(true) 
        .WaitForCompletion();

    while (operation.progress < 0.9f)
    {
        yield return null;
    }

    operation.allowSceneActivation = true;

    while (!operation.isDone)
    {
        yield return null;
    }

    Vector2 exitPos = GetOppositePosition(direction);
    yield return _transitionPanel.DOAnchorPos(exitPos, _transitionTime)
        .SetEase(_easeType)
        .SetUpdate(true)
        .WaitForCompletion();
    
    _transitionPanel.gameObject.SetActive(false);
}
    
    private Vector2 GetOppositePosition(FadeDirection direction)
    {
        float screenWidth = GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.width;
        float screenHeight = GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.height;
        
        switch (direction)
        {
            case FadeDirection.Up:
                return new Vector2(0, -screenHeight / 2);
            case FadeDirection.Down:
                return new Vector2(0, screenHeight / 2);
            case FadeDirection.Left:
                return new Vector2(screenWidth / 2, 0);
            case FadeDirection.Right:
                return new Vector2(-screenWidth / 2, 0);
            default:
                return Vector2.zero;
        }
    }
}

public enum TransitionType
{
    Death,
    Move
}
