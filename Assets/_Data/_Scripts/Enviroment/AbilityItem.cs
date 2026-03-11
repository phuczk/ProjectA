using UnityEngine;
using GlobalEnums;

public class AbilityItem : Interactable
{
    [SerializeField] private AbilityType _abilityType;
    [SerializeField] private string itemId;
    
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
        }
    }
}
