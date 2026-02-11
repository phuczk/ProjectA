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

        SaveSystemz.SetActiveSlot(_defaultSlot);
        PlayerSpawnService.Init(_playerPrefab.GetComponent<PlayerController>());
    }

    public void LoadSlot(int slot)
    {
        if (slot <= 0) slot = _defaultSlot;
        CurrentData = SaveSystemz.LoadFromSlot(slot);

        OnDataLoaded?.Invoke(); 

        SceneFlowService.LoadScene(CurrentData);
    }

    public List<SaveSystemz.SaveSlotInfo> GetSlots()
    {
        return SaveSystemz.ListSlots();
    }

    public void SaveGame()
    {
        SaveableRegistry.SaveAll(CurrentData);
        SaveSystemz.SaveToSlot(_defaultSlot, CurrentData);
    }
}
