using UnityEngine;

public interface IBossNode
{
    IBossNode Execute(BossContext context);
}
