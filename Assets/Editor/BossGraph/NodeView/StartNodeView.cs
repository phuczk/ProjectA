#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class StartNodeView : Node
{
    public Port Output;
    
    private StartNode _node;
    private BossController _machine;
    
    public StartNodeView(StartNode node, BossController machine)
    {
        _node = node;
        _machine = machine;
        
        title = "Start";
        viewDataKey = node.Guid;
        
        style.left = node.GraphPosition.x;
        style.top = node.GraphPosition.y;
        
        // OUTPUT PORT
        Output = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Single,
            typeof(bool));
        
        Output.portName = "Next";
        outputContainer.Add(Output);
        
        // INSPECTOR
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("Start Node", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This is the entry point of the Boss AI sequence.", EditorStyles.helpBox);
        });
        
        extensionContainer.Add(inspector);
        
        RefreshExpandedState();
        RefreshPorts();
    }
}
#endif
