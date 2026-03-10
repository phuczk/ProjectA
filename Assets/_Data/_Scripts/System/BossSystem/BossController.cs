// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;
// using System.Collections;
// using SerializeReferenceEditor;
// using System;

// public class BossController : MonoBehaviour
// {
//     [Header("AI Configuration")]
//     [SerializeReference, SR] public List<BossNode> StateNodes = new List<BossNode>();
    
//     [Header("Runtime State")]
//     [SerializeField] private BossNode _previousState;
//     private BossNode _currentState;
//     private bool _isRunning = false;
//     private bool _isTransitioning = false;
//     private bool _hasTriggeredAutoTransition = false;
    
//     [Header("Phase Management")]
//     [SerializeField] private int _currentPhase = 0;
//     public int CurrentPhase => _currentPhase;
    
//     [Header("Phase Tracking")]
//     [SerializeField] private HashSet<string> _executedPhaseNodes = new HashSet<string>();
    
//     public bool IsPhaseNodeExecuted(string phaseNodeGuid)
//     {
//         return _executedPhaseNodes.Contains(phaseNodeGuid);
//     }
    
//     public void MarkPhaseNodeExecuted(string phaseNodeGuid)
//     {
//         _executedPhaseNodes.Add(phaseNodeGuid);
//     }
    
//     public void SetCurrentPhase(int phase)
//     {
//         _currentPhase = phase;
//     }
    
//     public BossNode CurrentState => _currentState;
//     public BossNode PreviousState => _previousState;
//     public BossContext Context { get; set; }
//     public Animator Animator { get; set; }
//     public BossAnimationController AnimationController => _animationController;
//     public BossAnimationController _animationController;
    
//     [Header("Default State")]
//     public BossStateType DefaultStateType = BossStateType.Start;
    
//     private Dictionary<BossStateType, List<BossNode>> _typeToNodes = new Dictionary<BossStateType, List<BossNode>>();
    
//     void Start()
//     {
//         Animator = GetComponent<Animator>();
//         Context = new BossContext {
//             boss = gameObject,
//             animator = Animator,
//             player = GameObject.FindGameObjectWithTag("Player")?.transform,
//             hp = 100f
//         };
        
//         foreach (var node in StateNodes)
//         {
//             node.Initialize(this);
            
//             if (!_typeToNodes.ContainsKey(node.StateType))
//             {
//                 _typeToNodes[node.StateType] = new List<BossNode>();
//             }
//             _typeToNodes[node.StateType].Add(node);
//         }
        
//         TransitionToState(DefaultStateType);
//     }
    
//     void Update()
//     {
//         if (_isRunning && _currentState != null)
//         {
//             if (_currentState.IsFinished)
//             {
//                 if (!_hasTriggeredAutoTransition)
//                 {
//                     AutoTransitionToNextNode();
//                     _hasTriggeredAutoTransition = true;
//                 }
//                 return;
//             }
            
//             if (_currentState is AddNode addNode)
//             {
//                 ExecuteAddNodeSync(addNode);
//                 return;
//             }
            
//             _currentState.ExecuteLogic();
            
//             if (_currentState is MultiplyNode multiplyNode)
//             {
//                 ExecuteMultiplyNodeParallel(multiplyNode);
//                 return;
//             }
            
//             if (_currentState.IsFinished && !_hasTriggeredAutoTransition)
//             {
//                 AutoTransitionToNextNode();
//                 _hasTriggeredAutoTransition = true;
//             }
//             else
//             {
//                 return;
//             }
//         }
//     }
    
//     private void ExecuteMultiplyNodeParallel(MultiplyNode multiplyNode)
//     {
//         AddNode coordinatorAddNode = null;
        
//         foreach (var potentialAddNode in StateNodes.OfType<AddNode>())
//         {
//             bool hasParallelInputs = false;
//             foreach (var branch in potentialAddNode.InputBranches)
//             {
//                 foreach (var multiplyBranch in multiplyNode.Branches)
//                 {
//                     if (branch.NextNodeGuid == multiplyBranch.NextNodeGuid)
//                     {
//                         hasParallelInputs = true;
//                         break;
//                     }
//                 }
                
//                 if (hasParallelInputs) break;
//             }
            
