using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(Button))]
public class CursedNotchUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject emptyVisual;
    
    private CursedObjectData _currentData;
    [SerializeField] private Button _button;

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
        if (_currentData != null)
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                player.EquipCursedObject(_currentData.id);
            }
        }
    }
}
