#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class StateNodeView : Node
{
    public EnemyStateNode StateNode;
    public Port Input;
    public Dictionary<StateTransition, Port> TransitionPorts = new Dictionary<StateTransition, Port>();
    private EnemyUniversalMachine _machine;

    public StateNodeView(EnemyStateNode node, EnemyUniversalMachine machine)
    {
        StateNode = node;
        _machine = machine;
        title = node.GetType().Name; 
        viewDataKey = node.Guid;
        
        style.left = node.GraphPosition.x;
        style.top = node.GraphPosition.y;

        Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        Input.portName = "In";
        inputContainer.Add(Input);

        IMGUIContainer nodeDataInspector = new IMGUIContainer(() => {
            EditorGUILayout.LabelField("Node Settings", EditorStyles.boldLabel);
            DrawFieldsForObject(StateNode);
            EditorGUILayout.Space(5);
            Handles.color = Color.gray;
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        });
        extensionContainer.Add(nodeDataInspector);

        IMGUIContainer transitionInspector = new IMGUIContainer(() => {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(25))) AddEmptyTransition();
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < StateNode.Transitions.Count; i++)
            {
                var trans = StateNode.Transitions[i];
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                string dName = trans.Decision?.GetType().Name ?? "Select Decision";
                if (GUILayout.Button(dName, EditorStyles.miniPullDown)) ShowDecisionMenu(trans);
                
                if (GUILayout.Button("x", GUILayout.Width(20))) {
                    RemoveTransition(trans);
                    return; 
                }
                EditorGUILayout.EndHorizontal();

                if (trans.Decision != null) DrawFieldsForObject(trans.Decision);
                EditorGUILayout.EndVertical();
            }
        });
        extensionContainer.Add(transitionInspector);

        SyncPorts();
        RefreshExpandedState();
    }

    private void DrawFieldsForObject(object obj)
    {
        if (obj == null) return;

        EditorGUI.BeginChangeCheck();

        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.Name != "Guid" && f.Name != "Transitions" && f.Name != "GraphPosition");

        foreach (var field in fields)
        {
            object val = field.GetValue(obj);
            string label = field.Name;

            if (field.FieldType == typeof(RangedFloat))
            {
                RangedFloat rf = (RangedFloat)val;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                EditorGUIUtility.labelWidth = 30;
                rf.min = EditorGUILayout.FloatField("Min", rf.min);
                rf.max = EditorGUILayout.FloatField("Max", rf.max);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                field.SetValue(obj, rf);
            }
            else if (field.FieldType == typeof(RangedInt))
            {
                RangedInt ri = (RangedInt)val;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                EditorGUIUtility.labelWidth = 30;
                ri.min = EditorGUILayout.IntField("Min", ri.min);
                ri.max = EditorGUILayout.IntField("Max", ri.max);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                field.SetValue(obj, ri);
            }
            else
            {
                var rangeAttr = field.GetCustomAttribute<RangeAttribute>();
                if (rangeAttr != null)
                {
                    if (field.FieldType == typeof(float))
                        field.SetValue(obj, EditorGUILayout.Slider(label, (float)val, rangeAttr.min, rangeAttr.max));
                    else if (field.FieldType == typeof(int))
                        field.SetValue(obj, EditorGUILayout.IntSlider(label, (int)val, (int)rangeAttr.min, (int)rangeAttr.max));
                }
                else
                {
                    if (field.FieldType == typeof(float)) field.SetValue(obj, EditorGUILayout.FloatField(label, (float)val));
                    else if (field.FieldType == typeof(int)) field.SetValue(obj, EditorGUILayout.IntField(label, (int)val));
                    else if (field.FieldType == typeof(bool)) field.SetValue(obj, EditorGUILayout.Toggle(label, (bool)val));
                    else if (field.FieldType == typeof(Vector2)) field.SetValue(obj, EditorGUILayout.Vector2Field(label, (Vector2)val));
                    else if (field.FieldType == typeof(LayerMask)) field.SetValue(obj, (LayerMask)EditorGUILayout.LayerField(label, (LayerMask)val));
                    else if (field.FieldType.IsEnum) field.SetValue(obj, EditorGUILayout.EnumPopup(label, (Enum)val));
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_machine);
        }
    }

    private void SyncPorts()
    {
        outputContainer.Clear();
        TransitionPorts.Clear();
        foreach (var trans in StateNode.Transitions)
        {
            Port outPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            var target = _machine.StateNodes.FirstOrDefault(n => n.Guid == trans.TargetNodeGuid);
            outPort.portName = target != null ? $"-> {target.GetType().Name}" : "-> None";
            outputContainer.Add(outPort);
            TransitionPorts[trans] = outPort;
        }
        RefreshPorts();
    }

    public override void SetPosition(Rect newPos) { base.SetPosition(newPos); StateNode.GraphPosition = new Vector2(newPos.xMin, newPos.yMin); }

    private void AddEmptyTransition() 
    { 
        StateNode.Transitions.Add(new StateTransition()); 
        schedule.Execute(() => SyncPorts()).ExecuteLater(10); 
    }

    private void RemoveTransition(StateTransition trans) 
    { 
        StateNode.Transitions.Remove(trans); 
        schedule.Execute(() => SyncPorts()).ExecuteLater(10); 
    }

    private void ShowDecisionMenu(StateTransition trans)
    {
        GenericMenu menu = new GenericMenu();
        var types = TypeCache.GetTypesDerivedFrom<IStateDecision>().Where(t => !t.IsAbstract);
        foreach (var type in types)
        {
            menu.AddItem(new GUIContent(type.Name), trans.Decision?.GetType() == type, () => {
                trans.Decision = (IStateDecision)Activator.CreateInstance(type);
                EditorUtility.SetDirty(_machine);
                SyncPorts(); 
            });
        }
        menu.ShowAsContext();
    }
}
#endif
