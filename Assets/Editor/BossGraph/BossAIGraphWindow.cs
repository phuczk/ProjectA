#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BossAIGraphWindow : EditorWindow
{
    private BossAIGraphView _graphView;
    private BossController _selectedMachine;

    [MenuItem("Tools/Boss AI Graph Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<BossAIGraphWindow>();
        window.titleContent = new GUIContent("Boss AI Graph");
        window.Show();
    }
    
    private void OnEnable()
    {
        _graphView = new BossAIGraphView();
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
        
        CreateToolbar();
    }
    
    private void CreateToolbar()
    {
        var toolbar = new Toolbar();
        
        var addStartButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateStartNode();
        });
        addStartButton.text = "Start";
        toolbar.Add(addStartButton);
        
        var addRandomButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateRandomNode();
        });
        addRandomButton.text = "Random";
        toolbar.Add(addRandomButton);
        
        var addPhaseButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreatePhaseNode();
        });
        addPhaseButton.text = "Phase";
        toolbar.Add(addPhaseButton);
        
        var addIfButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateIfNode();
        });
        addIfButton.text = "If";
        toolbar.Add(addIfButton);
        
        var addMultiplyButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateMultiplyNode();
        });
        addMultiplyButton.text = "Multiply";
        toolbar.Add(addMultiplyButton);
        
        var addAddButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateAddNode();
        });
        addAddButton.text = "Add";
        toolbar.Add(addAddButton);
        
        var addEndButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateEndNode();
        });
        addEndButton.text = "End";
        toolbar.Add(addEndButton);
        
        var addAttackButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateSkillNode(typeof(AttackSkillNode));
        });
        addAttackButton.text = "Attack";
        toolbar.Add(addAttackButton);
        
        var addShootButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateSkillNode(typeof(ShootSkillNode));
        });
        addShootButton.text = "Shoot";
        toolbar.Add(addShootButton);
        
        var addSpecialButton = new Button(() => {
            var graphView = _graphView as BossAIGraphView;
            graphView?.CreateSkillNode(typeof(SpecialSkillNode));
        });
        addSpecialButton.text = "Special";
        toolbar.Add(addSpecialButton);
        
        rootVisualElement.Add(toolbar);
    }

    private void OnSelectionChange()
    {
        var machine = Selection.activeGameObject?.GetComponent<BossController>();
        
        if (_selectedMachine != machine)
        {
            if (machine != null)
            {
                _selectedMachine = machine;
                _graphView.Load(_selectedMachine);
            }
            else
            {
                _selectedMachine = null;
                _graphView.ClearVisualOnly();
            }
        }
    }
}
#endif
