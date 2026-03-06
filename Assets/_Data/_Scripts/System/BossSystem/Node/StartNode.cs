using System;
using UnityEngine;

[Serializable]
public class StartNode : BossNode
{
    public override BossStateType StateType => BossStateType.Start;

    public override void Enter()
    {
        base.Enter();
        Debug.Log("StartNode.Enter() - Starting boss AI");
    }
    
    public override void ExecuteLogic()
    {
        //Debug.Log("StartNode.ExecuteLogic() - Starting boss AI");
        IsFinished = true;
    }
}
