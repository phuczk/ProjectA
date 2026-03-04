using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;

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

    public CursedList CursedList => cursedList;

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
            
            if (i == 0)
            {
                notch.name = "SkillNotch_0";
                notch.gameObject.transform.localScale = Vector3.one * 1.5f;
            }
            
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
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        if (save?.player == null || cursedList == null) return;

        int currentNotchCount = save.player.currentNotch; 
        var equippedItemIds = new List<string>(save.player.currentCursedObjects);

        var skillItem = equippedItemIds.FirstOrDefault(id => 
        {
            var item = cursedList.GetById(id);
            return item != null && item.type == CursedObjectType.Skill;
        });

        var normalItems = equippedItemIds.Where(id => 
        {
            var item = cursedList.GetById(id);
            return item != null && item.type != CursedObjectType.Skill;
        }).ToList();

        for (int i = 0; i < notchSlots.Count; i++)
        {
            bool isSlotUnlocked = i < currentNotchCount;
            notchSlots[i].gameObject.SetActive(isSlotUnlocked);

            if (isSlotUnlocked)
            {
                if (i == 0)
                {
                    if (skillItem != null)
                    {
                        var itemData = cursedList.GetById(skillItem);
                        if (itemData != null)
                        {
                            notchSlots[i].SetItem(itemData);
                        }
                        else
                        {
                            notchSlots[i].Clear();
                        }
                    }
                    else
                    {
                        notchSlots[i].Clear();
                    }
                }
                else
                {
                    int normalIndex = i - 1;
                    if (normalIndex < normalItems.Count)
                    {
                        var itemData = cursedList.GetById(normalItems[normalIndex]);
                        if (itemData != null)
                        {
                            notchSlots[i].SetItem(itemData);
                        }
                        else
                        {
                            notchSlots[i].Clear();
                        }
                    }
                    else
                    {
                        notchSlots[i].Clear();
                    }
                }
            }
        }
    }

    public bool CanEquipItem(string cursedId, int notchIndex)
    {
        var itemData = cursedList.GetById(cursedId);
        if (itemData == null) return false;

        if (notchIndex == 0)
        {
            return itemData.type == CursedObjectType.Skill;
        }

        if (itemData.type == CursedObjectType.Skill)
        {
            return false;
        }

        return true;
    }

    public bool TryEquipItemToNotch(string cursedId, int notchIndex)
    {
        if (!CanEquipItem(cursedId, notchIndex)) return false;

        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        if (save?.player == null) return false;

        var itemData = cursedList.GetById(cursedId);
        if (itemData == null) return false;

        if (notchIndex == 0)
        {
            var oldSkillIndex = save.player.currentCursedObjects.FindIndex(id => 
            {
                var oldItem = cursedList.GetById(id);
                return oldItem != null && oldItem.type == CursedObjectType.Skill;
            });

            if (oldSkillIndex >= 0)
            {
                save.player.currentCursedObjects[oldSkillIndex] = cursedId;
            }
            else
            {
                save.player.currentCursedObjects.Insert(0, cursedId);
            }
        }
        else
        {
            int normalIndex = notchIndex - 1;
            
            var normalItems = save.player.currentCursedObjects.Where(id => 
            {
                var item = cursedList.GetById(id);
                return item != null && item.type != CursedObjectType.Skill;
            }).ToList();
            
            if (normalIndex < normalItems.Count)
            {
                var oldNormalId = normalItems[normalIndex];
                int oldIndex = save.player.currentCursedObjects.IndexOf(oldNormalId);
                save.player.currentCursedObjects[oldIndex] = cursedId;
            }
            else
            {
                save.player.currentCursedObjects.Add(cursedId);
            }
        }

        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        else SaveSystem.Save(save);

        RefreshNotchDisplay();
        return true;
    }

    public void SetNotchCount(int notchCount)
    {
        notchCount = Mathf.Clamp(notchCount, 0, maxNotches);
        var save = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : SaveSystem.Load();
        
        if (save.player == null) save.player = new PlayerData();
        save.player.currentNotch = notchCount;

        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        else SaveSystem.Save(save);

        RefreshNotchDisplay();
    }
}
