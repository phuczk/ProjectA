#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;

public class AddNodeView : Node
{
    public List<Port> InputPorts = new List<Port>();
    public Port OutputPort;
    
    private AddNode _node;
    private BossController _machine;
    
    public AddNodeView(AddNode node, BossController machine)
    {
        _node = node;
        _machine = machine;
        
        title = "Add";
        viewDataKey = node.Guid;
        
        SetPosition(new Rect(node.GraphPosition, Vector2.zero));
        
        // INPUT PORTS
        CreateInputPorts();
        
        // OUTPUT PORT
        OutputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Single,
            typeof(bool));
        
        OutputPort.portName = "Out";
        outputContainer.Add(OutputPort);
        
        // INSPECTOR
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField("Add Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"Input Count: {_node.InputBranches.Count}", EditorStyles.helpBox);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input Node GUIDs", EditorStyles.boldLabel);
            for (int i = 0; i < _node.InputBranches.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Input {i + 1}:", GUILayout.Width(60));
                EditorGUILayout.LabelField(_node.InputBranches[i].NextNodeGuid, EditorStyles.helpBox);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add Input"))
            {
                AddInputPort();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            _node.NextNodeGuid = EditorGUILayout.TextField("Output Node GUID", _node.NextNodeGuid);
            
            if (GUI.changed)
                if (_machine != null)
                    EditorUtility.SetDirty(_machine);
        });
        
        extensionContainer.Add(inspector);
        
        RefreshExpandedState();
        RefreshPorts();
    }
    
    private void CreateInputPorts()
    {
        InputPorts.Clear();
        inputContainer.Clear();
        
        for (int i = 0; i < _node.InputBranches.Count; i++)
        {
            Port port = InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Single,
                typeof(bool));
            
            port.portName = $"In {i + 1}";
            inputContainer.Add(port);
            InputPorts.Add(port);
        }
    }
    
    private void AddInputPort()
    {
        _node.InputBranches.Add(new AddBranch());
        RefreshInputPorts();
        
        if (_machine != null)
            EditorUtility.SetDirty(_machine);
    }
    
    private void RefreshInputPorts()
    {
        CreateInputPorts();
        RefreshPorts();
    }
    
    public AddNode GetNode()
    {
        return _node;
    }
}
#endif
