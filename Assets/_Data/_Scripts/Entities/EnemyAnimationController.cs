using UnityEngine;
using System.Collections.Generic;
using GlobalEnums;

public class EnemyAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    private EnemyUniversalMachine _machine;

    private static readonly int IDLE_HASH = Animator.StringToHash("idle");
    private static readonly int MOVE_HASH = Animator.StringToHash("move");
    private static readonly int ATTACK_HASH = Animator.StringToHash("attack");
    private static readonly int DEATH_HASH = Animator.StringToHash("death");
    private static readonly int HURT_HASH = Animator.StringToHash("hurt");

    private string _currentAnimName = "";
    private int _currentAnimHash = 0;

    private void Awake()
    {
        _machine = GetComponent<EnemyUniversalMachine>();
        animator ??= GetComponent<Animator>();
        PlayIdle();
    }

    private void ResetAllBools()
    {
        animator.SetBool(IDLE_HASH, false);
        animator.SetBool(MOVE_HASH, false);
        animator.SetBool(ATTACK_HASH, false);
        animator.SetBool(DEATH_HASH, false);
        animator.SetBool(HURT_HASH, false);

        if (_currentAnimHash != 0)
        {
            animator.SetBool(_currentAnimHash, false);
        }
    }

    public void PlayAnimationByName(string animName)
    {
        if (_currentAnimName == animName) return;

        ResetAllBools();
        
        _currentAnimName = animName;
        _currentAnimHash = Animator.StringToHash(animName);
        
        animator.SetBool(_currentAnimHash, true);
    }

    public void PlayIdle() => PlayAnimationByName("idle");
    public void PlayMove() => PlayAnimationByName("move");
    public void PlayDeath() => PlayAnimationByName("death");
    public void PlayHurt() => PlayAnimationByName("hurt");
    
    public void PlayAttack(int variant = -1)
    {
        PlayAnimationByName("attack"); 
    }

    public void SetActionFinished(bool finished)
    {
        // "isActionFinished" là tên Parameter bạn đặt trong Animator transition
        animator.SetBool("isActionFinished", finished);
    }

    public void PlayComplexAction(string startAnimName)
    {
        ResetAllBools();
        animator.SetBool("isActionFinished", false); // Reset lại flag
        PlayAnimationByName(startAnimName);
    }

    public void ResetToIdle() => PlayIdle();

    public void OnAnimationEnd()
    {
        ResetToIdle();
    }
}