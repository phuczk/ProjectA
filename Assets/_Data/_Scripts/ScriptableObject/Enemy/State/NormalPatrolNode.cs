using GlobalEnums;
using UnityEngine;
using System;

[Serializable]
public class UpPatrolNode : EnemyStateNode
{
    public override EnemyStateType StateType => EnemyStateType.Patrol;
    public RangedFloat PatrolTimeRange;
    public float MoveSpeed = 1f;
    public GameObject PatrolArea;

    private float _timer;
    private float _dir;
    private float _turnCooldown;

    public override void Enter()
    {
        IsFinished = false;
        _timer = PatrolTimeRange.RandomRange();
        _dir = machine.transform.localScale.x > 0 ? 1f : -1f;
        _turnCooldown = 0.2f;
    }

    public override void ExecuteLogic()
    {
        _timer -= Time.deltaTime;
        _turnCooldown -= Time.deltaTime;

        if (_timer <= 0f)
        {
            IsFinished = true;
            return;
        }

        if (_turnCooldown <= 0f && (!machine.HasGroundAhead((int)_dir) || machine.HasWallAhead((int)_dir)))
        {
            _dir *= -1f;
            _turnCooldown = 0.5f;

            machine.transform.localScale = new Vector3(_dir, 1, 1);
        }

        machine.Movement.SetMoveDirection(Vector2.up * _dir);
        machine.Movement.TickMovement(MoveSpeed);
    }
    
    public override void Exit()
    {
        machine.Movement.SetMoveDirection(Vector2.zero);
    }
}
