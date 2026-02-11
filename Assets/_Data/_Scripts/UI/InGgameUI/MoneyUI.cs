using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MoneyUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup root; // parent chứa icon + text
    public Image moneyIcon;
    public TextMeshProUGUI txtCurrent;
    public TextMeshProUGUI txtChange;

    int displayCurrent;
    int pendingChange;

    Tween delayTween;
    Tween countTween;
    Tween hideTween;

    const float delayTime = 1f;

    void Start()
    {
        root.alpha = 0;
        root.gameObject.SetActive(false);
        txtChange.gameObject.SetActive(false);

        LootManager.Instance.OnMoneyChanged += OnMoneyChanged;
        LootManager.Instance.OnMoneyLoaded += InitMoney;
    }

    void InitMoney()
    {
        displayCurrent = LootManager.Instance.GetCurrentMoney();
        txtCurrent.text = displayCurrent.ToString();
    }

    private void OnDestroy()
    {
        if (LootManager.Instance == null) return;
        
        LootManager.Instance.OnMoneyChanged -= OnMoneyChanged;
        LootManager.Instance.OnMoneyLoaded -= InitMoney;
    }

    // void OnEnable()
    // {
    //     LootManager.Instance.OnMoneyChanged += OnMoneyChanged;
    // }

    void OnDisable()
    {
        if (LootManager.Instance == null) return;

        LootManager.Instance.OnMoneyChanged -= OnMoneyChanged;
        LootManager.Instance.OnMoneyLoaded -= InitMoney;
    }

    // 🔥 gọi khi tiền thay đổi
    public void OnMoneyChanged(int delta)
    {
        ShowUI();

        pendingChange += delta;

        txtChange.gameObject.SetActive(true);
        txtChange.text = (pendingChange > 0 ? "+" : "") + pendingChange;

        txtChange.transform.DOKill();
        txtChange.transform.localScale = Vector3.one;
        txtChange.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);

        delayTween?.Kill();
        delayTween = DOVirtual.DelayedCall(delayTime, ApplyPending);
    }

    void ShowUI()
    {
        hideTween?.Kill();

        if (!root.gameObject.activeSelf)
        {
            root.gameObject.SetActive(true);
            root.alpha = 0;
            root.DOFade(1, 0.2f);
        }
    }

    void ApplyPending()
    {
        int target = LootManager.Instance.GetCurrentMoney();

        countTween?.Kill();

        int startValue = displayCurrent;

        countTween = DOTween.To(
            () => startValue,
            x =>
            {
                displayCurrent = x;
                txtCurrent.text = displayCurrent.ToString();
            },
            target,
            0.5f
        ).OnComplete(HideUI);

        txtChange.DOFade(0, 0.25f).OnComplete(() =>
        {
            txtChange.gameObject.SetActive(false);
            txtChange.alpha = 1;
        });

        pendingChange = 0;
    }

    void HideUI()
    {
        hideTween?.Kill();

        hideTween = root.DOFade(0, 0.25f).SetDelay(0.3f).OnComplete(() =>
        {
            root.gameObject.SetActive(false);
        });
    }
}
