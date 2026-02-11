using UnityEngine;
using GlobalEnums;

[CreateAssetMenu(fileName = "NewMapRoom", menuName = "Map/Map Room Data")]
public class MapRoomData : ScriptableObject
{
    [Header("Basic Info")]
    public string roomName;
    public AreaType area;

    [Header("Map UI Settings")]
    public Vector2 mapPosition;
    public Sprite roomSprite;
    
    [Header("World Info")]
    public Vector2 worldSize = new Vector2(40, 20);
    public Vector2 worldCenter = Vector2.zero;

    public AreaData areaData;

    [Header("Connection")]
    public MapRoomData[] connectedRooms;
}
