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
        
        // Clear existing graph
        _nodeLookup.Clear();
        DeleteElements(graphElements.ToList());
        
        // Load nodes from BossController
        if (_machine != null && _machine.StateNodes.Count > 0)
        {
            foreach (var node in _machine.StateNodes)
            {
                AddNodeView(node);
            }
        }
    }
    
    private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
    {
        // Mark as dirty when graph changes
        if (_machine != null)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_machine);
            #endif
        }
        
        return changes;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // Add Random Node option to context menu
        evt.menu.AppendAction("Add Random Node", (a) => CreateRandomNode());
        evt.menu.AppendAction("Add Phase Node", (a) => CreatePhaseNode());
        evt.menu.AppendAction("Add If Node", (a) => CreateIfNode());
        evt.menu.AppendAction("Add Start Node", (a) => CreateStartNode());
        evt.menu.AppendAction("Add End Node", (a) => CreateEndNode());
        
        // Add Skill Nodes submenu
        evt.menu.AppendSeparator();
        evt.menu.AppendAction("Skill/Attack Skill", (a) => CreateAttackSkillNode());
        evt.menu.AppendAction("Skill/Shoot Skill", (a) => CreateShootSkillNode());
        evt.menu.AppendAction("Skill/Special Skill", (a) => CreateSpecialSkillNode());
    }
    
    public void CreateRandomNode()
    {
        if (_machine == null) return;
        
        var randomNode = new RandomNode();
        var position = new Vector2(100, 100); // Default position
        CreateNode(randomNode, position);
    }
    
    public void CreatePhaseNode()
    {
        if (_machine == null) return;
        
        var phaseNode = new PhaseNode();
        var position = new Vector2(250, 100); // Default position
        CreateNode(phaseNode, position);
    }
    
    public void CreateIfNode()
    {
        if (_machine == null) return;
        
        var ifNode = new IfNode();
        var position = new Vector2(400, 100); // Default position
        CreateNode(ifNode, position);
    }
    
    public void CreateStartNode()
    {
        if (_machine == null) return;
        
        var startNode = new StartNode();
        var position = new Vector2(50, 200); // Default position
        CreateNode(startNode, position);
    }
    
    public void CreateEndNode()
    {
        if (_machine == null) return;
        
        var endNode = new EndNode();
        var position = new Vector2(550, 200); // Default position
        CreateNode(endNode, position);
    }
    
    public void CreateAttackSkillNode()
    {
        if (_machine == null) return;
        
        var attackSkillNode = new AttackSkillNode();
        var position = new Vector2(150, 300); // Default position
        CreateNode(attackSkillNode, position);
    }
    
    public void CreateShootSkillNode()
    {
        if (_machine == null) return;
        
        var shootSkillNode = new ShootSkillNode();
        var position = new Vector2(300, 300); // Default position
        CreateNode(shootSkillNode, position);
    }
    
    public void CreateSpecialSkillNode()
    {
        if (_machine == null) return;
        
        var specialSkillNode = new SpecialSkillNode();
        var position = new Vector2(450, 300); // Default position
        CreateNode(specialSkillNode, position);
    }

    private void CreateNode(BossNode node, Vector2 pos)
    {
        node.GraphPosition = pos;
        
        // Add node to BossController
        if (_machine != null)
        {
            _machine.StateNodes.Add(node);
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_machine);
            #endif
        }
        
        // Thay vì Load(_machine), chúng ta chỉ tạo thêm 1 View cho Node mới
        AddNodeView(node);
    }

    // Hàm bổ trợ để tạo NodeView mà không cần reload toàn bộ Graph
    private void AddNodeView(BossNode node)
    {
        if (node is RandomNode randomNode)
        {
            var nodeView = new RandomNodeView(randomNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
        else if (node is PhaseNode phaseNode)
        {
            var nodeView = new PhaseNodeView(phaseNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
        else if (node is IfNode ifNode)
        {
            var nodeView = new IfNodeView(ifNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
        else if (node is StartNode startNode)
        {
            var nodeView = new StartNodeView(startNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
        else if (node is EndNode endNode)
        {
            var nodeView = new EndNodeView(endNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
        else if (node is AttackSkillNode attackSkillNode)
        {
            var nodeView = new AttackSkillNodeView(attackSkillNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
        else if (node is ShootSkillNode shootSkillNode)
        {
            var nodeView = new ShootSkillNodeView(shootSkillNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
        else if (node is SpecialSkillNode specialSkillNode)
        {
            var nodeView = new SpecialSkillNodeView(specialSkillNode, _machine);
            AddElement(nodeView);
            _nodeLookup[node] = nodeView;
        }
    }
}
#endif
