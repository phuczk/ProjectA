using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class SaveSlotItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI detailText;

    [SerializeField] private Button loadBtn;
    [SerializeField] private Button deleteBtn;

    private int _slot;
    private SaveListUI _owner;

    public void Setup(SaveSystemz.SaveSlotInfo info, SaveListUI owner)
    {
        _slot = info.slot;
        _owner = owner;
        slotText.text = $"Slot {info.slot}";
        statusText.text = info.exists ? "Có dữ liệu" : "Trống";
        var timeStr = info.exists ? info.lastWriteTime.ToString("yyyy-MM-dd HH:mm") : "";
        var ps = info.playerSummary;
        var summary = info.exists ? $"HP {ps.maxHealth} | MP {ps.maxMana} | {ps.position.x:0.0},{ps.position.y:0.0} | Money {ps.currentMoney}" : "";
        detailText.text = info.exists ? $"{timeStr}  {summary}" : "";
    }

    private void Awake() {
        loadBtn.onClick.AddListener(LoadSlot);
        deleteBtn.onClick.AddListener(DeleteSlot);
    }

    private void LoadSlot()
    {
        transform.DOKill(true);
        Debug.Log($"Click slot {_slot}");
        _owner.OnSlotLeftClick(_slot);
    }

    private void DeleteSlot()
    {
        transform.DOKill(true);
        Debug.Log($"Click delete slot {_slot}");
        transform.localScale = Vector3.one * 0.98f;
            transform.DOScale(1f, 0.12f)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        _owner.OnSlotRightClick(_slot);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // transform.DOKill(true);
        // if (eventData.button == PointerEventData.InputButton.Left)
        // {
        //     _owner.OnSlotLeftClick(_slot);
        // }
        // else if (eventData.button == PointerEventData.InputButton.Right)
        // {
        //     transform.localScale = Vector3.one * 0.98f;
        //     transform.DOScale(1f, 0.12f)
        //         .SetEase(Ease.OutBack)
        //         .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        //     _owner.OnSlotRightClick(_slot);
        // }
    }

    private void OnDisable()
    {
        transform.DOKill();
    }
 
    private void OnDestroy()
    {
        transform.DOKill();
    }
}
