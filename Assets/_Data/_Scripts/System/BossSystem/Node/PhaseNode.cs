using UnityEngine;
using System;

[Serializable]
public class PhaseNode : BossNode
{
    public string AnimatorState;
    public AudioClip PhaseAudio;
    public string PhaseName;
    public override BossStateType StateType => BossStateType.Phase;

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"PhaseNode.Enter() - Entering phase: {PhaseName}");
    }

    public override void ExecuteLogic()
    {
        IsFinished = true;
    }

    public override BossNode Execute(BossController boss)
    {
        if (boss.Animator != null && !string.IsNullOrEmpty(AnimatorState))
        {
            boss.Animator.Play(AnimatorState);
        }

        if (PhaseAudio != null)
        {
            AudioSource source = boss.GetComponent<AudioSource>();

            if (source != null)
            {
                source.PlayOneShot(PhaseAudio);
            }
        }

        // Phase node finishes after playing animation/sound
        IsFinished = true;
        return null; // Let state machine handle transition
    }
}