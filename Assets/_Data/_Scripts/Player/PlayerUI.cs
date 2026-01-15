using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject mapUI;

    public void ShowInventoryUI(bool show)
    {
        inventoryUI.SetActive(show);
    }

    public void ShowMapUI(bool show)
    {
        mapUI.SetActive(show);
    }
}