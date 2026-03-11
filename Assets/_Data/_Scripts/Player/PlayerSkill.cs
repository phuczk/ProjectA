using UnityEngine;
using GlobalEnums;

public class PlayerSkill : MonoBehaviour
{
    private PlayerAbility _ability;
    private PlayerHealth _health;
    private PlayerController _playerController;
    private CursedObjectType _cursedObjectType;
    
    [SerializeField] private float skillCooldown = 0.5f;
    private float _lastSkillTime = 0f;

    public void Configure(PlayerAbility ability, PlayerHealth health)
    {
        _ability = ability;
        _health = health;
        _playerController = GetComponent<PlayerController>();
    }
    
    public void TrySpecialSkill(Vector2 inputDir)
    {
        if (Time.time - _lastSkillTime < skillCooldown) return;
        
        if (!_ability.Has(AbilityType.SpecialSkill)) return;
        
        var data = SaveSystem.Load();
        if (data.player == null) data.player = new PlayerData();
        
        //if (!_health.TryUseMana(1)) return; // Check if has enough mana
        
        _lastSkillTime = Time.time;
        GameEventBus.Instance.RaiseSkillUsed(_playerController, inputDir);
    }
}
