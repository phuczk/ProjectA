#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

public class TransitionEdge : Edge 
{
    public StateTransition Transition;

    public TransitionEdge(StateTransition transition)
    {
        Transition = transition;
    }
}
#endif
