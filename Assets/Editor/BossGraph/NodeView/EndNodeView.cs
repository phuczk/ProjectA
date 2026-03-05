#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class EndNodeView : Node
{
    public Port Input;
    
    private EndNode _node;
    private BossController _machine;
    
    public EndNodeView(EndNode node, BossController machine)
    {
        _node = node;
        _machine = machine;
        
        title = "End";
        viewDataKey = node.Guid;
        
        style.left = node.GraphPosition.x;
        style.top = node.GraphPosition.y;
        
        // INPUT PORT
        Input = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool));
        
        Input.portName = "In";
        inputContainer.Add(Input);
        
        // INSPECTOR
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("End Node", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This node ends the current cycle and returns to Start.", EditorStyles.helpBox);
        });
        
        extensionContainer.Add(inspector);
        
        RefreshExpandedState();
        RefreshPorts();
    }
}
#endif