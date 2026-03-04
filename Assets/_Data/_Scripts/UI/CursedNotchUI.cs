using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class CursedNotchUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image emptyVisual;
    
    private CursedObjectData _currentData;
    [SerializeField] private Button _button;
    [SerializeField] private float scale = 1.5f;
    private Vector3 _defaultScale;

    public static event Action<string> OnNotchClicked;

    private void Awake()
    {
        _button?.onClick.AddListener(HandleNotchClick);
        _defaultScale = icon.transform.localScale;
    }

    public void SetItem(CursedObjectData data)
    {
        _currentData = data;
        if (data != null)
        {
            icon.sprite = data.icon;
            icon.gameObject.SetActive(true);
            if (emptyVisual != null) emptyVisual.gameObject.SetActive(false);
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        _currentData = null;
        icon.gameObject.SetActive(false);
        if (emptyVisual != null) emptyVisual.gameObject.SetActive(true);
    }

    private void HandleNotchClick()
    {
        if (_currentData != null && !string.IsNullOrEmpty(_currentData.id))
        {
            string itemId = _currentData.id;
            
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                player.EquipCursedObject(itemId);
                OnNotchClicked?.Invoke(itemId);
            }
        }
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        icon.transform.localScale = _defaultScale * scale;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        icon.transform.localScale = _defaultScale;
    }
}
