using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CursedItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descText;

    public bool isSelected = false;

    public void Bind(CursedObjectData data)
    {
        icon.sprite = data.icon;
        nameText.text = Localization.Get(data.NameKey);
        descText.text = Localization.Get(data.DescKey);
    }
}
