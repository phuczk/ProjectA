using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening; // Thêm namespace DOTween

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform _leftPanel;
    [SerializeField] private RectTransform _rightPanel;

    [Header("Settings")]
    [SerializeField] private float _transitionTime = 0.5f;
    [SerializeField] private Ease _easeType = Ease.InOutQuad;

    private Vector2 _leftPanelOpenPos;
    private Vector2 _rightPanelOpenPos;

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

        // Thiết lập vị trí ban đầu dựa trên Resolution
        SetupPositions();
    }

    private void SetupPositions()
    {
        float screenWidth = GetComponentInChildren<Canvas>().GetComponent<RectTransform>().rect.width;
        
        // Vị trí khi mở: Đẩy sang 2 bên ngoài màn hình
        _leftPanelOpenPos = new Vector2(-screenWidth / 2, 0);
        _rightPanelOpenPos = new Vector2(screenWidth / 2, 0);

        // Khởi tạo trạng thái ban đầu là đang mở
        _leftPanel.anchoredPosition = _leftPanelOpenPos;
        _rightPanel.anchoredPosition = _rightPanelOpenPos;
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 1. Đóng màn lại (Về Vector2.zero)
        Sequence closeSequence = DOTween.Sequence();
        closeSequence.Join(_leftPanel.DOAnchorPos(Vector2.zero, _transitionTime).SetEase(_easeType));
        closeSequence.Join(_rightPanel.DOAnchorPos(Vector2.zero, _transitionTime).SetEase(_easeType));

        yield return closeSequence.WaitForCompletion();

        // 2. Load Scene mới
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }

        // 3. Mở màn ra (Về vị trí OpenPos)
        Sequence openSequence = DOTween.Sequence();
        openSequence.Join(_leftPanel.DOAnchorPos(_leftPanelOpenPos, _transitionTime).SetEase(_easeType));
        openSequence.Join(_rightPanel.DOAnchorPos(_rightPanelOpenPos, _transitionTime).SetEase(_easeType));

        yield return openSequence.WaitForCompletion();
    }
}