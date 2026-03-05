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
    [SerializeField] private BossNode _currentState;
    [SerializeField] private BossNode _previousState;
    [SerializeField] private bool _isRunning = false;
    
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
            // Execute current node and get next node
            BossNode nextNode = _currentState.Execute(this);
            
            // If node returned a next node, transition to it
            if (nextNode != null)
            {
                TransitionToState(nextNode.StateType);
            }
            // If current node is finished, auto-transition to next in list
            else if (_currentState.IsFinished)
            {
                AutoTransitionToNextNode();
            }
        }
    }
    
    private void AutoTransitionToNextNode()
    {
        int currentIndex = StateNodes.IndexOf(_currentState);
        if (currentIndex >= 0 && currentIndex < StateNodes.Count - 1)
        {
            // Go to next node in list
            BossNode nextNode = StateNodes[currentIndex + 1];
            TransitionToState(nextNode.StateType);
        }
        else if (currentIndex == StateNodes.Count - 1)
        {
            // Last node, cycle back to first
            BossNode firstNode = StateNodes[0];
            TransitionToState(firstNode.StateType);
        }
    }
    
    public void TransitionToState(BossStateType type)
    {
        Debug.Log($"BossController.TransitionToState() - Requested transition to: {type}");
        
        if (!_typeToNodes.ContainsKey(type)) 
        {
            Debug.LogError($"BossController.TransitionToState() - No nodes found for state type: {type}");
            return;
        }

        var potentialNodes = _typeToNodes[type];
        if (potentialNodes.Count == 0) 
        {
            Debug.LogError($"BossController.TransitionToState() - Empty node list for state type: {type}");
            return;
        }

        BossNode selectedNode = null;
        foreach (var node in potentialNodes)
        {
            if (node.CanEnter(this))
            {
                selectedNode = node;
                Debug.Log($"BossController.TransitionToState() - Selected node: {node.GetType().Name}");
                break;
            }
        }

        selectedNode ??= potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];

        if (selectedNode == _currentState && !_currentState.IsFinished) 
        {
            Debug.Log($"BossController.TransitionToState() - Already in state and not finished, skipping");
            return;
        }

        Debug.Log($"BossController.TransitionToState() - Transitioning from {_currentState?.GetType().Name} to {selectedNode.GetType().Name}");
        
        // Add delay between transitions
        StartCoroutine(DelayedTransition(selectedNode));
    }
    
    private IEnumerator DelayedTransition(BossNode newNode)
    {
        Debug.Log($"BossController.DelayedTransition() - Waiting 1 second before transitioning to {newNode.GetType().Name}");
        
        yield return new WaitForSeconds(1f); // 1 second delay
        
        // Exit current state
        _currentState?.Exit();

        // Update state tracking
        _previousState = _currentState;
        _currentState = newNode;

        // Reset and enter new state
        _currentState.ResetFinished();
        _currentState.Enter();
        
        Debug.Log($"BossController.DelayedTransition() - Transitioned to {_currentState.GetType().Name}");
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
