#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;

public class SkillNodeView : Node
{
    public Port Input;
    public Port Output;
    
    protected SkillNode _node;
    protected BossController _machine;
    
    public SkillNodeView(SkillNode node, BossController machine)
    {
        _node = node;
        _machine = machine;
        
        title = $"Skill ({node.SkillType})";
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
        
        // OUTPUT PORT
        Output = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Single,
            typeof(bool));
        
        Output.portName = "Next";
        outputContainer.Add(Output);
        
        // INSPECTOR
        CreateInspector();
        
        RefreshExpandedState();
        RefreshPorts();
    }
    
    protected virtual void CreateInspector()
    {
        IMGUIContainer inspector = new IMGUIContainer(() =>
        {
            EditorGUILayout.LabelField($"Skill: {_node.SkillType}", EditorStyles.boldLabel);
            
            // Base Settings
            EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
            _node.Weight = EditorGUILayout.FloatField("Weight", _node.Weight);
            _node.NextNodeGuid = EditorGUILayout.TextField("Next Node GUID", _node.NextNodeGuid);
            
            // Animation Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            _node.AnimationName = EditorGUILayout.TextField("Animation Name", _node.AnimationName);
            _node.UseCustomAnimation = EditorGUILayout.Toggle("Use Custom Animation", _node.UseCustomAnimation);
            _node.AnimationType = (BossAnimationType)EditorGUILayout.EnumPopup("Animation Type", _node.AnimationType);
            _node.AttackVariant = EditorGUILayout.IntField("Attack Variant", _node.AttackVariant);
            _node.DelayAnimation = EditorGUILayout.FloatField("Delay Animation", _node.DelayAnimation);
            
            // Entry Conditions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Entry Conditions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_node.EntryConditions.Count}", EditorStyles.helpBox);
            
            // Transitions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_node.Transitions.Count}", EditorStyles.helpBox);
            
            if (GUI.changed)
                EditorUtility.SetDirty(_machine);
        });
        
        extensionContainer.Add(inspector);
    }
}
#endif
