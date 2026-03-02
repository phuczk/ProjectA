using UnityEngine;
using UnityEngine.UI;
using System;

public class CursedNotchUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject emptyVisual;
    
    private CursedObjectData _currentData;
    [SerializeField] private Button _button;

    public static event Action<string> OnNotchClicked;

    private void Awake()
    {
        _button?.onClick.AddListener(HandleNotchClick);
    }

    public void SetItem(CursedObjectData data)
    {
        _currentData = data;
        if (data != null)
        {
            icon.sprite = data.icon;
            icon.gameObject.SetActive(true);
            if (emptyVisual != null) emptyVisual.SetActive(false);
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
        if (emptyVisual != null) emptyVisual.SetActive(true);
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
}