//             if (hasParallelInputs)
//             {
//                 coordinatorAddNode = potentialAddNode;
//                 break;
//             }
//         }
        
//         if (coordinatorAddNode != null)
//         {
//             coordinatorAddNode.Enter();
//         }
        
//         List<BossNode> parallelNodes = new List<BossNode>();
//         foreach (var branch in multiplyNode.Branches)
//         {
//             if (!string.IsNullOrEmpty(branch.NextNodeGuid))
//             {
//                 BossNode targetNode = StateNodes.Find(n => n.Guid == branch.NextNodeGuid);
//                 if (targetNode != null)
//                 {
//                     parallelNodes.Add(targetNode);
//                     StartCoroutine(ExecuteNodeParallel(targetNode));
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"Could not find target node with GUID: {branch.NextNodeGuid}");
//                 }
//             }
//         }
        
//         multiplyNode.Exit();
        
//         if (coordinatorAddNode != null)
//         {
//             return;
//         }
//         else
//         {
//             Debug.LogWarning("No AddNode found that receives parallel inputs - coordination unavailable");
//         }
//     }
    
//     private IEnumerator ExecuteNodeParallel(BossNode node)
//     {
//         float delay = Mathf.Max(node.Delay, 0.1f);
        
//         yield return new WaitForSeconds(delay);
        
//         node.Enter();
        
//         node.ExecuteLogic();
        
//         while (!node.IsFinished)
//         {
//             yield return null;
//         }
        
//         node.Exit();
        
//         NotifyAddNodeIfWaiting(node);
//     }
    
//     private void NotifyAddNodeIfWaiting(BossNode completedNode)
//     {
//         foreach (var potentialAddNode in StateNodes.OfType<AddNode>())
//         {
//             for (int i = 0; i < potentialAddNode.InputBranches.Count; i++)
//             {
//                 var branch = potentialAddNode.InputBranches[i];
                
//                 if (branch.NextNodeGuid == completedNode.Guid)
//                 {
//                     if (i < potentialAddNode.InputCompleted.Count)
//                     {
//                         potentialAddNode.InputCompleted[i] = true;
//                     }
//                     else
//                     {
//                         Debug.LogWarning($"AddNode InputCompleted index {i} out of range. Count: {potentialAddNode.InputCompleted.Count}");
//                     }
//                 }
//             }
//         }
//     }
    
//     private void ExecuteAddNodeSync(AddNode addNode)
//     {
//         bool allInputsCompleted = true;
//         for (int i = 0; i < addNode.InputBranches.Count; i++)
//         {
//             if (i < addNode.InputCompleted.Count)
//             {
//                 if (!addNode.InputCompleted[i])
//                 {
//                     allInputsCompleted = false;
//                     break;
//                 }
//             }
//             else
//             {
//                 allInputsCompleted = false;
//                 break;
//             }
//         }
        
//         if (allInputsCompleted)
//         {
//             addNode.ExecuteLogic();
//         }
//     }
    
//     private void AutoTransitionToNextNode()
//     {
//         if (_isTransitioning) return;
        
//         if (_currentState != null && !string.IsNullOrEmpty(_currentState.NextNodeGuid))
//         {
//             BossNode nextNode = StateNodes.Find(n => n.Guid == _currentState.NextNodeGuid);
            
//             if (nextNode != null)
//             {
//                 StartCoroutine(DelayedTransition(nextNode));
//                 return;
//             }
//         }
        
//         if (_currentState is EndNode)
//         {
//             TransitionToState(BossStateType.Start);
//             return;
//         }
        
//         StopAI();
//     }
    
//     public void TransitionToState(BossStateType type)
//     {
        
//         if (!_typeToNodes.ContainsKey(type)) 
//         {
//             return;
//         }

//         var potentialNodes = _typeToNodes[type];
//         if (potentialNodes.Count == 0) 
//         {
//             return;
//         }

//         BossNode selectedNode = null;
//         foreach (var node in potentialNodes)
//         {
//             if (node.CanEnter(this))
//             {
//                 selectedNode = node;
//                 break;
//             }
//         }

//         selectedNode ??= potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];

//         if (selectedNode == _currentState && !_currentState.IsFinished) 
//         {
//             return;
//         }

//         StartCoroutine(DelayedTransition(selectedNode));
//     }
    
