using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CursedList")]
public class CursedList : ScriptableObject
{
    public List<CursedObjectData> CursedObjects;
    public CursedObjectData GetById(string id) => CursedObjects.Find(x => x.id == id);
}