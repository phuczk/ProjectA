using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;
using GlobalEnums;

public enum MapViewState { World, Zone }

public class MapUI : MonoBehaviour
{
    [SerializeField] private MapViewState currentState = MapViewState.World;
    [SerializeField] private RectTransform worldMapRoot; // Chứa các khối đơn giản
    [SerializeField] private RectTransform zoneMapRoot;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private float worldScale = 1f;
    [SerializeField] private float zoneScale = 3f;

    [Header("UI References")]
    [SerializeField] private RectTransform mapContainer;
    [SerializeField] private GameObject roomTemplatePrefab; 
    [SerializeField] private RectTransform playerIcon;

    private Dictionary<AreaType, GameObject> _spawnedAreas = new Dictionary<AreaType, GameObject>();
    private Dictionary<string, GameObject> _spawnedRooms = new Dictionary<string, GameObject>();
    private Transform _playerTransform;

    private void Start() {
        SwitchToWorldView();
    }

    public void ToggleView(AreaData targetArea = null)
    {
        if (currentState == MapViewState.World)
            SwitchToZoneView(targetArea);
        else
            SwitchToWorldView();
    }

    private void SwitchToZoneView(AreaData area)
    {
        currentState = MapViewState.Zone;
        mapContainer.DOScale(zoneScale, zoomDuration).SetUpdate(true); // Thêm SetUpdate(true)
        RefreshMapVisibility(area);
    }

    private void SwitchToWorldView()
    {
        currentState = MapViewState.World;
        mapContainer.DOScale(worldScale, zoomDuration).SetUpdate(true); // Thêm SetUpdate(true)
        mapContainer.DOAnchorPos(Vector2.zero, zoomDuration).SetUpdate(true);
        RefreshMapVisibility(null);
    }

    private void RefreshMapDefault()
    {
        RefreshMapVisibility(null); 
    }

    private void OnEnable()
    {
        SaveManager.OnDataLoaded += RefreshMapDefault;
        
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
        {
            RefreshMapVisibility(null);
        }
        
        FindPlayer();
    }

    private void OnDisable()
    {
        SaveManager.OnDataLoaded -= RefreshMapDefault;
    }

    private void Update()
    {
        if (_playerTransform == null) FindPlayer();
        UpdatePlayerPosition();
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Manual Refresh Triggered");
            RefreshMapDefault();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleView();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    // public void RefreshMap()
    // {
    //     if (MapManager.Instance == null) return;
    //     if (mapContainer == null || roomTemplatePrefab == null) return;

    //     if (MapManager.Instance.allRooms != null)
    //     {
    //         foreach (var roomData in MapManager.Instance.allRooms)
    //         {
    //             if (roomData == null) continue;

    //             bool isVisited = MapManager.Instance.IsRoomVisited(roomData.roomName);
                
    //             if (_spawnedRooms.ContainsKey(roomData.roomName))
    //             {
    //                 _spawnedRooms[roomData.roomName].SetActive(isVisited);
    //             }
    //             else
    //             {
    //                 if (isVisited)
    //                 {
    //                     SpawnRoom(roomData);
    //                 }
    //             }
    //         }
    //     }
        
    //     if (playerIcon != null)
    //     {
    //         playerIcon.SetAsLastSibling();
    //     }
    // }

private void RefreshMapVisibility(AreaData activeArea)
{
    if (MapManager.Instance == null) return;

    bool isWorldView = (currentState == MapViewState.World);
    
    // --- 1. XỬ LÝ AREA LỚN (WORLD MAP) ---
    // Duyệt qua tất cả phòng để đảm bảo Area đã được Spawn
    foreach (var roomData in MapManager.Instance.allRooms)
    {
        if (roomData == null) continue;
        bool visited = MapManager.Instance.IsRoomVisited(roomData.roomName);

        if (visited && roomData.areaData != null)
        {
            // Nếu khu vực này đã từng có phòng được đi qua, hãy spawn khối Area
            if (!_spawnedAreas.ContainsKey(roomData.areaData.areaType))
            {
                SpawnArea(roomData.areaData);
            }
        }
    }

    // Bật/Tắt các khối Area dựa trên view
    foreach (var area in _spawnedAreas)
    {
        area.Value.SetActive(isWorldView);
    }

    // --- 2. XỬ LÝ ROOM CHI TIẾT ---
    foreach (var roomData in MapManager.Instance.allRooms)
    {
        if (roomData == null) continue;
        bool visited = MapManager.Instance.IsRoomVisited(roomData.roomName);
        
        // Spawn phòng nếu đã đi qua mà chưa có object
        if (visited && !_spawnedRooms.ContainsKey(roomData.roomName))
        {
            SpawnRoom(roomData);
        }

        if (_spawnedRooms.ContainsKey(roomData.roomName))
        {
            GameObject roomObj = _spawnedRooms[roomData.roomName];

            if (isWorldView)
            {
                // WORLD VIEW: Ẩn phòng chi tiết để hiện khối Area (Hollow Knight style)
                // Nếu bạn muốn hiện cả phòng trong World View, hãy đổi thành roomObj.SetActive(visited);
                roomObj.SetActive(false); 
            }
            else
            {
                // ZONE VIEW: Hiện phòng đã đi qua VÀ phải thuộc Area đang xem
                // Nếu activeArea null (lỗi truyền vào), ta lấy area của scene hiện tại làm mặc định
                if (activeArea == null) 
                {
                    string currentScene = SceneManager.GetActiveScene().name;
                    var currentData = MapManager.Instance.GetRoomData(currentScene);
                    activeArea = currentData?.areaData;
                }
                
                roomObj.SetActive(visited && roomData.areaData == activeArea);
            }
        }
    }
}

    private void SpawnArea(AreaData data)
    {
        if (data.areaSimpleSprite == null) return;

        // Dùng chung template hoặc tạo một cái riêng cho Area khối
        GameObject obj = Instantiate(roomTemplatePrefab, worldMapRoot); // worldMapRoot là cha
        obj.name = "Area_" + data.areaName;
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = data.worldMapPosition;

        Image img = obj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = data.areaSimpleSprite;
            img.color = data.areaColor;
            img.SetNativeSize();
        }

        _spawnedAreas.Add(data.areaType, obj);
    }

