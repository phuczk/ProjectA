#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;

public class RandomNodeView : Node
{
    public Port Input;

    private RandomNode _node;
    private BossController _machine;

    public Dictionary<RandomBranch, Port> BranchPorts = new();

    public RandomNodeView(RandomNode node, BossController machine)
    {
        Debug.Log("RandomNodeView constructor called");
        _node = node;
        _machine = machine;

        title = "Random Node";
        viewDataKey = node.Guid;

        style.left = node.GraphPosition.x;
        style.top = node.GraphPosition.y;

        Debug.Log($"RandomNodeView setup complete. Position: {node.GraphPosition}");

        //--------------------------------
        // INPUT PORT
        //--------------------------------

        Input = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool));

        Input.portName = "In";
        inputContainer.Add(Input);

        //--------------------------------
        // ADD BUTTON
        //--------------------------------

        IMGUIContainer header = new IMGUIContainer(() =>
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Random Branches", EditorStyles.boldLabel);

            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                AddBranch();
            }

            EditorGUILayout.EndHorizontal();
        });

        extensionContainer.Add(header);

        SyncPorts();

        RefreshExpandedState();
        RefreshPorts();
    }

    //--------------------------------
    // CREATE PORTS
    //--------------------------------

    private void SyncPorts()
    {
        outputContainer.Clear();
        BranchPorts.Clear();

        foreach (var branch in _node.Branches)
        {
            var port = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Single,
                typeof(bool));

            port.portName = "";

            //--------------------------------
            // UI CONTAINER
            //--------------------------------

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            //--------------------------------
            // PERCENT FIELD
            //--------------------------------

            IntegerField percentField = new IntegerField();
            percentField.value = branch.Percent;
            percentField.style.width = 60;

            percentField.RegisterValueChangedCallback(evt =>
            {
                branch.Percent = evt.newValue;
                EditorUtility.SetDirty(_machine);
            });

            //--------------------------------
            // REMOVE BUTTON
            //--------------------------------

            Button removeBtn = new Button(() =>
            {
                RemoveBranch(branch);
            });

            removeBtn.text = "x";
            removeBtn.style.width = 20;

            //--------------------------------
            // ADD ELEMENTS
            //--------------------------------

            container.Add(percentField);
            container.Add(removeBtn);

            port.contentContainer.Add(container);

            //--------------------------------

            outputContainer.Add(port);

            BranchPorts.Add(branch, port);
        }

        RefreshPorts();
        RefreshExpandedState();
    }

    //--------------------------------
    // ADD BRANCH
    //--------------------------------

    private void AddBranch()
    {
        _node.Branches.Add(new RandomBranch()
        {
            Percent = 0
        });

        EditorUtility.SetDirty(_machine);

        SyncPorts();
    }

    //--------------------------------
    // REMOVE BRANCH
    //--------------------------------

    private void RemoveBranch(RandomBranch branch)
    {
        _node.Branches.Remove(branch);

        EditorUtility.SetDirty(_machine);

        SyncPorts();
    }
}
#endif