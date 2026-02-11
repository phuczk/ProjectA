using GlobalEnums;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArea", menuName = "Map/Area Data")]
public class AreaData : ScriptableObject
{
    public AreaType areaType;
    public string areaName;
    public Vector2 worldMapPosition;
    public Sprite areaSimpleSprite;
    public Color areaColor = Color.white;
}
