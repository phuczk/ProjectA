using GlobalEnums;
using System.Collections.Generic;
using UnityEngine;
using SerializeReferenceEditor;

[CreateAssetMenu(menuName = "CursedObject")]
public class CursedObjectData : ScriptableObject
{
    [Header("Localization")]
    public string id;
    public string NameKey => $"cursed.{id}.name";
    public string DescKey => $"cursed.{id}.desc";
    public CursedObjectType type;
    public Sprite icon;

    [field: SerializeReference, SR]
    public List<Effect> Effects { get; private set; }
}
