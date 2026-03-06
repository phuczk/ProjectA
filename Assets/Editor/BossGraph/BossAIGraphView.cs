#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BossAIGraphView : GraphView
{
    private BossController _machine;
    private Dictionary<BossNode, Node> _nodeLookup = new Dictionary<BossNode, Node>();

    public BossAIGraphView()
    {
        Insert(0, new GridBackground());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        graphViewChanged = OnGraphViewChanged;
    }
    
    public void Load(BossController machine)
    {
        _machine = machine;
        
        var elementsToRemove = graphElements.ToList();
        foreach (var element in elementsToRemove)
        {
            RemoveElement(element);
        }
        _nodeLookup.Clear();

        if (_machine != null && _machine.StateNodes.Count > 0)
        {
            foreach (var node in _machine.StateNodes)
            {
                AddNodeView(node);
            }
            
            CreateConnectionsFromGuids();
        }
    }
    
    public void ClearGraph()
    {
        _machine = null;
        _nodeLookup.Clear();
        DeleteElements(graphElements.ToList());
    }
    
    public void ClearVisualOnly()
    {
        _machine = null;
        _nodeLookup.Clear();
        
        var elementsToRemove = graphElements.ToList();
        foreach (var element in elementsToRemove)
        {
            RemoveElement(element);
        }
    }
    
    private void CreateConnectionsFromGuids()
    {
        foreach (var kvp in _nodeLookup)
        {
            var node = kvp.Key;
            var nodeView = kvp.Value;
            
            if (node is IfNode ifNode)
            {
                for (int i = 0; i < ifNode.Conditions.Count; i++)
                {
                    var branch = ifNode.Conditions[i];
                    if (!string.IsNullOrEmpty(branch.NextNodeGuid))
                    {
                        Node targetNodeView = null;
                        foreach (var targetKvp in _nodeLookup)
                        {
                            if (targetKvp.Key.Guid == branch.NextNodeGuid)
                            {
                                targetNodeView = targetKvp.Value;
                                break;
                            }
                        }
                        
                        if (targetNodeView != null && nodeView is IfNodeView ifView)
                        {
                            Port conditionPort = null;
                            int portIndex = 0;
                            foreach (var portKvp in ifView.ConditionPorts)
                            {
                                if (portIndex == i)
                                {
                                    conditionPort = portKvp.Value;
                                    break;
                                }
                                portIndex++;
                            }
                            
                            if (conditionPort != null)
                            {
                                Port inputPort = GetInputPort(targetNodeView);
                                
                                if (inputPort != null)
                                {
                                    var edge = new Edge
                                    {
                                        output = conditionPort,
                                        input = inputPort
                                    };
                                    
                                    conditionPort.Connect(edge);
                                    inputPort.Connect(edge);
                                    AddElement(edge);
                                }
                            }
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(ifNode.ElseNodeGuid))
                {
                    Node targetNodeView = null;
                    foreach (var targetKvp in _nodeLookup)
                    {
                        if (targetKvp.Key.Guid == ifNode.ElseNodeGuid)
                        {
                            targetNodeView = targetKvp.Value;
                            break;
                        }
                    }
                    
                    if (targetNodeView != null && nodeView is IfNodeView ifView)
                    {
                        Port inputPort = GetInputPort(targetNodeView);
                        
                        if (inputPort != null)
                        {
                            var edge = new Edge
                            {
                                output = ifView.ElsePort,
                                input = inputPort
                            };
                            
                            ifView.ElsePort.Connect(edge);
                            inputPort.Connect(edge);
                            AddElement(edge);
                        }
                    }
                }
            }
            else if (node is RandomNode randomNode)
            {
                for (int i = 0; i < randomNode.Branches.Count; i++)
                {
                    var branch = randomNode.Branches[i];
                    if (!string.IsNullOrEmpty(branch.NextNodeGuid))
                    {
                        Node targetNodeView = null;
                        foreach (var targetKvp in _nodeLookup)
                        {
                            if (targetKvp.Key.Guid == branch.NextNodeGuid)
                            {
                                targetNodeView = targetKvp.Value;
                                break;
                            }
                        }
                        
                        if (targetNodeView != null && nodeView is RandomNodeView randomView)
                        {
                            Port branchPort = null;
                            int portIndex = 0;
                            foreach (var portKvp in randomView.BranchPorts)
                            {
                                if (portIndex == i)
                                {
                                    branchPort = portKvp.Value;
                                    break;
                                }
                                portIndex++;
                            }
                            
                            if (branchPort != null)
                            {
                                Port inputPort = GetInputPort(targetNodeView);
                                
                                if (inputPort != null)
                                {
                                    var edge = new Edge
                                    {
                                        output = branchPort,
                                        input = inputPort
                                    };
                                    
                                    branchPort.Connect(edge);
                                    inputPort.Connect(edge);
                                    AddElement(edge);
                                }
                            }
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(node.NextNodeGuid))
            {
                Node targetNodeView = null;
                foreach (var targetKvp in _nodeLookup)
                {
                    if (targetKvp.Key.Guid == node.NextNodeGuid)
                    {
                        targetNodeView = targetKvp.Value;
                        break;
                    }
                }
                
                if (targetNodeView != null)
                {
                    if (targetNodeView is IfNodeView ifTargetView)
                    {
                        Port outputPort = GetOutputPort(nodeView);
                        Port inputPort = ifTargetView.Input;
                        
                        if (outputPort != null && inputPort != null)
                        {
                            var edge = new Edge
                            {
                                output = outputPort,
                                input = inputPort
                            };
                            
                            outputPort.Connect(edge);
                            inputPort.Connect(edge);
                            AddElement(edge);
                        }
                    }
                    else if (targetNodeView is RandomNodeView randomTargetView)
                    {
                        Port outputPort = GetOutputPort(nodeView);
                        Port inputPort = randomTargetView.Input;
                        
                        if (outputPort != null && inputPort != null)
                        {
                            var edge = new Edge
                            {
                                output = outputPort,
                                input = inputPort
                            };
                            
                            outputPort.Connect(edge);
                            inputPort.Connect(edge);
                            AddElement(edge);
                        }
                    }
                    else
                    {
                        Port outputPort = GetOutputPort(nodeView);
                        Port inputPort = GetInputPort(targetNodeView);
                        
                        if (outputPort != null && inputPort != null)
                        {
                            var edge = new Edge
                            {
                                output = outputPort,
                                input = inputPort
                            };
                            
                            outputPort.Connect(edge);
                            inputPort.Connect(edge);
                            AddElement(edge);
                        }
                        else
                        {
                            Debug.LogWarning($"Could not create edge - missing ports. Output: {outputPort != null}, Input: {inputPort != null}. Source: {node.GetType().Name} -> Target: {targetNodeView.GetType().Name}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not find target node with Guid {node.NextNodeGuid}");
                }
            }
        }
    }
    
    private Port GetOutputPort(Node nodeView)
    {
        if (nodeView is SkillNodeView skillView)
            return skillView.Output;
        else if (nodeView is StartNodeView startView)
            return startView.Output;
        else if (nodeView is PhaseNodeView phaseView)
            return phaseView.Output;
        else if (nodeView is EndNodeView endView)
            return null;
        
        Debug.LogWarning($"GetOutputPort: Unsupported node type {nodeView.GetType().Name}");
        return null;
    }
    
    private Port GetInputPort(Node nodeView)
    {
        if (nodeView is SkillNodeView skillView)
            return skillView.Input;
        else if (nodeView is PhaseNodeView phaseView)
            return phaseView.Input;
        else if (nodeView is IfNodeView ifView)
            return ifView.Input;
        else if (nodeView is RandomNodeView randomView)
            return randomView.Input;
        else if (nodeView is EndNodeView endView)
            return endView.Input;
        else if (nodeView is StartNodeView startView)
            return null;
        
        Debug.LogWarning($"GetInputPort: Unsupported node type {nodeView.GetType().Name}");
        return null;
    }
    
    private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
    {
        if (_machine == null) return changes;
        
        if (changes.edgesToCreate != null)
        {
            foreach (var edge in changes.edgesToCreate)
            {
                var outputNode = edge.output.node;
                var inputNode = edge.input.node;
                
                BossNode outputBossNode = null;
                BossNode inputBossNode = null;
                
                if (outputNode is SkillNodeView skillOutput)
                    outputBossNode = skillOutput.GetNode();
                else if (outputNode is StartNodeView startOutput)
                    outputBossNode = startOutput.GetNode();
                else if (outputNode is EndNodeView endOutput)
                    outputBossNode = endOutput.GetNode();
                else if (outputNode is PhaseNodeView phaseOutput)
                    outputBossNode = phaseOutput.GetNode();
                else if (outputNode is IfNodeView ifOutput)
                    outputBossNode = ifOutput.GetNode();
                else if (outputNode is RandomNodeView randomOutput)
                    outputBossNode = randomOutput.GetNode();
                    
                if (inputNode is SkillNodeView skillInput)
                    inputBossNode = skillInput.GetNode();
                else if (inputNode is StartNodeView startInput)
                    inputBossNode = startInput.GetNode();
                else if (inputNode is EndNodeView endInput)
                    inputBossNode = endInput.GetNode();
                else if (inputNode is PhaseNodeView phaseInput)
                    inputBossNode = phaseInput.GetNode();
                else if (inputNode is IfNodeView ifInput)
                    inputBossNode = ifInput.GetNode();
                else if (inputNode is RandomNodeView randomInput)
                    inputBossNode = randomInput.GetNode();
                
                if (outputBossNode != null && inputBossNode != null)
                {
                    if (outputBossNode is IfNode ifNode)
                    {
                        var outputPortName = edge.output.portName;
                        
                        if (outputPortName == "Else")
                        {
                            ifNode.ElseNodeGuid = inputBossNode.Guid;
                        }
                        else
                        {
                            var ifView = outputNode as IfNodeView;
                            var conditionIndex = GetConditionPortIndex(edge.output as Port, ifView);
                            if (conditionIndex >= 0 && conditionIndex < ifNode.Conditions.Count)
                            {
                                ifNode.Conditions[conditionIndex].NextNodeGuid = inputBossNode.Guid;
                            }
                        }
                    }
                    else if (outputBossNode is RandomNode randomNode && outputNode is RandomNodeView randomView)
                    {
                        var outputPort = edge.output as Port;
                        var branchIndex = GetRandomBranchPortIndex(outputPort, randomView);
                        
                        if (branchIndex >= 0 && branchIndex < randomNode.Branches.Count)
                        {
                            randomNode.Branches[branchIndex].NextNodeGuid = inputBossNode.Guid;
                        }
                    }
                    else
                    {
                        outputBossNode.NextNodeGuid = inputBossNode.Guid;
                    }
                    
                    if (_machine != null)
                    {
                        EditorUtility.SetDirty(_machine);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not get BossNode from edge. Output: {outputBossNode != null}, Input: {inputBossNode != null}");
                }
            }
        }
        
        if (changes.elementsToRemove != null)
        {
            foreach (var element in changes.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    var outputNode = edge.output.node;
                    BossNode outputBossNode = null;
                    
                    if (outputNode is SkillNodeView skillOutput)
                        outputBossNode = skillOutput.GetNode();
                    else if (outputNode is StartNodeView startOutput)
                        outputBossNode = startOutput.GetNode();
                    else if (outputNode is EndNodeView endOutput)
                        outputBossNode = endOutput.GetNode();
                    else if (outputNode is PhaseNodeView phaseOutput)
                        outputBossNode = phaseOutput.GetNode();
                    else if (outputNode is IfNodeView ifOutput)
                        outputBossNode = ifOutput.GetNode();
                    
                    if (outputBossNode != null)
                    {
                        outputBossNode.NextNodeGuid = "";
                        if (_machine != null)
                        {
                            EditorUtility.SetDirty(_machine);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
        }
        
        if (_machine != null)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_machine);
            #endif
        }
        
        return changes;
    }

    private int GetConditionPortIndex(Port port, IfNodeView ifView)
    {
        if (port == null || ifView == null) return -1;
        
        int index = 0;
        foreach (var kvp in ifView.ConditionPorts)
        {
            if (kvp.Value == port)
            {
                return index;
            }
            index++;
        }
        
        return -1;
    }
    
    private int GetRandomBranchPortIndex(Port port, RandomNodeView randomView)
    {
        if (port == null || randomView == null) return -1;
        
        int index = 0;
        foreach (var kvp in randomView.BranchPorts)
        {
            if (kvp.Value == port)
            {
                return index;
            }
            index++;
        }
        
        return -1;
    }
    
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
    }
    
    private void RegisterNodePositionCallbacks(Node nodeView)
    {
        nodeView.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            BossNode bossNode = null;
            
            if (nodeView is SkillNodeView skillView)
                bossNode = skillView.GetNode();
            else if (nodeView is StartNodeView startView)
                bossNode = startView.GetNode();
            else if (nodeView is EndNodeView endView)
                bossNode = endView.GetNode();
            else if (nodeView is PhaseNodeView phaseView)
                bossNode = phaseView.GetNode();
            else if (nodeView is IfNodeView ifView)
                bossNode = ifView.GetNode();
            else if (nodeView is RandomNodeView randomView)
                bossNode = randomView.GetNode();
                
            if (bossNode != null)
            {
                bossNode.GraphPosition = nodeView.GetPosition().position;
                EditorUtility.SetDirty(_machine);
            }
        });
    }
    
    public void OnEdgeConnected(Edge edge)
    {
        var outputNode = edge.output.node;
        var inputNode = edge.input.node;
        
        BossNode outputBossNode = null;
        BossNode inputBossNode = null;
        
        if (outputNode is SkillNodeView skillOutput)
            outputBossNode = skillOutput.GetNode();
        else if (outputNode is StartNodeView startOutput)
            outputBossNode = startOutput.GetNode();
        else if (outputNode is EndNodeView endOutput)
            outputBossNode = endOutput.GetNode();
        else if (outputNode is PhaseNodeView phaseOutput)
            outputBossNode = phaseOutput.GetNode();
        else if (outputNode is IfNodeView ifOutput)
            outputBossNode = ifOutput.GetNode();
        else if (outputNode is RandomNodeView randomOutput)
            outputBossNode = randomOutput.GetNode();
            
        if (inputNode is SkillNodeView skillInput)
            inputBossNode = skillInput.GetNode();
        else if (inputNode is StartNodeView startInput)
            inputBossNode = startInput.GetNode();
        else if (inputNode is EndNodeView endInput)
            inputBossNode = endInput.GetNode();
        else if (inputNode is PhaseNodeView phaseInput)
            inputBossNode = phaseInput.GetNode();
        else if (inputNode is IfNodeView ifInput)
            inputBossNode = ifInput.GetNode();
        else if (inputNode is RandomNodeView randomInput)
            inputBossNode = randomInput.GetNode();
        
        if (outputBossNode != null && inputBossNode != null)
        {
            outputBossNode.NextNodeGuid = inputBossNode.Guid;
            if (_machine != null)
            {
                EditorUtility.SetDirty(_machine);
                AssetDatabase.SaveAssets();
            }
        }
    }
    
    public void OnEdgeDisconnected(Edge edge)
    {
        var outputNode = edge.output.node;
        BossNode outputBossNode = null;
        
        if (outputNode is SkillNodeView skillOutput)
            outputBossNode = skillOutput.GetNode();
        else if (outputNode is StartNodeView startOutput)
            outputBossNode = startOutput.GetNode();
        else if (outputNode is EndNodeView endOutput)
            outputBossNode = endOutput.GetNode();
        else if (outputNode is PhaseNodeView phaseOutput)
            outputBossNode = phaseOutput.GetNode();
        else if (outputNode is IfNodeView ifOutput)
            outputBossNode = ifOutput.GetNode();
        else if (outputNode is RandomNodeView randomOutput)
            outputBossNode = randomOutput.GetNode();
        
        if (outputBossNode != null)
        {
            outputBossNode.NextNodeGuid = "";
            if (_machine != null)
            {
                EditorUtility.SetDirty(_machine);
                AssetDatabase.SaveAssets();
            }
        }
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Add Random Node", (a) => CreateRandomNode());
        evt.menu.AppendAction("Add Phase Node", (a) => CreatePhaseNode());
        evt.menu.AppendAction("Add If Node", (a) => CreateIfNode());
        evt.menu.AppendAction("Add Start Node", (a) => CreateStartNode());
        evt.menu.AppendAction("Add End Node", (a) => CreateEndNode());
        
        evt.menu.AppendSeparator();
        var skillTypes = GetAllSkillNodeTypes();
        
        if (skillTypes.Count > 0)
        {
            foreach (var skillType in skillTypes)
            {
                string displayName = skillType.Name.Replace("Node", "");
                evt.menu.AppendAction($"Skill/{displayName}", (a) => CreateSkillNode(skillType));
            }
        }
        else
        {
            evt.menu.AppendAction("Skill/No Skill Nodes Found", null, DropdownMenuAction.Status.Disabled);
        }
    }
    
    private System.Collections.Generic.List<System.Type> GetAllSkillNodeTypes()
    {
        var skillTypes = new System.Collections.Generic.List<System.Type>();
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(SkillNode)))
                    {
                        skillTypes.Add(type);
                    }
                }
            }
            catch (System.Exception)
            {
                continue;
            }
        }
        
        return skillTypes;
    }
    
    public void CreateSkillNode(System.Type skillType)
    {
        if (_machine == null) return;
        
        var skillNode = (SkillNode)System.Activator.CreateInstance(skillType);
        var position = new Vector2(100, 300);
        CreateNode(skillNode, position);
    }
    
    public void CreateRandomNode()
    {
        if (_machine == null) return;
        
        var randomNode = new RandomNode();
        var position = new Vector2(100, 100);
        CreateNode(randomNode, position);
    }
    
    public void CreatePhaseNode()
    {
        if (_machine == null) return;
        
        var phaseNode = new PhaseNode();
        var position = new Vector2(250, 100);
        CreateNode(phaseNode, position);
    }
    
    public void CreateIfNode()
    {
        if (_machine == null) return;
        
        var ifNode = new IfNode();
        var position = new Vector2(400, 100);
        CreateNode(ifNode, position);
    }
    
    public void CreateStartNode()
    {
        if (_machine == null) return;
        
        var startNode = new StartNode();
        var position = new Vector2(50, 200);
        CreateNode(startNode, position);
    }
    
    public void CreateEndNode()
    {
        if (_machine == null) return;
        
        var endNode = new EndNode();
        var position = new Vector2(550, 200);
        CreateNode(endNode, position);
    }

    private void CreateNode(BossNode node, Vector2 pos)
    {
        node.GraphPosition = pos;
        
        if (_machine != null)
        {
            _machine.StateNodes.Add(node);
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_machine);
            #endif
        }
        
        AddNodeView(node);
    }

    private void AddNodeView(BossNode node)
    {
        Node nodeView = null;
        
        if (node is RandomNode randomNode)
        {
            nodeView = new RandomNodeView(randomNode, _machine);
        }
        else if (node is PhaseNode phaseNode)
        {
            nodeView = new PhaseNodeView(phaseNode, _machine);
        }
        else if (node is IfNode ifNode)
        {
            nodeView = new IfNodeView(ifNode, _machine);
        }
        else if (node is StartNode startNode)
        {
            nodeView = new StartNodeView(startNode, _machine);
        }
        else if (node is EndNode endNode)
        {
            nodeView = new EndNodeView(endNode, _machine);
        }
        else if (node is SkillNode skillNode)
        {
            nodeView = new SkillNodeView(skillNode, _machine);
        }
        
        if (nodeView != null)
        {
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
            
            RegisterNodePositionCallbacks(nodeView);
        }
    }
}
#endif
