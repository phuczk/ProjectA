using UnityEngine;
using System;
using GlobalEnums;

[Serializable]
public class IdleNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Idle;
    public RangedFloat IdleTimeRange;
    private float _timer;

    public override void Enter()
    {
        IsFinished = false;
        _timer = IdleTimeRange.RandomRange();
        machine.Movement.SetMoveDirection(Vector2.zero);
    }

    public override void ExecuteLogic()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
            machine.Movement.BobAndSway(true);
        }
        else
        {
            IsFinished = true;
        }
    }

    public override void Exit() { }
}
