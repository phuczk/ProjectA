using UnityEngine;
using GlobalEnums;
public class TestEnemyBehaviour : EntityStateMachine<TestEnemyBehaviour>
{
    [Header("Movement Module")]
    public EnemyPuppetMovement Movement { get; private set; }
    [Header("Loot Prefabs")]
    public GameObject pigmentPrefab;
    public GameObject shardPrefab;

    [Header("Properties")]
    [SerializeField] private int m_StartLife;
    [SerializeField] private bool m_StartFacingRight;

    [Header("Collisions")]
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private LayerMask m_EnemyLayer;

    [Header("Idle")]
    [SerializeField, RangedValue(0, 30)] private RangedFloat m_IdleTime;

    [Header("Patrol")]
    [SerializeField, Range(0, 100)] private float m_PatrolChance;
    [SerializeField, RangedValue(0, 30)] private RangedFloat m_PatrolTime;
    [SerializeField] private float m_PatrolMoveSpeed;

    [Header("Detection")]
    [SerializeField] private Transform m_Player;
    [SerializeField] private float m_ChaseRange = 6f;
    [SerializeField] private float m_ChaseMoveSpeed;
    public int LastSeenDir { get; set; } = 1;

    [Header("Attack")]
    [SerializeField] private float m_AttackDuration = 0.3f;
    [SerializeField] private LayerMask m_CharactersLayer;
    [SerializeField] private AttackRangeType m_AttackRangeType;
    [SerializeField] private Vector2 m_AttackBoxSize = new Vector2(2f, 1.2f);
    [SerializeField] private float m_AttackCircleRadius = 1.5f;
    // [SerializeField] private float m_AttackDelay = 0.3f;
    // [SerializeField] private float m_ComboDelay = 0.6f;

    [Header("Attack Dash")]
    [SerializeField] private float m_AttackDashSpeed = 8f;
    [SerializeField] private float m_AttackDashTime = 0.25f;
    [SerializeField] private float m_AttackWindup = 0.15f;

    [Header("Hurt")]
    [SerializeField] private float m_HurtDuration;
    [SerializeField] private Vector2 m_HurtOffset;

    [Header("Death")]
    [SerializeField] private float m_DeathDuration;
    [SerializeField] private float m_DeathFadeDelay;
    [SerializeField] private ParticleSystem dieParticles;

    public Rigidbody2D Rb { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    private int facingDirection { get; set; }
    private float life { get; set; }
    private float normalizedSpeedFactor { get; set; }

    private bool onLeftLedge { get; set; }
    private bool onRightLedge { get; set; }
    private bool onLeftWall { get; set; }
    private bool onRightWall { get; set; }
    public bool IsGrounded;

    private bool leftObstructed => !onLeftLedge || onLeftWall;
    private bool rightObstructed => !onRightLedge || onRightWall;

    public bool WasRecentlyHit { get; private set; }
    public int CurrentHealth { get; private set; } = 100;
    private EnemyStateType currentStateType;

    public float rayLength = 2.5f;

    private IdleState _idleState;
    private PatrolState _patrolState;
    private ChaseState _chaseState;
    private AttackState _attackState;
    private EvadeState _evadeState;
    private HurtState _hurtState;
    private DieState _dieState;

    private void Awake()
    {
        Movement = GetComponent<EnemyPuppetMovement>();
        Rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        m_Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        CurrentHealth = m_StartLife;

        _idleState = new IdleState(this);
        _patrolState = new PatrolState(this);
        _chaseState = new ChaseState(this);
        _attackState = new AttackState(this);
        _evadeState = new EvadeState(this);
        _hurtState = new HurtState(this);
        _dieState = new DieState(this);

        SwitchState(_patrolState);
    }
    
    public bool HasGroundAhead(int dir)
    {
        Vector2 origin = Rb.position + Vector2.right * dir * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            rayLength,
            m_GroundLayer
        );

        RaycastHit2D aheadHit = Physics2D.Raycast(
            origin,
            Vector2.right * dir,
            0.2f,
            m_GroundLayer
        );

        return hit.collider != null;
    }

