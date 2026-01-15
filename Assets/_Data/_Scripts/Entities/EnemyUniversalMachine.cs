using UnityEngine;
using System.Collections.Generic;
using GlobalEnums;
using SerializeReferenceEditor;

public class EnemyUniversalMachine : EntityStateMachine<EnemyUniversalMachine>
{
    [Header("Modules")]
    public EnemyPuppetMovement Movement;
    public Rigidbody2D Rb;
    public Animator Anim;

    [Header("Global Detection")]
    public float AttackRange = 2f;
    public float ChaseRange = 5f;
    public float GroundCheckDistance = 2.5f;
    public float WallCheckDistance = 1.5f;

    public LayerMask CharacterLayer;
    public LayerMask GroundLayer;
    public Transform Target { get; private set; }

    public EnemyStateType DefaultStateType = EnemyStateType.Idle;

    [Header("AI Configuration")]
    [SerializeReference, SR] public List<EnemyStateNode> StateNodes = new List<EnemyStateNode>();

    private EnemyStateNode _currentNode;

    private Dictionary<EnemyStateType, List<EnemyStateNode>> _typeToNodes = new Dictionary<EnemyStateType, List<EnemyStateNode>>();

    private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();

    public void SetCooldown(string actionName, float duration) 
        => _cooldowns[actionName] = Time.time + duration;

    public bool IsCooldownFinished(string actionName) 
        => !_cooldowns.ContainsKey(actionName) || Time.time >= _cooldowns[actionName];

    protected virtual void Awake()
    {
        Movement = GetComponent<EnemyPuppetMovement>();
        Rb = GetComponent<Rigidbody2D>();
        Target = GameObject.FindGameObjectWithTag("Player")?.transform;

        foreach (var node in StateNodes)
        {
            node.Initialize(this);
            
            if (!_typeToNodes.ContainsKey(node.StateType))
            {
                _typeToNodes[node.StateType] = new List<EnemyStateNode>();
            }
            _typeToNodes[node.StateType].Add(node);
        }

        TransitionToState(DefaultStateType);
    }

    public void TransitionToState(EnemyStateType type)
    {
        if (!_typeToNodes.ContainsKey(type)) return;
        var potentialNodes = _typeToNodes[type];

        if (potentialNodes.Count == 0) return;

        EnemyStateNode selectedNode = null;
        foreach (var node in potentialNodes)
        {
            if (node.CanEnter(this))
            {
                selectedNode = node;
                break;
            }
        }

        if (selectedNode == null)
            selectedNode = potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];

        if (selectedNode == _currentNode) return;

        _currentNode?.Exit();
        _currentNode = selectedNode;
        _currentNode.ResetFinished();
        _currentNode.Enter();

        Debug.Log($"[AI] Switched to {type}: {selectedNode.Guid}");
    }

    protected override void Update()
    {
        _currentNode?.LogicUpdate();
    }

    public bool IsPlayerInAttackRange()
    {
        if (Target == null) return false;
        return Vector2.Distance(transform.position, Target.position) <= AttackRange;
    }

    public bool IsPlayerInChaseRange()
    {
        if (Target == null) return false;
        return Vector2.Distance(transform.position, Target.position) <= ChaseRange;
    }

    public bool HasGroundAhead(int dir)
    {
        Vector2 origin = (Vector2)transform.position + Vector2.right * dir * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, GroundCheckDistance, GroundLayer);

        return hit.collider != null;
    }

    public bool HasWallAhead(int dir)
    {
        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.5f;
        
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, WallCheckDistance, GroundLayer);

        return hit.collider != null;
    }

    public bool IsCurrentNodeFinished() => _currentNode != null && _currentNode.IsFinished;

    public int PlayerHealth => 100;
    public int LastSeenDir { get; set; } = 1;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, ChaseRange);
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        int currentDir = transform.localScale.x >= 0 ? 1 : -1;

        float groundRayDist = GroundCheckDistance;
        Vector2 groundOrigin = (Vector2)transform.position + Vector2.right * currentDir * 0.5f;
        bool groundDetected = HasGroundAhead(currentDir);
        
        Gizmos.color = groundDetected ? Color.green : Color.red;
        Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * groundRayDist);
        Gizmos.DrawSphere(groundOrigin + Vector2.down * groundRayDist, 0.05f);

        float wallRayDist = WallCheckDistance;
        Vector2 wallOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
        bool wallDetected = HasWallAhead(currentDir);

        Gizmos.color = wallDetected ? Color.red : Color.green;
        Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * currentDir * wallRayDist);
        Gizmos.DrawSphere(wallOrigin + Vector2.right * currentDir * wallRayDist, 0.05f);

        if (_currentNode != null && _currentNode.StateType == EnemyStateType.Chase)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up, Vector3.right * LastSeenDir * 1.5f);
        }
    }
#endif
}
