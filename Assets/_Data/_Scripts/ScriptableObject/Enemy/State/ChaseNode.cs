using GlobalEnums;
using System;
using UnityEngine;

[Serializable]
public class ChaseNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Chase;
    public float ChaseSpeed = 5f;
    public float LostTargetTimeout = 5f;
    private float _lostTimer;

    public override void Enter() => _lostTimer = LostTargetTimeout;

    public override void ExecuteLogic()
    {
        if (machine.IsPlayerInChaseRange())
        {
            _lostTimer = LostTargetTimeout;
            machine.LastSeenDir = (machine.Target.position.x > machine.transform.position.x) ? 1 : -1;
        }
        else
        {
            _lostTimer -= Time.deltaTime;
        }

        machine.Movement.SetMoveDirection(Vector2.right * machine.LastSeenDir);
        machine.Movement.TickMovement(ChaseSpeed);
        if (_lostTimer <= 0)
        {
            IsFinished = true;
        }
    }

    public override void Exit() { }
}
