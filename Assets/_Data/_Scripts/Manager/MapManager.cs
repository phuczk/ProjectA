using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using GlobalEnums;

public class MapManager : Singleton<MapManager>
{
    private List<string> _pendingRooms = new List<string>();
    
    [Header("Data Storage")]
    public List<MapRoomData> allRooms = new List<MapRoomData>();

    private Dictionary<string, MapRoomData> _roomLookup = new Dictionary<string, MapRoomData>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeLookup();
    }

    private void InitializeLookup()
    {
        _roomLookup.Clear();
        foreach (var room in allRooms)
        {
            if (room != null && !string.IsNullOrEmpty(room.roomName))
            {
                if (!_roomLookup.ContainsKey(room.roomName))
                    _roomLookup.Add(room.roomName, room);
            }
        }
    }

    // Thay thế hàm Find (O(n)) bằng Dictionary Lookup (O(1))
    public MapRoomData GetRoomData(string roomName)
    {
        if (_roomLookup.TryGetValue(roomName, out MapRoomData data))
        {
            return data;
        }
        return null;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string roomName = scene.name;
        if (IsRoomVisited(roomName)) return;

        if (!_pendingRooms.Contains(roomName))
            _pendingRooms.Add(roomName);
    }

    public void OnSitAtBench()
    {
        var saveData = SaveManager.Instance?.CurrentData;
        if (saveData == null || saveData.world == null) return;

        bool changed = false;
        foreach (var room in _pendingRooms)
        {
            if (!saveData.world.visitedRooms.Contains(room))
            {
                saveData.world.visitedRooms.Add(room);
                changed = true;
            }
        }
        _pendingRooms.Clear();
        if (changed) Debug.Log("MapManager: Map updated at bench.");
    }

    public bool IsRoomVisited(string roomName)
    {
        var saveData = SaveManager.Instance?.CurrentData;
        if (saveData == null || saveData.world == null) return false;
        return saveData.world.visitedRooms.Contains(roomName);
    }
}
