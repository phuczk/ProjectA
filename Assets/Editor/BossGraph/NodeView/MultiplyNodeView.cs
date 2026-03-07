#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;

public class MultiplyNodeView : Node
{
    public List<Port> OutputPorts = new List<Port>();
    
    private MultiplyNode _node;
    private BossController _machine;
    
    public MultiplyNodeView(MultiplyNode node, BossController machine)
    {
        _node = node;
        _machine = machine;
        
        title = "Multiply";
        viewDataKey = node.Guid;
        
        SetPosition(new Rect(node.GraphPosition, Vector2.zero));
        
        // INPUT PORT
        var inputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool));
        
        inputPort.portName = "In";
        inputContainer.Add(inputPort);
        
        // OUTPUT PORTS
        CreateOutputPorts();
        
        // INSPECTOR
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("Multiply Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"Output Count: {_node.Branches.Count}", EditorStyles.helpBox);
            
            // Display GUIDs for debugging
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output Node GUIDs", EditorStyles.boldLabel);
            for (int i = 0; i < _node.Branches.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Output {i + 1}:", GUILayout.Width(60));
                EditorGUILayout.LabelField(_node.Branches[i].NextNodeGuid, EditorStyles.helpBox);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add Output"))
            {
                AddOutputPort();
            }
            
            if (GUI.changed)
                if (_machine != null)
                    EditorUtility.SetDirty(_machine);
        });
        
        extensionContainer.Add(inspector);
        
        RefreshExpandedState();
        RefreshPorts();
    }
    
    private void CreateOutputPorts()
    {
        OutputPorts.Clear();
        outputContainer.Clear();
        
        for (int i = 0; i < _node.Branches.Count; i++)
        {
            Port port = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Single,
                typeof(bool));
            
            port.portName = $"Out {i + 1}";
            outputContainer.Add(port);
            OutputPorts.Add(port);
        }
    }
    
    private void AddOutputPort()
    {
        _node.Branches.Add(new MultiplyBranch());
        RefreshOutputPorts();
        
        if (_machine != null)
            EditorUtility.SetDirty(_machine);
    }
    
    private void RefreshOutputPorts()
    {
        CreateOutputPorts();
        RefreshPorts();
    }
    
    public MultiplyNode GetNode()
    {
        return _node;
    }
}
#endif
