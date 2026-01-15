using GlobalEnums;
using System;
using UnityEngine;

[Serializable]
public class AlertNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Alert;
    public float LostTargetTimeout = 0.5f;
    private float _lostTimer;

    public override void Enter() => _lostTimer = LostTargetTimeout;

    public override void ExecuteLogic()
    {
        _lostTimer -= Time.deltaTime;
        // machine.Movement.SetMoveDirection(Vector2.right * machine.LastSeenDir);
        // machine.Movement.TickMovement(0f);
        if (_lostTimer <= 0)
        {
            IsFinished = true;
        }
    }

    public override void Exit() { }
}
