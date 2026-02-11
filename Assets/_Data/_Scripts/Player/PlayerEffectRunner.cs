using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectRunner : MonoBehaviour
{
    [SerializeField] private CursedList cursedList;
    private readonly List<Effect> _effects = new();
    private PlayerController _player;

    public void RebuildEffects(HashSet<string> equippedSet)
    {
        ClearAll();

        foreach (var id in equippedSet)
        {
            var data = cursedList.GetById(id);
            if (data == null) continue;

            if (data.Effects == null) continue;

            foreach (var effect in data.Effects)
            {
                AddEffect(effect);
            }
        }
    }

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        var bus = GameEventBus.Instance;

        bus.OnGunFire += HandleGunFire;
        bus.OnHeal += HandleHeal;
    }

    private void OnDisable()
    {
        if (GameEventBus.Instance == null) return;
        var bus = GameEventBus.Instance;

        bus.OnGunFire -= HandleGunFire;
        bus.OnHeal -= HandleHeal;
    }

    public void AddEffect(Effect e)
    {
        _effects.Add(e);
        e.OnApply(_player);
    }

    public void RemoveEffect(Effect e)
    {
        e.OnRemove(_player);
        _effects.Remove(e);
    }

    private void ClearAll()
    {
        foreach (var e in _effects)
            e.OnRemove(_player);

        _effects.Clear();
    }

    private void HandleGunFire(PlayerController player, Vector2 dir)
    {
        if (player != _player) return;

        foreach (var e in _effects)
            e.OnGunFire(player, dir);
    }

    private void HandleHeal(PlayerController player)
    {
        if (player != _player) return;

        foreach (var e in _effects)
            e.OnHeal(player);
    }
}
