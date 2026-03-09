using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Button))]
public class CursedItemUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image icon;

    [SerializeField] private Color equippedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    private CursedObjectData data;
    private Button _button;
    private RectTransform _rect;

    public Action<CursedObjectData, RectTransform> OnItemSelected;
    public Action<CursedObjectData> OnItemClicked;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rect = GetComponent<RectTransform>();

        _button.onClick.AddListener(HandleClick);
    }

    public void Bind(CursedObjectData data, bool isEquipped)
    {
        this.data = data;
        icon.sprite = data.icon;
        
        icon.color = isEquipped ? equippedColor : Color.white;
        icon.transform.localScale = Vector3.one;
    }
    
    public string ItemId => data != null ? data.id : string.Empty;

    public void OnSelect(BaseEventData eventData)
    {
        icon.transform.localScale = Vector3.one * 1.1f;
        OnItemSelected?.Invoke(data, _rect);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        icon.transform.localScale = Vector3.one;
    }

    private void HandleClick()
    {
        if (PlayerRestState.Instance != null && !PlayerRestState.Instance.IsResting)
        {
            ShowRestRequiredPanel();
            return;
        } 
        
        OnItemClicked?.Invoke(data);
    }

    private void ShowRestRequiredPanel()
    {
        var restPanel = FindAnyObjectByType<RestRequiredPanel>();
        if (restPanel != null)
        {
            restPanel.ShowPanel();
        }
    }
}