    private void SpawnRoom(MapRoomData data)
    {
        GameObject obj = Instantiate(roomTemplatePrefab, mapContainer);
        obj.name = data.roomName;
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = data.mapPosition;
        }

        Image img = obj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = data.roomSprite;
            img.SetNativeSize();
        }

        _spawnedRooms.Add(data.roomName, obj);
    }

    private void UpdatePlayerPosition()
    {
        if (playerIcon == null || MapManager.Instance == null || _playerTransform == null) return;

        string currentScene = SceneManager.GetActiveScene().name;
        MapRoomData roomData = MapManager.Instance.GetRoomData(currentScene);

        if (roomData == null || !_spawnedRooms.ContainsKey(currentScene))
        {
            playerIcon.gameObject.SetActive(false);
            return;
        }

        playerIcon.gameObject.SetActive(true);

        if (currentState == MapViewState.World)
        {
            // Icon bám theo Map Container hoặc WorldMapRoot
            playerIcon.SetParent(worldMapRoot);
            // Tính toán vị trí Player trên World Map dựa trên tỉ lệ world map
            playerIcon.anchoredPosition = roomData.areaData.worldMapPosition; 
        }
        else
        {
            // Icon bám theo Room chi tiết
            if (_spawnedRooms.ContainsKey(currentScene))
            {
                GameObject currentRoomObj = _spawnedRooms[currentScene];
                playerIcon.SetParent(currentRoomObj.transform);
                Vector2 relativeWorldPos = (Vector2)_playerTransform.position - roomData.worldCenter;
        
                float normX = (roomData.worldSize.x != 0) ? relativeWorldPos.x / roomData.worldSize.x : 0;
                float normY = (roomData.worldSize.y != 0) ? relativeWorldPos.y / roomData.worldSize.y : 0;

                RectTransform roomRT = currentRoomObj.GetComponent<RectTransform>();
                Vector2 uiSize = roomRT.sizeDelta;

                playerIcon.anchoredPosition = new Vector2(normX * uiSize.x, normY * uiSize.y);
            }
        }

        // GameObject currentRoomObj = _spawnedRooms[currentScene];
        // if (playerIcon.parent != currentRoomObj.transform)
        // {
        //     playerIcon.SetParent(currentRoomObj.transform);
        //     playerIcon.localScale = Vector3.one;
        // }

        // Vector2 relativeWorldPos = (Vector2)_playerTransform.position - roomData.worldCenter;
        
        // float normX = (roomData.worldSize.x != 0) ? relativeWorldPos.x / roomData.worldSize.x : 0;
        // float normY = (roomData.worldSize.y != 0) ? relativeWorldPos.y / roomData.worldSize.y : 0;

        // RectTransform roomRT = currentRoomObj.GetComponent<RectTransform>();
        // Vector2 uiSize = roomRT.sizeDelta;

        // playerIcon.anchoredPosition = new Vector2(normX * uiSize.x, normY * uiSize.y);
    }
}