//     private IEnumerator DelayedTransition(BossNode newNode)
//     {
//         _isTransitioning = true;
        
//         float delay = Mathf.Max(newNode.Delay, 0.1f);
        
//         yield return new WaitForSeconds(delay);
        
//         _currentState?.Exit();

//         _previousState = _currentState;
//         _currentState = newNode;

//         _currentState.ResetFinished();
//         _currentState.Enter();
        
//         _isTransitioning = false;
//         _hasTriggeredAutoTransition = false;
        
//     }
    
//     public void StartAI()
//     {
//         _isRunning = true;
//         Debug.Log("Boss AI Started");
//     }
    
//     public void StopAI()
//     {
//         _isRunning = false;
//         Debug.Log("Boss AI Stopped");
//     }
    
//     private BossStateType GetNextStateType(BossStateType currentStateType)
//     {
//         switch (currentStateType)
//         {
//             case BossStateType.Start:
//                 return BossStateType.Attack;
                
//             case BossStateType.Attack:
//                 return BossStateType.Shoot;
                
//             case BossStateType.Shoot:
//                 return BossStateType.Special;
                
//             case BossStateType.Special:
//                 return BossStateType.End;
                
//             case BossStateType.End:
//                 return BossStateType.Start;
                
//             case BossStateType.Phase:
//                 return BossStateType.Attack;
                
//             case BossStateType.If:
//             case BossStateType.Random:
//             case BossStateType.Multiply:
//             case BossStateType.Add:
//                 return BossStateType.Attack;
                
//             default:
//                 return BossStateType.Start;
//         }
//     }
    
//     public BossNode GetNode(string guid)
//     {
//         return StateNodes.Find(n => n.Guid == guid);
//     }
    
//     public string GetCurrentNodeName()
//     {
//         return _currentState?.GetType().Name ?? "None";
//     }
    
//     public bool IsRunning => _isRunning;
    
//     [ContextMenu("Start AI")]
//     public void TestStartAI()
//     {
//         StartAI();
//     }
    
