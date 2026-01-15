using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    // Các hook cơ bản
    public virtual void OnApply(PlayerController player) { }
    public virtual void OnRemove(PlayerController player) { }

    // Tick mỗi frame (nếu cần)
    public virtual void OnUpdate(PlayerController player, float deltaTime) { }

    // Các sự kiện hành động cụ thể
    public virtual void OnJump(PlayerController player) { }
    public virtual void OnDash(PlayerController player) { }
    public virtual void OnHeal(PlayerController player) { }
    public virtual void OnAttack(PlayerController player) { }
    public virtual void OnFlipGravity(PlayerController player) { }
    public virtual void OnGunFire(PlayerController player, Vector2 direction) { }
    public virtual void OnSkillUsed(PlayerController player, Vector2 direction) { }
    public virtual void OnUltimateUsed(PlayerController player) { }

    // Nếu muốn kiểm tra điều kiện trước khi kích hoạt (cho passive có điều kiện)
    public virtual bool ShouldTrigger(PlayerController player) => true;
}