    public override void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        Debug.Log($"Enemy take damage {damage}, current health {CurrentHealth}");
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            //SwitchState(new DieState(this));
        }
        else
        {
            if (currentStateType == EnemyStateType.Chase) return;
            SwitchState(_chaseState);
        }
    }

    private void EnsurePlayer()
    {
        if (m_Player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            m_Player = playerObj.transform;
        }
    }

    public bool IsPlayerInChaseRange()
    {
        EnsurePlayer();
        if (m_Player == null) return false;

        float dist = Vector2.Distance(transform.position, m_Player.position);
        return dist <= m_ChaseRange;
    }

    public bool IsPlayerInAttackRange()
    {
        EnsurePlayer();
        if (m_Player == null) return false;

        int dir = transform.localScale.x >= 0 ? 1 : -1;

        if (m_AttackRangeType == AttackRangeType.Circle)
        {
            float dist = Vector2.Distance(transform.position, m_Player.position);
            return dist <= m_AttackCircleRadius;
        }
        else
        {
            Vector2 center =
                (Vector2)transform.position +
                Vector2.right * dir * m_AttackBoxSize.x * 0.5f;

            return Physics2D.OverlapBox(
                center,
                m_AttackBoxSize,
                0f,
                m_CharactersLayer
            );
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Rigidbody2D _rb = Rb != null ? Rb : GetComponent<Rigidbody2D>();

        Vector2 pos = _rb != null ? _rb.position : (Vector2)transform.position;

        int dir = transform.localScale.x >= 0 ? 1 : -1;

        Vector2 origin = pos + Vector2.right * dir * 0.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(origin, 0.05f);

        Gizmos.DrawLine(origin, origin + Vector2.down * rayLength);

        // CHASE RANGE
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_ChaseRange);

        // ATTACK RANGE
        Gizmos.color = Color.red;

        if (m_AttackRangeType == AttackRangeType.Circle)
        {
            Gizmos.DrawWireSphere(transform.position, m_AttackCircleRadius);
        }
        else
        {
            Vector3 center =
                transform.position + Vector3.right * dir * m_AttackBoxSize.x * 0.5f;

            Gizmos.DrawWireCube(center, m_AttackBoxSize);
        }
    }
