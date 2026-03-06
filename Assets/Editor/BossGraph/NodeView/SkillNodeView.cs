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
    private bool _isCollapsed = false;
    private Button _collapseButton;
    private IMGUIContainer _inspectorContainer;
    
    public SkillNodeView(SkillNode node, BossController machine)
    {
        _node = node;
        _machine = machine;
        
        // Load saved collapse state
        _isCollapsed = node.IsCollapsed;
        
        // Dynamic title based on actual node type
        title = node.GetType().Name.Replace("Node", "");
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
        
        // CREATE COLLAPSE BUTTON
        CreateCollapseButton();
        
        // INSPECTOR
        CreateInspector();
        
        // Apply initial collapse state
        UpdateCollapseButton();
        UpdateInspectorVisibility();
        
        RefreshExpandedState();
        RefreshPorts();
    }
    
    private void CreateCollapseButton()
    {
        _collapseButton = new Button(() =>
        {
            _isCollapsed = !_isCollapsed;
            
            // Save state to node
            _node.IsCollapsed = _isCollapsed;
            if (_machine != null)
                EditorUtility.SetDirty(_machine);
            
            UpdateCollapseButton();
            UpdateInspectorVisibility();
        })
        {
            text = "▼"
        };
        
        _collapseButton.style.width = 24;
        _collapseButton.style.height = 20;
        
        titleContainer.Insert(0, _collapseButton);
    }
    
    private void UpdateCollapseButton()
    {
        if (_collapseButton != null)
        {
            _collapseButton.text = _isCollapsed ? "▶" : "▼";
        }
    }
    
    private void UpdateInspectorVisibility()
    {
        if (_inspectorContainer != null)
        {
            _inspectorContainer.style.display = _isCollapsed ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
    
    protected virtual void CreateInspector()
    {
        _inspectorContainer = new IMGUIContainer(() =>
        {
            if (_isCollapsed) return;
            
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
            
            // Dynamic Properties
            DrawDynamicProperties();
            
            // Entry Conditions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Entry Conditions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_node.EntryConditions.Count}", EditorStyles.helpBox);
            
            // Transitions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_node.Transitions.Count}", EditorStyles.helpBox);
            
            if (GUI.changed)
                if (_machine != null)
                    EditorUtility.SetDirty(_machine);
        });
        
        extensionContainer.Add(_inspectorContainer);
    }
    
    protected virtual void DrawDynamicProperties()
    {
        // Override in derived classes or use reflection
        // This method will draw properties specific to each skill type
        var nodeType = _node.GetType();
        var fields = nodeType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            // Skip base class fields that are already drawn
            if (IsBaseClassField(field.Name)) continue;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{field.Name}", EditorStyles.boldLabel);
            
            var value = field.GetValue(_node);
            var newValue = DrawFieldForType(field.FieldType, field.Name, value);
            if (newValue != null && !newValue.Equals(value))
            {
                field.SetValue(_node, newValue);
            }
        }
    }
    
    protected virtual bool IsBaseClassField(string fieldName)
    {
        string[] baseFields = { "Weight", "NextNodeGuid", "AnimationName", "UseCustomAnimation", 
                               "AnimationType", "AttackVariant", "DelayAnimation", "EntryConditions", 
                               "Transitions", "Guid", "GraphPosition", "IsFinished", "StateType", "IsCollapsed" };
        return System.Array.IndexOf(baseFields, fieldName) >= 0;
    }
    
    protected virtual object DrawFieldForType(System.Type fieldType, string label, object value)
    {
        if (fieldType == typeof(string))
        {
            return EditorGUILayout.TextField(label, (string)value);
        }
        else if (fieldType == typeof(int))
        {
            return EditorGUILayout.IntField(label, (int)value);
        }
        else if (fieldType == typeof(float))
        {
            return EditorGUILayout.FloatField(label, (float)value);
        }
        else if (fieldType == typeof(bool))
        {
            return EditorGUILayout.Toggle(label, (bool)value);
        }
        else if (fieldType == typeof(Vector2))
        {
            return EditorGUILayout.Vector2Field(label, (Vector2)value);
        }
        else if (fieldType == typeof(Vector3))
        {
            return EditorGUILayout.Vector3Field(label, (Vector3)value);
        }
        else if (fieldType.IsEnum)
        {
            return EditorGUILayout.EnumPopup(label, (System.Enum)value);
        }
        else if (fieldType == typeof(GameObject))
        {
            return EditorGUILayout.ObjectField(label, (GameObject)value, typeof(GameObject), false);
        }
        else if (fieldType == typeof(AudioClip))
        {
            return EditorGUILayout.ObjectField(label, (AudioClip)value, typeof(AudioClip), false);
        }
        else if (fieldType == typeof(LayerMask))
        {
            // LayerMask is actually an int, so we need to handle it specially
            int layerValue = value is LayerMask mask ? (int)mask : (int)value;
            int newLayerValue = EditorGUILayout.LayerField(label, layerValue);
            return (LayerMask)newLayerValue;
        }
        else
        {
            EditorGUILayout.LabelField($"{label}: {fieldType.Name} (Unsupported)");
            return value;
        }
    }
    
    public SkillNode GetNode()
    {
        return _node;
    }
}
#endif
