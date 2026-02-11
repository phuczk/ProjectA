using System;
using UnityEngine;

public class GameEventBus : Singleton<GameEventBus>
{
    public event Action<PlayerController> OnJump;
    public event Action<PlayerController> OnDash;
    public event Action<PlayerController> OnHeal;
    public event Action<PlayerController, Vector2> OnGunFire;
    public event Action<PlayerController, Vector2> OnSkillUsed;

    protected override void Awake()
    {
        base.Awake();
    }

    public void RaiseJump(PlayerController p) => OnJump?.Invoke(p);
    public void RaiseDash(PlayerController p) => OnDash?.Invoke(p);
    public void RaiseHeal(PlayerController p) => OnHeal?.Invoke(p);
    public void RaiseGunFire(PlayerController p, Vector2 dir) => OnGunFire?.Invoke(p, dir);
    public void RaiseSkillUsed(PlayerController p, Vector2 dir) => OnSkillUsed?.Invoke(p, dir);
}