#endif

    public void DebugLog()
    {
        Debug.Log($"Current state: {currentStateType}");
    }

    public abstract class StateBase : EntityBehaviourState<TestEnemyBehaviour>
    {
        protected StateBase(TestEnemyBehaviour entity) : base(entity)
        {
        }
    }

    public class IdleState : StateBase
    {
        private float idleTimer;

        public IdleState(TestEnemyBehaviour entity) : base(entity) { }

        public override void Enter()
        {
            idleTimer = entity.m_IdleTime.RandomRange();
            entity.Movement.SetMoveDirection(Vector2.zero);
            entity.currentStateType = EnemyStateType.Idle;
            entity.DebugLog();
        }

        public override void LogicUpdate()
        {
            idleTimer -= Time.deltaTime;
            entity.Movement.BobAndSway(true);
            if (idleTimer <= 0f)
            {
                entity.SwitchState(entity._patrolState);
            }
        }
    }

    public class PatrolState : StateBase
    {
        private float dir;
        private float timer;
        private float turnCooldown = 0.2f;

        public PatrolState(TestEnemyBehaviour entity) : base(entity) { }

        public override void Enter()
        {
            timer = entity.m_PatrolTime.RandomRange();
            dir = Random.value > 0.5f ? 1f : -1f;
            entity.currentStateType = EnemyStateType.Patrol;
            entity.DebugLog();
        }

        public override void LogicUpdate()
        {
            timer -= Time.deltaTime;
            turnCooldown -= Time.deltaTime;

            if (entity.IsPlayerInAttackRange())
            {
                entity.SwitchState(entity._attackState);
                return;
            }

            if (entity.IsPlayerInChaseRange())
            {
                entity.SwitchState(entity._chaseState);
                return;
            }

            if (turnCooldown <= 0f && !entity.HasGroundAhead((int)dir))
            {
                dir *= -1f;
                turnCooldown = 0.2f;
            }

            entity.Movement.SetMoveDirection(Vector2.right * dir);
            entity.Movement.TickMovement();

            if (timer <= 0f)
            {
                entity.SwitchState(entity._idleState);
            }
        }
    }

    public class ChaseState : StateBase
    {
        private float lostTimer;

        public ChaseState(TestEnemyBehaviour entity) : base(entity) { }

        public override void Enter()
        {
            lostTimer = 5f;
            entity.currentStateType = EnemyStateType.Chase;
            entity.DebugLog();
        }

        public override void LogicUpdate()
        {
            if (entity.IsPlayerInAttackRange())
            {
                entity.SwitchState(entity._attackState);
                return;
            }

            if (entity.IsPlayerInChaseRange())
            {
                lostTimer = 5f;
                entity.LastSeenDir = (entity.m_Player.position.x > entity.transform.position.x) ? 1 : -1;
            }
            else
            {
                lostTimer -= Time.deltaTime;
                if (lostTimer <= 0f)
                {
                    entity.SwitchState(entity._patrolState);
                    return;
                }
            }

            entity.Movement.SetMoveDirection(Vector2.right * entity.LastSeenDir);
            entity.Movement.TickMovement(entity.m_ChaseMoveSpeed);
        }
    }

    public class AttackState : StateBase
    {
        private float attackDuration;
        private float timer;
        private float dashTimer;
        private Vector2 dashDir;

        private enum Phase
        {
            Windup,
            Dash,
            Recover
        }

        private Phase phase;

        public AttackState(TestEnemyBehaviour entity) : base(entity) { }

        public override void Enter()
        {
            attackDuration = entity.m_AttackDuration;
            phase = Phase.Windup;
            timer = entity.m_AttackWindup;
            dashTimer = entity.m_AttackDashTime;

            entity.currentStateType = EnemyStateType.Attack;
            entity.Movement.SetMoveDirection(Vector2.zero);
            entity.Rb.linearVelocity = Vector2.zero;

            dashDir = (entity.m_Player.position.x > entity.transform.position.x) ? Vector2.right : Vector2.left;

            entity.transform.localScale = new Vector3(Mathf.Sign(dashDir.x), 1, 1);

            entity.DebugLog();
        }

        public override void LogicUpdate()
            {
                attackDuration -= Time.deltaTime;
            if (attackDuration <= 0f)
            {
                entity.SwitchState(entity._chaseState);
            }
            switch (phase)
            {
                case Phase.Windup:
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                    {
                        phase = Phase.Dash;
                    }
                    break;

                case Phase.Dash:
                    dashTimer -= Time.deltaTime;

                    entity.Rb.linearVelocity =
                        dashDir * entity.m_AttackDashSpeed;

                    if (dashTimer <= 0f)
                    {
                        entity.Rb.linearVelocity = Vector2.zero;
                        phase = Phase.Recover;
                        timer = 0.2f;
                    }
                    break;

                case Phase.Recover:
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                    {
                        entity.SwitchState(entity._chaseState);
                    }
                    break;
            }
        }

        public override void Exit()
        {
            entity.Rb.linearVelocity = Vector2.zero;
        }
    }

    public class EvadeState : StateBase
    {
        // private float elapsedTime;
        // private float evadeDuration = 0.6f;

        public EvadeState(TestEnemyBehaviour entity) : base(entity) { }

        public override void Enter()
        {
            // elapsedTime = 0f;
            entity.currentStateType = EnemyStateType.Evade;
            entity.DebugLog();
        }

        public override void LogicUpdate()
        {
        }
    }

    public class HurtState : StateBase
    {
        private float elapsedTime { get; set; }

        public HurtState(TestEnemyBehaviour entity) : base(entity)
        {
        }

        public override void Enter()
        {
            elapsedTime = 0f;
            entity.currentStateType = EnemyStateType.Hurt;
            entity.DebugLog();
        }

        public override void LogicUpdate()
        {
        }
    }

    public class DieState : StateBase
    {
        private float elapsedTime { get; set; }
        public DieState(TestEnemyBehaviour entity) : base(entity)
        {
        }

        public override void Enter()
        {
            elapsedTime = 0f;
            entity.currentStateType = EnemyStateType.Dead;
            entity.DebugLog();
        }

        public override void LogicUpdate()
        {
        }
    }
}