//     [ContextMenu("Stop AI")]
//     public void TestStopAI()
//     {
//         StopAI();
//     }
// }

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class BossController : MonoBehaviour
{
    [Header("AI Configuration")]
    [SerializeReference] public List<BossNode> StateNodes = new List<BossNode>();
    
    [Header("Runtime State")]
    [SerializeField] private BossNode _previousState;
    private BossNode _currentState;
    private bool _isRunning = false;
    private bool _isTransitioning = false;
    private bool _hasTriggeredAutoTransition = false;
    
    [Header("Phase Management")]
    [SerializeField] private int _currentPhase = 0;
    private readonly HashSet<string> _executedPhaseNodes = new HashSet<string>();

    private readonly Dictionary<string, BossNode> _nodeCache = new Dictionary<string, BossNode>();
    private readonly Dictionary<BossStateType, List<BossNode>> _typeToNodes = new Dictionary<BossStateType, List<BossNode>>();

    #region Properties
    public int CurrentPhase => _currentPhase;
    public BossNode CurrentState => _currentState;
    public BossNode PreviousState => _previousState;
    public BossContext Context { get; private set; }
    public Animator Animator { get; private set; }
    public BossAnimationController AnimationController => _animationController;
    public BossAnimationController _animationController;
    public bool IsRunning => _isRunning;
    #endregion

    [Header("Default State")]
    public BossStateType DefaultStateType = BossStateType.Start;

    void Awake() 
    {
        Animator = GetComponent<Animator>();
        Context = new BossContext {
            boss = gameObject,
            animator = Animator,
            player = GameObject.FindGameObjectWithTag("Player")?.transform,
            hp = 100f
        };

        foreach (var node in StateNodes)
        {
            node.Initialize(this);
            _nodeCache[node.Guid] = node;

            if (!_typeToNodes.TryGetValue(node.StateType, out var list))
            {
                list = new List<BossNode>();
                _typeToNodes[node.StateType] = list;
            }
            list.Add(node);
        }
    }

    void Start()
    {
        TransitionToState(DefaultStateType);
    }

    void Update()
    {
        if (!_isRunning || _currentState == null || _isTransitioning) return;

        if (_currentState.IsFinished)
        {
            if (!_hasTriggeredAutoTransition)
            {
                _hasTriggeredAutoTransition = true;
                AutoTransitionToNextNode();
            }
            return;
        }

        switch (_currentState)
        {
            case AddNode addNode:
                ExecuteAddNodeSync(addNode);
                break;
            default:
                _currentState.ExecuteLogic();
                // Check lại sau khi execute
                if (_currentState is MultiplyNode multiplyNode) 
                    ExecuteMultiplyNodeParallel(multiplyNode);
                break;
        }
    }

    private void ExecuteMultiplyNodeParallel(MultiplyNode multiplyNode)
    {
        AddNode coordinator = FindCoordinatorFor(multiplyNode);

        coordinator?.Enter();

        foreach (var branch in multiplyNode.Branches)
        {
            if (_nodeCache.TryGetValue(branch.NextNodeGuid, out var targetNode))
            {
                StartCoroutine(ExecuteNodeParallelRoutine(targetNode));
            }
        }
        
        multiplyNode.Exit();
    }

    private AddNode FindCoordinatorFor(MultiplyNode multiply)
    {
        return StateNodes.OfType<AddNode>().FirstOrDefault(an => 
            an.InputBranches.Any(ib => multiply.Branches.Any(mb => mb.NextNodeGuid == ib.NextNodeGuid)));
    }

    private IEnumerator ExecuteNodeParallelRoutine(BossNode node)
    {
        yield return new WaitForSeconds(Mathf.Max(node.Delay, 0.05f));
        
        node.Enter();
        node.ExecuteLogic();

        while (!node.IsFinished) yield return null;
        
        node.Exit();
        NotifyAddNodeIfWaiting(node);
    }

    private void NotifyAddNodeIfWaiting(BossNode completedNode)
    {
        foreach (var addNode in StateNodes.OfType<AddNode>())
        {
            for (int i = 0; i < addNode.InputBranches.Count; i++)
            {
                if (addNode.InputBranches[i].NextNodeGuid == completedNode.Guid)
                {
                    if (i < addNode.InputCompleted.Count)
                        addNode.InputCompleted[i] = true;
                }
            }
        }
    }

    private void ExecuteAddNodeSync(AddNode addNode)
    {
        if (addNode.InputCompleted.All(c => c))
        {
            addNode.ExecuteLogic();
        }
    }

    public void TransitionToState(BossStateType type)
    {
        if (!_typeToNodes.TryGetValue(type, out var potentialNodes) || potentialNodes.Count == 0) return;

        BossNode selectedNode = potentialNodes.FirstOrDefault(n => n.CanEnter(this)) 
                                ?? potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];

        if (selectedNode == _currentState && !_currentState.IsFinished) return;

        StartCoroutine(DelayedTransition(selectedNode));
    }

    private IEnumerator DelayedTransition(BossNode newNode)
    {
        _isTransitioning = true;
        yield return new WaitForSeconds(Mathf.Max(newNode.Delay, 0.01f));
        
        _currentState?.Exit();
        _previousState = _currentState;
        _currentState = newNode;

        _currentState.ResetFinished();
        _currentState.Enter();
        
        _isTransitioning = false;
        _hasTriggeredAutoTransition = false;
    }

    public void AutoTransitionToNextNode()
    {
        if (_isTransitioning) return;

        if (_currentState != null && _nodeCache.TryGetValue(_currentState.NextNodeGuid, out var nextNode))
        {
            StartCoroutine(DelayedTransition(nextNode));
            return;
        }

        if (_currentState is EndNode)
        {
            TransitionToState(BossStateType.Start);
            return;
        }

        StopAI();
    }

    public void SetCurrentPhase(int phase)
    {
        _currentPhase = phase;
    }

    public string GetCurrentNodeName()
    {
        return _currentState?.GetType().Name ?? "None";
    }

    #region Helper Methods
    public BossNode GetNode(string guid) => _nodeCache.GetValueOrDefault(guid);
    public bool IsPhaseNodeExecuted(string guid) => _executedPhaseNodes.Contains(guid);
    public void MarkPhaseNodeExecuted(string guid) => _executedPhaseNodes.Add(guid);
    public void StartAI() { _isRunning = true; Debug.Log("AI Started"); }
    public void StopAI() { _isRunning = false; Debug.Log("AI Stopped"); }
    #endregion


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
