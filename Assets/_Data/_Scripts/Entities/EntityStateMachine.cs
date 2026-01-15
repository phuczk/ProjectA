public abstract class EntityStateMachine<TEntity> : EntityBehaviour where TEntity : EntityStateMachine<TEntity>
{
    /// <summary>The state running in the machine</summary>
    protected EntityBehaviourState<TEntity> currentState { get; private set; }

    protected virtual void Update()
    {
        currentState?.LogicUpdate();
    }

    protected virtual void InitStates()
    {
    }

    public virtual void SwitchState(EntityBehaviourState<TEntity> state)
    {
        if (state == null) return;
        if (currentState == state) return;

        currentState?.Exit();
        var previousState = currentState;
        currentState = state;
        currentState.Enter();

        OnStateChanged(previousState, currentState);
    }

    protected virtual void OnStateChanged(EntityBehaviourState<TEntity> previousState, EntityBehaviourState<TEntity> newState)
    {
    }
}
