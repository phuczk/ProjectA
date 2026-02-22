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
    public ParticleSystem deathParticles;

    [Header("Loot")]
    public int MoneyDropAmount = 10;
    public GameObject MoneyPrefab;

    [Header("Enemy Stats")]
    public int MaxHealth = 100;
    public int CurrentHealth { get; private set; }
    public int speed = 5;

    public DamageFlash _damageFlash;

    [Header("Global Detection")]
    public float AttackRange = 2f;
    public float ChaseRange = 5f;
    public float GroundCheckDistance = 2.5f;
    public float WallCheckDistance = 1.5f;

    public LayerMask CharacterLayer;
    public LayerMask GroundLayer;
    public Transform Target { get; private set; }
    public Transform CachedTransform { get; private set; }

    private bool _isGroundAhead = false;
    private bool _isWallAhead = false;
    private int _cachedRaycastDir = 0;

    public EnemyStateType DefaultStateType = EnemyStateType.Idle;

    [Header("AI Configuration")]
    [SerializeReference, SR] public List<EnemyStateNode> StateNodes = new List<EnemyStateNode>();

    private EnemyStateNode _currentNode;

    [Header("State Decisions")]
    public bool IsDeath => CurrentHealth <= 0;
    public bool JustTakenDamageThisFrame = false;

    private Dictionary<EnemyStateType, List<EnemyStateNode>> _typeToNodes = new Dictionary<EnemyStateType, List<EnemyStateNode>>();

    private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();

    public void SetCooldown(string actionName, float duration) 
        => _cooldowns[actionName] = Time.time + duration;

    public bool IsCooldownFinished(string actionName) 
        => !_cooldowns.ContainsKey(actionName) || Time.time >= _cooldowns[actionName];

    protected virtual void Awake()
    {
        CachedTransform = transform;
        Movement = GetComponent<EnemyPuppetMovement>();
        Rb = GetComponent<Rigidbody2D>();
        Target = GameObject.FindGameObjectWithTag("Player")?.transform;
        CurrentHealth = MaxHealth;

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

    private void Start() {
        if (Target == null)
        {
            Target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
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

        selectedNode ??= potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];

        if (selectedNode == _currentNode) return;

        _currentNode?.Exit();
        _currentNode = selectedNode;
        _currentNode.ResetFinished();
        _currentNode.Enter();

    }

    protected override void Update()
    {
        _cachedRaycastDir = CachedTransform.localScale.x >= 0 ? 1 : -1;
        _isGroundAhead = HasGroundAheadInternal(_cachedRaycastDir);
        _isWallAhead = HasWallAheadInternal(_cachedRaycastDir);

        _currentNode?.LogicUpdate();
    }

    public override void TakeDamage(int damage)
    {
        if(CurrentHealth <= 0)
        {
            Death();
            return;
        }
        CurrentHealth -= damage;
        JustTakenDamageThisFrame = true;

        if (_damageFlash == null) return;
        _damageFlash.CallDamageFlash();
    }

    public void Death()
    {
        if (MoneyPrefab != null)
        {
            for (int i = 0; i < MoneyDropAmount; i++)
            {
                Vector3 spawnPos = CachedTransform.position;

                GameObject money = BulletPool.Instance.Get(
                    MoneyPrefab,
                    spawnPos,
                    Quaternion.identity
                );

                Rigidbody2D rb = money.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    float angle = Random.Range(0f, 180f) * Mathf.Deg2Rad;

                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                    float force = Random.Range(8f, 16f);

                    rb.linearVelocity = dir * force;
                }
            }
        }

        gameObject.SetActive(false);
    }

    public bool IsPlayerInAttackRange()
    {
        if (Target == null) return false;
        return (CachedTransform.position - Target.position).sqrMagnitude <= AttackRange * AttackRange;
    }

    public bool IsPlayerInChaseRange()
    {
        if (Target == null) return false;
        return (CachedTransform.position - Target.position).sqrMagnitude <= ChaseRange * ChaseRange;
    }

    public bool HasGroundAhead(int dir)
    {
        if (dir == _cachedRaycastDir) return _isGroundAhead;
        return HasGroundAheadInternal(dir);
    }

    private bool HasGroundAheadInternal(int dir)
    {
        Vector2 origin = (Vector2)CachedTransform.position + Vector2.right * dir * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, GroundCheckDistance, GroundLayer);

        return hit.collider != null;
    }

    public bool HasWallAhead(int dir)
    {
        if (dir == _cachedRaycastDir) return _isWallAhead;
        return HasWallAheadInternal(dir);
    }

    private bool HasWallAheadInternal(int dir)
    {
        Vector2 origin = (Vector2)CachedTransform.position + Vector2.up * 0.5f;
        
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, WallCheckDistance, GroundLayer);

        return hit.collider != null;
    }

    public bool IsCurrentNodeFinished() => _currentNode != null && _currentNode.IsFinished;

    public int PlayerHealth => 100;
    public int LastSeenDir { get; set; } = 1;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, ChaseRange);
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        int currentDir = transform.localScale.x >= 0 ? 1 : -1;

        Vector2 groundOrigin = (Vector2)transform.position + Vector2.right * currentDir * 0.5f;
        Gizmos.color = _isGroundAhead ? Color.green : Color.red;
        Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * GroundCheckDistance);

        Vector2 wallOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
        Gizmos.color = _isWallAhead ? Color.red : Color.green;
        Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * currentDir * WallCheckDistance);

        if (_currentNode != null && _currentNode.StateType == EnemyStateType.Chase)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up, Vector3.right * LastSeenDir * 1.5f);
        }
    }
#endif
}
