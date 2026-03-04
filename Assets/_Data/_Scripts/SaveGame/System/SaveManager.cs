using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SaveManager : Singleton<SaveManager>
{
    [SerializeField] private int _defaultSlot = 1;

    public SaveData CurrentData { get; private set; }

    public GameObject _playerPrefab;
    public static System.Action OnDataLoaded;

    protected override void Awake()
    {
        base.Awake();

        SaveSystem.SetActiveSlot(_defaultSlot);
        PlayerSpawnService.Init(_playerPrefab.GetComponent<PlayerController>());
    }

    public void LoadSlot(int slot)
    {
        if (slot <= 0) slot = _defaultSlot;
        CurrentData = SaveSystem.LoadFromSlot(slot);

        OnDataLoaded?.Invoke(); 

        SceneFlowService.LoadScene(CurrentData);
    }

    public List<SaveSystem.SaveSlotInfo> GetSlots()
    {
        return SaveSystem.ListSlots();
    }

    public void SaveGame()
    {
        SaveableRegistry.SaveAll(CurrentData);
        SaveSystem.SaveToSlot(_defaultSlot, CurrentData);
    }
}
