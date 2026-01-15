using UnityEngine;
using GlobalEnums;

public class AbilityItem : Interactable
{
    [SerializeField] private AbilityType _abilityType;
    public AbilityType AbilityType => _abilityType;

    protected override void OnInteract(Transform player)
    {
        var ability = player?.GetComponent<PlayerAbility>();
        if (ability != null)
        {
            ability.Unlock(_abilityType);
            Destroy(gameObject);
        }
    }
}
