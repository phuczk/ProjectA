using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using GlobalEnums;

public class SaveListUI : MonoBehaviour
{
    [SerializeField] private SaveManager _manager;
    [SerializeField] private Transform _listRoot;
    [SerializeField] private SaveSlotItemUI _itemPrefab;
    private readonly List<SaveSlotItemUI> _items = new();
    [SerializeField] private GameStateChannel _stateChannel;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (_manager == null) _manager = Object.FindFirstObjectByType<SaveManager>();
        foreach (var it in _items)
        {
            if (it == null) continue;
            it.transform.DOKill();
            Object.Destroy(it.gameObject);
        }
        _items.Clear();
        var slots = _manager != null ? _manager.GetSlots() : new List<SaveSystemz.SaveSlotInfo>();
        foreach (var info in slots)
        {
            var item = Object.Instantiate(_itemPrefab, _listRoot);
            item.Setup(info, this);
            _items.Add(item);
        }
    }

    public void OnSlotLeftClick(int slot)
    {
        _manager ??= Object.FindFirstObjectByType<SaveManager>();
        if (_manager == null) return;
        Debug.Log($"Load slot {slot}");
        _manager.LoadSlot(slot);
        _stateChannel.RaiseRequest(GameState.Playing);
        Refresh();
    }

    public void OnSlotRightClick(int slot)
    {
        SaveSystemz.DeleteSlot(slot);
        Refresh();
    }
    
    private void OnDisable()
    {
        foreach (var it in _items)
        {
            if (it == null) continue;
            it.transform.DOKill();
        }
        transform.DOKill();
    }
}
