using UnityEngine;
using System;

[Serializable]
public class PhaseNode : BossNode
{
    public string AnimatorState;
    public AudioClip PhaseAudio;
    public string PhaseName;
    public int TargetPhase = 1;
    public override BossStateType StateType => BossStateType.Phase;

    public override void Enter()
    {
        base.Enter();
        
        var bossController = machine as BossController;
        if (bossController != null)
        {
            bool isCurrentPhase = (bossController.CurrentPhase == TargetPhase);
            bool alreadyExecuted = bossController.IsPhaseNodeExecuted(Guid);
            
            if (isCurrentPhase && alreadyExecuted)
            {
                IsFinished = true;
                return;
            }
            
            bossController.MarkPhaseNodeExecuted(Guid);
            
            if (!isCurrentPhase)
            {
                if (machine.Animator != null && !string.IsNullOrEmpty(AnimatorState))
                {
                    machine.Animator.Play(AnimatorState);
                }

                if (PhaseAudio != null)
                {
                    AudioSource source = machine.GetComponent<AudioSource>();
                    if (source != null)
                    {
                        source.PlayOneShot(PhaseAudio);
                    }
                }
                
                bossController.SetCurrentPhase(TargetPhase);
                
                IsFinished = true;
                return;
            }
        }
        
        IsFinished = true;
    }

    public override void ExecuteLogic()
    {
        // All logic is handled in Enter()
        // Phase node should finish immediately based on Enter() logic
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

        boss.SetCurrentPhase(TargetPhase);
        boss.MarkPhaseNodeExecuted(Guid);

        return base.Execute(boss);
    }
}