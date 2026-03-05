using UnityEngine;
using System.Collections.Generic;
using GlobalEnums;
using SerializeReferenceEditor;

public class EnemyUniversalMachine : EntityStateMachine<EnemyUniversalMachine>
{
    [Header("Modules")]
    public EnemyPuppetMovement Movement;
    public EnemyAnimationController Animation;
    public Rigidbody2D Rb;
    public Animator Anim;
    public ParticleSystem deathParticles;

    [Header("Loot")]
    public int MoneyDropAmount = 10;
    public GameObject MoneyPrefab;

    [Header("Enemy Stats")]
    public string enemyId = "";
    public int MaxHealth = 100;
    public int CurrentHealth { get; private set; }
    public int speed = 5;

    public DamageFlash _damageFlash;

    [Header("Enemy Type Detection")]
    public EnemyMovementType EnemyType = EnemyMovementType.Ground;
    
    [Header("Global Detection - Air Enemy")]
    public float AttackRange = 2f;
    public float ChaseRange = 5f;
    
    [Header("Global Detection - Ground Enemy")]
    public float GroundChaseDistance = 8f;
    public float GroundChaseSpread = 30f;
    public float GroundAttackDistance = 3f;
    public float GroundAttackSpread = 15f;
    public int GroundRayCount = 3;
    
    [Header("Ground Enemy - Back Detection")]
    public bool EnableBackDetection = true;
    public float BackChaseDistance = 4f;
    public float BackChaseSpread = 45f;
    public float BackAttackDistance = 2f;
    public float BackAttackSpread = 30f;
    public int BackRayCount = 3;
    
    [Header("Ground Movement")]
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

        if (type == EnemyStateType.Attack && _currentNode?.StateType == EnemyStateType.Attack && !_currentNode.IsFinished) 
        {
            if (!(_currentNode is ThrowAttackNode throwNode && throwNode.RemainingThrows > 1))
                return;
        }

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

        if (selectedNode == _currentNode && !_currentNode.IsFinished) return;

        if (type == EnemyStateType.Attack && !IsCooldownFinished("GlobalAttack")) return;

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

        if (Target != null)
        {
            Vector2 toPlayer = (Target.position - CachedTransform.position).normalized;
            int newDir = toPlayer.x >= 0 ? 1 : -1;
            LastSeenDir = newDir;
        }

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
        if (!string.IsNullOrEmpty(enemyId))
        {
            EnemyDefeatManager.ReportEnemyDeath(enemyId);
        }

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
        
