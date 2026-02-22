using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CursedNotchManager : Singleton<CursedNotchManager>
{
    [Header("Notch Configuration")]
    [SerializeField] private CursedNotchUI notchPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private CursedList cursedList;
    
    [Header("Notch Settings")]
    [SerializeField] private int maxNotches = 4;
    
    private List<CursedNotchUI> notchSlots = new List<CursedNotchUI>();
    private PlayerController _playerController;

    protected override void Awake()
    {
        base.Awake();
        _playerController = FindFirstObjectByType<PlayerController>();
        
        GenerateNotchSlots();
        
        RefreshNotchDisplay();
    }

    private void GenerateNotchSlots()
    {
        if (container == null) container = this.transform;
        
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        
        notchSlots.Clear();

        for (int i = 0; i < maxNotches; i++)
        {
            var notch = Instantiate(notchPrefab, container);
            notch.name = $"Notch_{i}";
            
            notch.gameObject.SetActive(true); 
            notch.Clear();
            notchSlots.Add(notch);
        }
    }

    private void Start()
    {
        RefreshNotchDisplay();
    }

    public void RefreshNotchDisplay()
    {
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystemz.Load();
        if (save?.player == null || cursedList == null) return;

        int currentNotchCount = save.player.currentNotch; 
        var equippedItemIds = save.player.currentCursedObjects; 

        for (int i = 0; i < notchSlots.Count; i++)
        {
            bool isSlotUnlocked = i < currentNotchCount;
            notchSlots[i].gameObject.SetActive(isSlotUnlocked);

            if (isSlotUnlocked)
            {
                if (i < equippedItemIds.Count)
                {
                    var itemData = cursedList.GetById(equippedItemIds[i]);
                    if (itemData != null) notchSlots[i].SetItem(itemData);
                }
                else
                {
                    notchSlots[i].Clear(); 
                }
            }
        }
    }

    public void SetNotchCount(int notchCount)
    {
        notchCount = Mathf.Clamp(notchCount, 0, maxNotches);
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystemz.Load();
        
        if (save.player == null) save.player = new PlayerData();
        save.player.currentNotch = notchCount;

        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        else SaveSystemz.Save(save);

        RefreshNotchDisplay();
    }
}
