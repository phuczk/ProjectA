using GlobalEnums;
using UnityEngine;
using System;

[Serializable]
public class DashAttackNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Attack;

    public float WindupTime = 0.15f;
    public float DashTime = 0.25f;
    public float DashSpeed = 8f;
    public float RecoverTime = 0.2f;

    private enum Phase { Windup, Dash, Recover }
    private Phase _currentPhase;
    private float _phaseTimer;
    private Vector2 _dashDir;

    public override void Enter()
    {
        _currentPhase = Phase.Windup;
        _phaseTimer = WindupTime;

        _dashDir = (machine.Target.position.x > machine.transform.position.x) ? Vector2.right : Vector2.left;
        machine.transform.localScale = new Vector3(Mathf.Sign(_dashDir.x), 1, 1);
        machine.Rb.linearVelocity = Vector2.zero;
    }

    public override void ExecuteLogic()
    {
        _phaseTimer -= Time.deltaTime;

        switch (_currentPhase)
        {
            case Phase.Windup:
                if (_phaseTimer <= 0)
                {
                    _currentPhase = Phase.Dash;
                    _phaseTimer = DashTime;
                }
                break;

            case Phase.Dash:
                machine.Rb.linearVelocity = _dashDir * DashSpeed;
                if (_phaseTimer <= 0)
                {
                    machine.Rb.linearVelocity = Vector2.zero;
                    _currentPhase = Phase.Recover;
                    _phaseTimer = RecoverTime;
                }
                break;

            case Phase.Recover:
                break;
        }

        if (_currentPhase == Phase.Recover && _phaseTimer <= 0)
        {
            IsFinished = true;
        }
    }

    public override void Exit() => machine.Rb.linearVelocity = Vector2.zero;
}
