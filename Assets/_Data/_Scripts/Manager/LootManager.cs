using GlobalEnums;
using UnityEngine;

public class LootManager : Singleton<LootManager>
{
    public System.Action<int> OnMoneyChanged;
    public System.Action OnMoneyLoaded;

    public int CurrentMoney = 0;

    protected override void Awake()
    {
        base.Awake();
        SaveManager.OnDataLoaded += LoadMoney;
    }

    private void OnDestroy()
    {
        SaveManager.OnDataLoaded -= LoadMoney;
    }

    private void LoadMoney()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
        {
            CurrentMoney = SaveManager.Instance.CurrentData.player.currentMoney;
            OnMoneyLoaded?.Invoke();
        }
    }

    public void AddMoney(int money)
    {
        CurrentMoney += money;
        OnMoneyChanged?.Invoke(money);
    }

    public int GetCurrentMoney()
    {
        return CurrentMoney;
    }

    public void UseMoney(int money)
    {
        CurrentMoney -= money;
        OnMoneyChanged?.Invoke(money);
    }

    public void ResetMoney()
    {
        CurrentMoney = 0;
    }
}