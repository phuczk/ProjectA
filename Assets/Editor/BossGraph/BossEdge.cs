#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BossEdge : Edge
{
    public BossAIGraphView GraphView;
    
    public BossEdge(BossAIGraphView graphView)
    {
        GraphView = graphView;
        
        // Handle connection events
        RegisterCallback<DetachFromPanelEvent>(OnDisconnected);
        RegisterCallback<AttachToPanelEvent>(OnConnected);
    }
    
    private void OnConnected(AttachToPanelEvent evt)
    {
        if (output != null && input != null)
        {
            GraphView.OnEdgeConnected(this);
        }
    }
    
    private void OnDisconnected(DetachFromPanelEvent evt)
    {
        if (output != null)
        {
            GraphView.OnEdgeDisconnected(this);
        }
    }
}
#endif
