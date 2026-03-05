#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class IfNodeView : Node
{
    public Port Input;
    public Port ElsePort;

    private IfNode _node;
    private BossController _machine;

    public Dictionary<ConditionBranch, Port> ConditionPorts = new();

    public IfNodeView(IfNode node, BossController machine)
    {
        _node = node;
        _machine = machine;

        title = "If";

        viewDataKey = node.Guid;

        style.left = node.GraphPosition.x;
        style.top = node.GraphPosition.y;

        //--------------------------------
        // INPUT
        //--------------------------------

        Input = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool));

        Input.portName = "In";
        inputContainer.Add(Input);

        //--------------------------------
        // ADD CONDITION BUTTON
        //--------------------------------

        IMGUIContainer header = new IMGUIContainer(() =>
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                AddCondition();
            }

            EditorGUILayout.EndHorizontal();
        });

        extensionContainer.Add(header);

        SyncPorts();

        RefreshExpandedState();
    }

    //--------------------------------
    // SYNC PORTS
    //--------------------------------

    private void SyncPorts()
    {
        outputContainer.Clear();
        ConditionPorts.Clear();

        //--------------------------------
        // CONDITION PORTS
        //--------------------------------

        foreach (var branch in _node.Conditions)
        {
            Port port = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Single,
                typeof(bool));

            port.portName = "";

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            //--------------------------------
            // CONDITION BUTTON
            //--------------------------------

            Button conditionBtn = new Button(() =>
            {
                ShowConditionMenu(branch);
            });

            conditionBtn.text = branch.Condition == null
                ? "Condition"
                : branch.Condition.GetType().Name;

            //--------------------------------
            // REMOVE BUTTON
            //--------------------------------

            Button removeBtn = new Button(() =>
            {
                RemoveCondition(branch);
            });

            removeBtn.text = "x";
            removeBtn.style.width = 20;

            container.Add(conditionBtn);
            container.Add(removeBtn);

            port.contentContainer.Add(container);

            outputContainer.Add(port);

            ConditionPorts.Add(branch, port);
        }

        //--------------------------------
        // ELSE PORT
        //--------------------------------

        ElsePort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Single,
            typeof(bool));

        ElsePort.portName = "Else";

        outputContainer.Add(ElsePort);

        RefreshPorts();
        RefreshExpandedState();
    }

    //--------------------------------
    // ADD CONDITION
    //--------------------------------

    private void AddCondition()
    {
        _node.Conditions.Add(new ConditionBranch());

        EditorUtility.SetDirty(_machine);

        SyncPorts();
    }

    //--------------------------------
    // REMOVE CONDITION
    //--------------------------------

    private void RemoveCondition(ConditionBranch branch)
    {
        _node.Conditions.Remove(branch);

        EditorUtility.SetDirty(_machine);

        SyncPorts();
    }

    //--------------------------------
    // CONDITION MENU
    //--------------------------------

    private void ShowConditionMenu(ConditionBranch branch)
    {
        GenericMenu menu = new GenericMenu();

        var types = TypeCache
            .GetTypesDerivedFrom<IBossCondition>()
            .Where(t => !t.IsAbstract);

        foreach (var type in types)
        {
            menu.AddItem(
                new GUIContent(type.Name),
                branch.Condition?.GetType() == type,
                () =>
                {
                    branch.Condition =
                        (IBossCondition)Activator.CreateInstance(type);

                    EditorUtility.SetDirty(_machine);

                    SyncPorts();
                });
        }

        menu.ShowAsContext();
    }
}
#endif