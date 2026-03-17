using UnityEngine;
using GlobalEnums;

public class AbilityItem : Interactable
{
    [SerializeField] private AbilityType _abilityType;
    [SerializeField] private string itemId;
    [SerializeField] private Sprite abilityIcon;
    
    public AbilityType AbilityType => _abilityType;

    private void Awake()
    {
        if (PersistentItemManager.Instance != null && PersistentItemManager.Instance.PlayerHasAbility(_abilityType))
        {
            Destroy(gameObject);
            return;
        }
    }

    protected override void OnInteract(Transform player)
    {
        var ability = player?.GetComponent<PlayerAbility>();
        if (ability != null)
        {
            ability.Unlock(_abilityType);
            
            if (PersistentItemManager.Instance != null)
            {
                PersistentItemManager.Instance.MarkAbilityItemCollected(itemId);
            }
            
            Destroy(gameObject);
            ShowInfoUi();
        }
    }

    private void ShowInfoUi()
    {
        InteractableUI.Instance.ShowInteractableInfo(InteractableType.Ability, abilityIcon, Localization.Get($"item.ability.{itemId}.name"), Localization.Get($"item.ability.{itemId}.desc"));
    }
}
