using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RestRequiredPanel : MonoBehaviour, IBackHandler
{
    [Header("UI Components")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private float _displayDuration = 3f;
    
    [Header("Settings")]
    [SerializeField] private string _restRequiredMessage = Localization.Get("ui.rest_required");
    
    private void Awake()
    {
        if (_panel == null)
            _panel = gameObject;
            
        HidePanel();
    }
    
    private void Start()
    {
        PlayerRestState.OnRestStateChanged += OnRestStateChanged;
    }
    
    private void OnDestroy()
    {
        PlayerRestState.OnRestStateChanged -= OnRestStateChanged;
    }
    
    private void OnRestStateChanged(bool isResting)
    {
        // Panel tự động ẩn khi player bắt đầu rest
        if (isResting)
        {
            HidePanel();
        }
    }
    
    public void ShowPanel()
    {
        if (_panel != null)
        {
            _panel.SetActive(true);
            
            if (_messageText != null)
            {
                _messageText.text = _restRequiredMessage;
            }
            
            // Tự động ẩn sau một khoảng thời gian
            StartCoroutine(HideAfterDelay());
        }
    }
    
    public void HidePanel()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
        
        StopAllCoroutines();
    }
    
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(_displayDuration);
        HidePanel();
    }
    
    public void SetMessage(string message)
    {
        _restRequiredMessage = message;
    }
    
    public bool OnBack()
    {
        // Only handle back if panel is currently visible
        if (_panel != null && _panel.activeInHierarchy)
        {
            HidePanel();
            return true;
        }
        return false;
    }
}
