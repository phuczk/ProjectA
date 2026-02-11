using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class WorldSaveData : ISerializationCallbackReceiver
{
    public string currentSceneName = "";
    public string currentBench = "";
    public List<string> defeatedBossIds = new();
    public List<string> openedDoors = new();
    public List<Enemy> enemies = new();
    public Language language = Language.Vietnamese;

    [System.NonSerialized] public HashSet<string> visitedRooms = new();
    
    [SerializeField] 
    [UnityEngine.Serialization.FormerlySerializedAs("visitedRooms")]
    private List<string> _visitedRoomsList = new();

    public List<AreaType> unlockedMaps = new();

    public void OnBeforeSerialize()
    {
        _visitedRoomsList.Clear();
        _visitedRoomsList.AddRange(visitedRooms);
    }

    public void OnAfterDeserialize()
    {
        visitedRooms.Clear();
        foreach (var room in _visitedRoomsList)
        {
            visitedRooms.Add(room);
        }
    }
}

[System.Serializable]
public class Enemy
{
    public string id = "";
    public int numDeaths = 0;
}
