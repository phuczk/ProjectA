using System;
using UnityEngine;

[Serializable]
public class EndNode : BossNode
{
    public string StartNodeGuid;
    public override BossStateType StateType => BossStateType.End;
    
    public override void ExecuteLogic()
    {
        Debug.Log("EndNode.ExecuteLogic() - Ending boss AI cycle");
        IsFinished = true;
    }
}
