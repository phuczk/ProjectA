using UnityEngine;
using System.Collections.Generic;
using GlobalEnums;

public class BossAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    private BossController _machine;

    private static readonly int IDLE_HASH = Animator.StringToHash("idle");
    private static readonly int MOVE_HASH = Animator.StringToHash("move");
    private static readonly int ATTACK_HASH = Animator.StringToHash("attack");
    private static readonly int SPECIAL_HASH = Animator.StringToHash("special");
    private static readonly int SHOOT_HASH = Animator.StringToHash("shoot");
    private static readonly int DEATH_HASH = Animator.StringToHash("death");
    private static readonly int HURT_HASH = Animator.StringToHash("hurt");

    private string _currentAnimName = "";
    private int _currentAnimHash = 0;

    private void Awake()
    {
        _machine = GetComponent<BossController>();
        animator ??= GetComponent<Animator>();
        PlayIdle();
    }

    private void ResetAllBools()
    {
        animator.SetBool(IDLE_HASH, false);
        animator.SetBool(MOVE_HASH, false);
        animator.SetBool(ATTACK_HASH, false);
        animator.SetBool(SPECIAL_HASH, false);
        animator.SetBool(SHOOT_HASH, false);
        animator.SetBool(DEATH_HASH, false);
        animator.SetBool(HURT_HASH, false);

        if (_currentAnimHash != 0)
        {
            animator.ResetTrigger(_currentAnimHash);
        }
    }

    public void PlayIdle()
    {
        ResetAllBools();
        animator.SetBool(IDLE_HASH, true);
        _currentAnimName = "idle";
        _currentAnimHash = IDLE_HASH;
    }

    public void PlayMove()
    {
        ResetAllBools();
        animator.SetBool(MOVE_HASH, true);
        _currentAnimName = "move";
        _currentAnimHash = MOVE_HASH;
    }

    public void PlayAttack(string attackVariant = "")
    {
        ResetAllBools();
        animator.SetBool(ATTACK_HASH, true);
        
        if (!string.IsNullOrEmpty(attackVariant))
        {
            animator.Play($"Attack{attackVariant}");
        }
        else
        {
            animator.Play("Attack");
        }
        
        _currentAnimName = $"Attack{attackVariant}";
        _currentAnimHash = ATTACK_HASH;
    }

    public void PlayShoot()
    {
        ResetAllBools();
        animator.SetBool(SHOOT_HASH, true);
        animator.Play("Shoot");
        _currentAnimName = "shoot";
        _currentAnimHash = SHOOT_HASH;
    }

    public void PlaySpecial()
    {
        ResetAllBools();
        animator.SetBool(SPECIAL_HASH, true);
        animator.Play("Special");
        _currentAnimName = "special";
        _currentAnimHash = SPECIAL_HASH;
    }

    public void PlayDeath()
    {
        ResetAllBools();
        animator.SetBool(DEATH_HASH, true);
        animator.Play("Death");
        _currentAnimName = "death";
        _currentAnimHash = DEATH_HASH;
    }

    public void PlayHurt()
    {
        ResetAllBools();
        animator.SetBool(HURT_HASH, true);
        animator.Play("Hurt");
        _currentAnimName = "hurt";
        _currentAnimHash = HURT_HASH;
    }

    public void PlayCustomAnimation(string animationName)
    {
        if (string.IsNullOrEmpty(animationName)) return;
        
        ResetAllBools();
        animator.Play(animationName);
        _currentAnimName = animationName;
        _currentAnimHash = 0;
    }

    public string GetCurrentAnimationName()
    {
        return _currentAnimName;
    }

    public bool IsPlayingAnimation(string animationName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }

    public bool IsAttacking()
    {
        return animator.GetBool(ATTACK_HASH) || IsPlayingAnimation("Attack");
    }

    public bool IsShooting()
    {
        return animator.GetBool(SHOOT_HASH) || IsPlayingAnimation("Shoot");
    }

    public bool IsUsingSpecial()
    {
        return animator.GetBool(SPECIAL_HASH) || IsPlayingAnimation("Special");
    }

    public bool IsHurting()
    {
        return animator.GetBool(HURT_HASH) || IsPlayingAnimation("Hurt");
    }

    public bool IsDead()
    {
        return animator.GetBool(DEATH_HASH) || IsPlayingAnimation("Death");
    }

    public bool IsIdle()
    {
        return animator.GetBool(IDLE_HASH) || IsPlayingAnimation("Idle");
    }

    public bool IsMoving()
    {
        return animator.GetBool(MOVE_HASH) || IsPlayingAnimation("Move");
    }

    public float GetCurrentAnimationLength()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    public float GetCurrentAnimationTime()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public void SetAnimationSpeed(float speed)
    {
        animator.speed = speed;
    }

    public void ResetAnimationSpeed()
    {
        animator.speed = 1f;
    }

    public void CrossFadeToAnimation(string animationName, float fadeDuration = 0.25f)
    {
        if (string.IsNullOrEmpty(animationName)) return;
        
        ResetAllBools();
        animator.CrossFade(animationName, fadeDuration);
        _currentAnimName = animationName;
        _currentAnimHash = 0;
    }

    public void SetAnimatorParameter(string parameterName, bool value)
    {
        animator.SetBool(parameterName, value);
    }

    public void SetAnimatorParameter(string parameterName, float value)
    {
        animator.SetFloat(parameterName, value);
    }

    public void SetAnimatorParameter(string parameterName, int value)
    {
        animator.SetInteger(parameterName, value);
    }

    public void TriggerAnimatorParameter(string parameterName)
    {
        animator.SetTrigger(parameterName);
    }

    private void OnValidate()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
}
