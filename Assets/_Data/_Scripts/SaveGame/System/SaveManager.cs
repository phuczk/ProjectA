using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private int _defaultSlot = 1;

    public SaveData CurrentData { get; private set; }

    public GameObject _playerPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveSystemz.SetActiveSlot(_defaultSlot);
        PlayerSpawnService.Init(_playerPrefab.GetComponent<PlayerController>());
    }

    public void LoadSlot(int slot)
    {
        CurrentData = SaveSystemz.LoadFromSlot(slot);
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
