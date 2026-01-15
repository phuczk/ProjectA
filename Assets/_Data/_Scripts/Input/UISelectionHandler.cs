using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Thêm để dùng Button

public class UISelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public enum NavigationTrigger { None, Up, Down, Left, Right }

    [Header("Quick Navigation Click")]
    [SerializeField] private NavigationTrigger _triggerDirection = NavigationTrigger.None;
    private Button _button;

    [Header("Animation Settings")]
    [SerializeField] private float _moveTime = 0.2f;
    [Range(0f, 2f), SerializeField] private float _scaleAmount = 1.1f;
    private Vector3 _startScale;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _startScale = transform.localScale;
    }

    public void CheckQuickClick(Vector2 input)
    {
        if (_triggerDirection == NavigationTrigger.None || _button == null || !_button.interactable) return;

        bool shouldClick = false;
        if (_triggerDirection == NavigationTrigger.Up && input.y > 0.5f) shouldClick = true;
        if (_triggerDirection == NavigationTrigger.Down && input.y < -0.5f) shouldClick = true;
        if (_triggerDirection == NavigationTrigger.Left && input.x < -0.5f) shouldClick = true;
        if (_triggerDirection == NavigationTrigger.Right && input.x > 0.5f) shouldClick = true;

        if (shouldClick)
        {
            _button.onClick.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => EventSystem.current.SetSelectedGameObject(gameObject);
    public void OnPointerExit(PointerEventData eventData) { }

    public void OnSelect(BaseEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(MoveUI(true));
        UISelectionManager.Instance.LastSelected = gameObject;
        UISelectionManager.Instance.UpdateCurrentIndex(gameObject);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(MoveUI(false));
    }

    private IEnumerator MoveUI(bool startingAnimation)
    {
        Vector3 endScale = startingAnimation ? _startScale * _scaleAmount : _startScale;
        float elapsedTime = 0f;
        Vector3 initialScale = transform.localScale;

        while (elapsedTime < _moveTime)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, endScale, elapsedTime / _moveTime);
            yield return null;
        }
        transform.localScale = endScale;
    }
}
