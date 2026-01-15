#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyAIGraphView : GraphView
{
    private EnemyUniversalMachine _machine;
    private Dictionary<EnemyStateNode, StateNodeView> _nodeLookup = new Dictionary<EnemyStateNode, StateNodeView>();

    public EnemyAIGraphView()
    {
        Insert(0, new GridBackground());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        graphViewChanged = OnGraphViewChanged;
    }

    public void Load(EnemyUniversalMachine machine)
    {
        _machine = machine;
        
        graphElements.ForEach(RemoveElement); 
        _nodeLookup.Clear();

        foreach (var node in machine.StateNodes)
        {
            AddNodeView(node);
        }

        foreach (var node in machine.StateNodes)
        {
            if (!_nodeLookup.TryGetValue(node, out var fromView)) continue;

            foreach (var trans in node.Transitions)
            {
                if (string.IsNullOrEmpty(trans.TargetNodeGuid)) continue;

                var toNode = machine.StateNodes.FirstOrDefault(n => n.Guid == trans.TargetNodeGuid);
                if (toNode != null && _nodeLookup.TryGetValue(toNode, out var toView))
                {
                    if (fromView.TransitionPorts.TryGetValue(trans, out var port))
                    {
                        var edge = new TransitionEdge(trans)
                        {
                            output = port,
                            input = toView.Input
                        };
                        edge.output.Connect(edge);
                        edge.input.Connect(edge);
                        AddElement(edge);
                    }
                }
            }
        }
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (_machine == null) return change;

        if (change.elementsToRemove != null)
        {
            foreach (var element in change.elementsToRemove)
            {
                // 1. XỬ LÝ XÓA NODE
                if (element is StateNodeView nodeView)
                {
                    _machine.StateNodes.Remove(nodeView.StateNode);
                    _nodeLookup.Remove(nodeView.StateNode);
                    
                    // (Tùy chọn) Xóa các dây nối đang trỏ tới Node này từ các Node khác
                    foreach(var otherNode in _machine.StateNodes) {
                        otherNode.Transitions.RemoveAll(t => t.TargetNodeGuid == nodeView.StateNode.Guid);
                    }
                }

                // 2. XỬ LÝ XÓA DÂY (Giữ nguyên logic của bạn)
                if (element is Edge edge)
                {
                    var fromView = edge.output?.node as StateNodeView;
                    if (fromView != null)
                    {
                        var entry = fromView.TransitionPorts.FirstOrDefault(x => x.Value == edge.output);
                        if (entry.Key != null) entry.Key.TargetNodeGuid = ""; 
                    }
                }
            }
        }

        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                var fromView = edge.output?.node as StateNodeView;
                var toView = edge.input?.node as StateNodeView;

                if (fromView != null && toView != null)
                {
                    var entry = fromView.TransitionPorts.FirstOrDefault(x => x.Value == edge.output);
                    if (entry.Key != null)
                    {
                        entry.Key.TargetNodeGuid = toView.StateNode.Guid;
                        entry.Key.TargetState = toView.StateNode.StateType;
                        edge.output.portName = $"-> {toView.StateNode.GetType().Name}";
                    }
                }
            }
        }

        EditorUtility.SetDirty(_machine);
        return change;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        var types = TypeCache.GetTypesDerivedFrom<EnemyStateNode>().Where(t => !t.IsAbstract);
        foreach (var type in types)
        {
            evt.menu.AppendAction($"Add State/{type.Name}", (a) => {
                var newNode = (EnemyStateNode)Activator.CreateInstance(type);
                CreateNode(newNode, evt.localMousePosition);
            });
        }
    }

    private void CreateNode(EnemyStateNode node, Vector2 pos)
    {
        node.GraphPosition = pos;
        _machine.StateNodes.Add(node);
        
        // Thay vì Load(_machine), chúng ta chỉ tạo thêm 1 View cho Node mới
        AddNodeView(node);
        
        EditorUtility.SetDirty(_machine);
    }

    // Hàm bổ trợ để tạo NodeView mà không cần reload toàn bộ Graph
    private void AddNodeView(EnemyStateNode node)
    {
        var nodeView = new StateNodeView(node, _machine);
        AddElement(nodeView);
        _nodeLookup[node] = nodeView;
    }
}
#endif
