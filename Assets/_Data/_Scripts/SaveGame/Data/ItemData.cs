using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    [SerializeField]
    public List<string> unlockedCursedObjects = new();

    public List<string> purchasedItems = new();
}
