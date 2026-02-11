using UnityEngine;

public enum ShopItemType {OneTime, Permanent}

[CreateAssetMenu(menuName ="Shop/Item")]
public class ShopItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public int price;
    public ShopItemType type;
}