        if (EnemyType == EnemyMovementType.Air)
        {
            return (CachedTransform.position - Target.position).sqrMagnitude <= AttackRange * AttackRange;
        }
        else
        {
            bool frontDetection = IsPlayerDetectedByGroundRaycast(true);
            bool backDetection = IsPlayerDetectedByBackRaycast(true);
            
            return frontDetection || backDetection;
        }
    }

    public bool IsPlayerInChaseRange()
    {
        if (Target == null) return false;
        
        if (EnemyType == EnemyMovementType.Air)
        {
            return (CachedTransform.position - Target.position).sqrMagnitude <= ChaseRange * ChaseRange;
        }
        else
        {
            bool frontDetection = IsPlayerDetectedByGroundRaycast(false);
            bool backDetection = IsPlayerDetectedByBackRaycast(false);
            
            return frontDetection || backDetection;
        }
    }
    
    private bool IsPlayerDetectedByGroundRaycast(bool isAttackRange)
    {
        if (Target == null) return false;
        
        Vector2 enemyPos = CachedTransform.position;
        Vector2 toPlayer = (Vector2)Target.position - enemyPos;
        float distanceToPlayer = toPlayer.magnitude;
        
        float maxDistance = isAttackRange ? GroundAttackDistance : GroundChaseDistance;
        float maxAngle = isAttackRange ? GroundAttackSpread : GroundChaseSpread;
        
        if (distanceToPlayer > maxDistance) return false;
        
        int facingDir = LastSeenDir;
        Vector2 forwardDir = new Vector2(facingDir, 0);
        
        float angleToPlayer = Vector2.Angle(forwardDir, toPlayer);
        if (angleToPlayer > maxAngle) return false;
        
        float angleStep = (maxAngle * 2f) / (GroundRayCount - 1);
        float startAngle = -maxAngle;
        
        int hitCount = 0;
        
        for (int i = 0; i < GroundRayCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle * facingDir) * forwardDir;
            
            RaycastHit2D hit = Physics2D.Raycast(enemyPos, rayDirection, maxDistance, CharacterLayer);
            
            if (hit.collider != null && hit.collider.transform == Target)
            {
                hitCount++;
                
                Color rayColor = isAttackRange ? Color.red : Color.blue;
                Debug.DrawRay(enemyPos, rayDirection * maxDistance, rayColor, 0.1f);
            }
            else
            {
                Debug.DrawRay(enemyPos, rayDirection * maxDistance, Color.gray, 0.1f);
            }
        }
        
        return hitCount > 0;
    }
    
    private bool IsPlayerDetectedByBackRaycast(bool isAttackRange)
    {
        if (Target == null) return false;
        if (!EnableBackDetection) return false;
        
        Vector2 enemyPos = CachedTransform.position;
        Vector2 toPlayer = (Vector2)Target.position - enemyPos;
        float distanceToPlayer = toPlayer.magnitude;
        
        float maxDistance = isAttackRange ? BackAttackDistance : BackChaseDistance;
        float maxAngle = isAttackRange ? BackAttackSpread : BackChaseSpread;
        
        if (distanceToPlayer > maxDistance) return false;
        
        int facingDir = LastSeenDir;
        Vector2 backwardDir = new Vector2(-facingDir, 0);
        
        float angleToPlayer = Vector2.Angle(backwardDir, toPlayer);
        if (angleToPlayer > maxAngle) return false;
        
        float angleStep = (maxAngle * 2f) / (BackRayCount - 1);
        float startAngle = -maxAngle;
        
        int hitCount = 0;
        
        for (int i = 0; i < BackRayCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle * facingDir) * backwardDir;
            
            RaycastHit2D hit = Physics2D.Raycast(enemyPos, rayDirection, maxDistance, CharacterLayer);
            
            if (hit.collider != null && hit.collider.transform == Target)
            {
                hitCount++;
                
                Color rayColor = isAttackRange ? Color.yellow : Color.green;
                Debug.DrawRay(enemyPos, rayDirection * maxDistance, rayColor, 0.1f);
            }
            else
            {
                Debug.DrawRay(enemyPos, rayDirection * maxDistance, Color.gray, 0.1f);
            }
        }
        
        return hitCount > 0;
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
        if (EnemyType == EnemyMovementType.Air)
        {
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawWireSphere(transform.position, ChaseRange);
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
        else
        {
            int currentDir = transform.localScale.x >= 0 ? 1 : -1;
            Vector2 enemyPos = transform.position;
            Vector2 forwardDir = new Vector2(currentDir, 0);
            Vector2 backwardDir = new Vector2(-currentDir, 0);
            
            float frontChaseAngleStep = (GroundChaseSpread * 2f) / (GroundRayCount - 1);
            float frontChaseStartAngle = -GroundChaseSpread;
            
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            for (int i = 0; i < GroundRayCount; i++)
            {
                float currentAngle = frontChaseStartAngle + frontChaseAngleStep * i;
                Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle * currentDir) * forwardDir;
                Gizmos.DrawRay(enemyPos, rayDirection * GroundChaseDistance);
            }
            
            if (EnableBackDetection)
            {
                float backChaseAngleStep = (BackChaseSpread * 2f) / (BackRayCount - 1);
                float backChaseStartAngle = -BackChaseSpread;
                
                Gizmos.color = new Color(1, 0.5f, 0, 0.3f); // Orange for back chase
                for (int i = 0; i < BackRayCount; i++)
                {
                    float currentAngle = backChaseStartAngle + backChaseAngleStep * i;
                    Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle * currentDir) * backwardDir;
                    Gizmos.DrawRay(enemyPos, rayDirection * BackChaseDistance);
                }
            }
            
            float frontAttackAngleStep = (GroundAttackSpread * 2f) / (GroundRayCount - 1);
            float frontAttackStartAngle = -GroundAttackSpread;
            
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            for (int i = 0; i < GroundRayCount; i++)
            {
                float currentAngle = frontAttackStartAngle + frontAttackAngleStep * i;
                Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle * currentDir) * forwardDir;
                Gizmos.DrawRay(enemyPos, rayDirection * GroundAttackDistance);
            }
            
            if (EnableBackDetection)
            {
                float backAttackAngleStep = (BackAttackSpread * 2f) / (BackRayCount - 1);
                float backAttackStartAngle = -BackAttackSpread;
                
                Gizmos.color = new Color(1, 1, 0, 0.3f);
                for (int i = 0; i < BackRayCount; i++)
                {
                    float currentAngle = backAttackStartAngle + backAttackAngleStep * i;
                    Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle * currentDir) * backwardDir;
                    Gizmos.DrawRay(enemyPos, rayDirection * BackAttackDistance);
                }
            }
        }

        if (EnemyType == EnemyMovementType.Ground)
        {
            int currentDir = transform.localScale.x >= 0 ? 1 : -1;

            Vector2 groundOrigin = (Vector2)transform.position + Vector2.right * currentDir * 0.5f;
            Gizmos.color = _isGroundAhead ? Color.green : Color.red;
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * GroundCheckDistance);

            Vector2 wallOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
            Gizmos.color = _isWallAhead ? Color.red : Color.green;
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * currentDir * WallCheckDistance);
        }

        if (_currentNode != null && _currentNode.StateType == EnemyStateType.Chase)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up, Vector3.right * LastSeenDir * 1.5f);
        }
    }
#endif
}
