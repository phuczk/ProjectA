using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Button))]
public class CursedItemUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image icon;

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

    public void Bind(CursedObjectData data)
    {
        this.data = data;
        icon.sprite = data.icon;
        icon.transform.localScale = Vector3.one;
    }

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
        OnItemClicked?.Invoke(data);
    }
}
