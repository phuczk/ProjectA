#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class RandomNodeView : Node
{
    public Port Input;

    private RandomNode _node;
    private BossController _machine;

    public Dictionary<RandomBranch, Port> BranchPorts = new Dictionary<RandomBranch, Port>();

    public RandomNodeView(RandomNode node, BossController machine)
    {
        _node = node;
        _machine = machine;

        title = "Random";

        viewDataKey = node.Guid;

        style.left = node.GraphPosition.x;
        style.top = node.GraphPosition.y;

        Input = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool));

        Input.portName = "In";

        inputContainer.Add(Input);

        IMGUIContainer header = new IMGUIContainer(() =>
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Branches", EditorStyles.boldLabel);

            if (GUILayout.Button("+", GUILayout.Width(25f)))
            {
                AddBranch();
            }

            EditorGUILayout.EndHorizontal();
        });

        extensionContainer.Add(header);

        SyncPorts();

        RefreshExpandedState();
    }

    private void SyncPorts()
    {
        outputContainer.Clear();
        BranchPorts.Clear();

        foreach (var branch in _node.Branches)
        {
            Port port = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Single,
                typeof(bool));

            port.portName = $"{branch.Percent}%";

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            TextField percentField = new TextField()
            {
                value = branch.Percent.ToString(),
                style = { width = 60f }
            };

            percentField.RegisterValueChangedCallback(evt =>
            {
                if (float.TryParse(evt.newValue, out float percent))
                {
                    branch.Percent = percent;
                    port.portName = $"{percent}%";
                    if (_machine != null)
                        EditorUtility.SetDirty(_machine);
                }
            });

            Button removeBtn = new Button(() =>
            {
                RemoveBranch(branch);
            });

            removeBtn.text = "x";
            removeBtn.style.width = 20f;

            container.Add(percentField);
            container.Add(removeBtn);

            port.contentContainer.Add(container);

            outputContainer.Add(port);

            BranchPorts.Add(branch, port);
        }

        RefreshPorts();
        RefreshExpandedState();
    }

    public void UpdateBranchConnections()
    {
        foreach (var kvp in BranchPorts)
        {
            RandomBranch branch = kvp.Key;
            Port port = kvp.Value;
            
            if (port.connected)
            {
                var edge = port.connections.FirstOrDefault();
                if (edge != null)
                {
                    var connectedNode = edge.input.node;
                    if (connectedNode is SkillNodeView skillView)
                    {
                        var skillNode = skillView.GetNode();
                        if (skillNode != null)
                        {
                            branch.NextNodeGuid = skillNode.Guid;
                        }
                    }
                    else if (connectedNode is StartNodeView startView)
                    {
                        var startNode = startView.GetNode();
                        if (startNode != null)
                        {
                            branch.NextNodeGuid = startNode.Guid;
                        }
                    }
                    else if (connectedNode is EndNodeView endView)
                    {
                        var endNode = endView.GetNode();
                        if (endNode != null)
                        {
                            branch.NextNodeGuid = endNode.Guid;
                        }
                    }
                    else if (connectedNode is AttackSkillNodeView attackView)
                    {
                        var attackNode = attackView.GetNode();
                        if (attackNode != null)
                        {
                            branch.NextNodeGuid = attackNode.Guid;
                        }
                    }
                    else if (connectedNode is ShootSkillNodeView shootView)
                    {
                        var shootNode = shootView.GetNode();
                        if (shootNode != null)
                        {
                            branch.NextNodeGuid = shootNode.Guid;
                        }
                    }
                    else if (connectedNode is SpecialSkillNodeView specialView)
                    {
                        var specialNode = specialView.GetNode();
                        if (specialNode != null)
                        {
                            branch.NextNodeGuid = specialNode.Guid;
                        }
                    }
                    else if (connectedNode is PhaseNodeView phaseView)
                    {
                        var phaseNode = phaseView.GetNode();
                        if (phaseNode != null)
                        {
                            branch.NextNodeGuid = phaseNode.Guid;
                        }
                    }
                    else if (connectedNode is IfNodeView ifView)
                    {
                        var ifNode = ifView.GetNode();
                        if (ifNode != null)
                        {
                            branch.NextNodeGuid = ifNode.Guid;
                        }
                    }
                }
            }
            else
            {
                branch.NextNodeGuid = "";
            }
        }
        
        if (_machine != null)
            EditorUtility.SetDirty(_machine);
    }

    private void AddBranch()
    {
        _node.Branches.Add(new RandomBranch { Percent = 50f });

        if (_machine != null)
            EditorUtility.SetDirty(_machine);

        SyncPorts();
    }

    private void RemoveBranch(RandomBranch branch)
    {
        _node.Branches.Remove(branch);

        if (_machine != null)
            EditorUtility.SetDirty(_machine);

        SyncPorts();
    }
    
    public RandomNode GetNode()
    {
        return _node;
    }
}
#endif
