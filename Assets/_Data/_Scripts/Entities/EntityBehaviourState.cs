public abstract class EntityBehaviourState<TEntity> where TEntity : EntityStateMachine<TEntity>
{
    /// <summary>The entity behaviour using this state</summary>
    public readonly TEntity entity;

    protected EntityBehaviourState(TEntity entity)
    {
        this.entity = entity;
    }

    /// <summary>This method should be called in entity.Update()</summary> 
    public virtual void LogicUpdate()
    {
    }

    /// <summary>This method should be called when the entity switch to this state</summary> 
    public virtual void Enter()
    {
    }

    /// <summary>This method should be called when the entity is in this state and switch to another state</summary> 
    public virtual void Exit()
    {
    }
}