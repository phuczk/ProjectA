using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using SerializeReferenceEditor;

public class BossController : MonoBehaviour
{
    [Header("AI Configuration")]
    [SerializeReference, SR] public List<BossNode> StateNodes = new List<BossNode>();
    
    [Header("Runtime State")]
    [SerializeField] private BossNode _previousState;
    private BossNode _currentState;
    private bool _isRunning = false;
    private bool _isTransitioning = false;
    private bool _hasTriggeredAutoTransition = false;
    
    public BossNode CurrentState => _currentState;
    public BossNode PreviousState => _previousState;
    public BossContext Context { get; set; }
    public Animator Animator { get; set; }
    
    [Header("Default State")]
    public BossStateType DefaultStateType = BossStateType.Start;
    
    private Dictionary<BossStateType, List<BossNode>> _typeToNodes = new Dictionary<BossStateType, List<BossNode>>();
    
    void Start()
    {
        Animator = GetComponent<Animator>();
        Context = new BossContext {
            boss = gameObject,
            animator = Animator,
            player = GameObject.FindGameObjectWithTag("Player")?.transform,
            hp = 100f
        };
        
        // Initialize all nodes and build type dictionary
        foreach (var node in StateNodes)
        {
            // Initialize all nodes (including BossNode base class)
            node.Initialize(this);
            
            if (!_typeToNodes.ContainsKey(node.StateType))
            {
                _typeToNodes[node.StateType] = new List<BossNode>();
            }
            _typeToNodes[node.StateType].Add(node);
        }
        
        // Set initial state
        TransitionToState(DefaultStateType);
        
        Debug.Log($"BossController initialized with state: {_currentState?.GetType().Name}");
    }
    
    void Update()
    {
        if (_isRunning && _currentState != null)
        {
            // Don't execute if node is already finished
            if (_currentState.IsFinished)
            {
                return;
            }
            
            // Execute current node logic directly
            _currentState.ExecuteLogic();
            
            // If current node is finished, auto-transition to next in list
            if (_currentState.IsFinished && !_hasTriggeredAutoTransition)
            {
                AutoTransitionToNextNode();
                _hasTriggeredAutoTransition = true;
            }
            // If node is not finished, don't Execute again until it finishes
            else
            {
                // Don't Execute again this frame - wait for next frame
                return;
            }
        }
    }
    
    private void AutoTransitionToNextNode()
    {
        if (_isTransitioning) return; // Don't transition if already transitioning
        
        // Only use graph connection (NextNodeGuid) - no fallback to inspector order
        if (_currentState != null && !string.IsNullOrEmpty(_currentState.NextNodeGuid))
        {
            // Find node by Guid
            BossNode nextNode = StateNodes.Find(n => n.Guid == _currentState.NextNodeGuid);
            
            if (nextNode != null)
            {
                // Transition to specific node by Guid, not by StateType
                StartCoroutine(DelayedTransition(nextNode));
                return;
            }
            else
            {
                Debug.LogWarning($"BossController.AutoTransitionToNextNode() - Could not find node with Guid: {_currentState.NextNodeGuid}");
            }
        }
        
        // Special handling for EndNode - restart from StartNode
        if (_currentState is EndNode)
        {
            TransitionToState(BossStateType.Start);
            return;
        }
        
        // No fallback to inspector order - just stop execution
        StopAI();
    }
    
    public void TransitionToState(BossStateType type)
    {
        
        if (!_typeToNodes.ContainsKey(type)) 
        {
            return;
        }

        var potentialNodes = _typeToNodes[type];
        if (potentialNodes.Count == 0) 
        {
            return;
        }

        BossNode selectedNode = null;
        foreach (var node in potentialNodes)
        {
            if (node.CanEnter(this))
            {
                selectedNode = node;
                break;
            }
        }

        selectedNode ??= potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];

        if (selectedNode == _currentState && !_currentState.IsFinished) 
        {
            return;
        }

        // Add delay between transitions
        StartCoroutine(DelayedTransition(selectedNode));
    }
    
    private IEnumerator DelayedTransition(BossNode newNode)
    {
        _isTransitioning = true; // Set transitioning flag
        
        yield return new WaitForSeconds(1f); // 1 second delay
        
        // Exit current state
        _currentState?.Exit();

        // Update state tracking
        _previousState = _currentState;
        _currentState = newNode;

        // Reset and enter new state
        _currentState.ResetFinished();
        _currentState.Enter();
        
        _isTransitioning = false; // Clear transitioning flag
        _hasTriggeredAutoTransition = false; // Reset auto-transition flag
        
    }
    
    public void StartAI()
    {
        _isRunning = true;
        Debug.Log("Boss AI Started");
    }
    
    public void StopAI()
    {
        _isRunning = false;
        Debug.Log("Boss AI Stopped");
    }
    
    private BossStateType GetNextStateType(BossStateType currentStateType)
    {
        // Define state transition flow like EnemyUniversalMachine
        switch (currentStateType)
        {
            case BossStateType.Start:
                // After Start, go to Attack (default skill)
                return BossStateType.Attack;
                
            case BossStateType.Attack:
                // After Attack, go to Shoot
                return BossStateType.Shoot;
                
            case BossStateType.Shoot:
                // After Shoot, go to Special
                return BossStateType.Special;
                
            case BossStateType.Special:
                // After Special, go to End
                return BossStateType.End;
                
            case BossStateType.End:
                // After End, cycle back to Start
                return BossStateType.Start;
                
            case BossStateType.Phase:
                // After Phase, go to Attack
                return BossStateType.Attack;
                
            case BossStateType.If:
            case BossStateType.Random:
                // For logic nodes, default to Attack
                return BossStateType.Attack;
                
            default:
                return BossStateType.Start;
        }
    }
    
    public BossNode GetNode(string guid)
    {
        return StateNodes.Find(n => n.Guid == guid);
    }
    
    // Helper methods for editor
    public string GetCurrentNodeName()
    {
        return _currentState?.GetType().Name ?? "None";
    }
    
    public bool IsRunning => _isRunning;
    
    // For testing
    [ContextMenu("Start AI")]
    public void TestStartAI()
    {
        StartAI();
    }
    
    [ContextMenu("Stop AI")]
    public void TestStopAI()
    {
        StopAI();
    }
}
