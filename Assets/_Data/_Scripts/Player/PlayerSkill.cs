using UnityEngine;
using GlobalEnums;

public class PlayerSkill : MonoBehaviour
{
    private PlayerAbility _ability;
    private PlayerHealth _health;
    private CursedObjectType _cursedObjectType;

    public void Configure(PlayerAbility ability, PlayerHealth health)
    {
        _ability = ability;
        _health = health;
    }
    public void TrySpecialSkill(Vector2 inputDir)
    {
        if (!_ability.Has(AbilityType.SpecialSkill)) return;
        var data = SaveSystemz.Load();
        if (data.player == null) data.player = new PlayerData();
        Debug.Log($"TrySpecialSkill {_health.CurrentMana}");
        _health.TryUseMana(3);
    }
}
