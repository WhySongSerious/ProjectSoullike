using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectSoullike
{
    // Adapted from the teammate boss-state-machine prototype.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GolemHealth))]
    public sealed class BossController : MonoBehaviour
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float detectionRange = 14f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float chaseSpeed = 2.2f;
        [SerializeField, Min(0f)] private float rotationSpeed = 360f;
        [SerializeField, Min(0.1f)] private float attackRange = 2.5f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float attackDamage = 20f;
        [SerializeField, Min(0f)] private float attackDelay = 0.5f;
        [SerializeField, Min(0f)] private float attackCooldown = 1.5f;

        private readonly Dictionary<BossStateType, BossState> _states =
            new Dictionary<BossStateType, BossState>();
        private GolemHealth _health;
        private NavMeshAgent _agent;
        private Animator _animator;
        private BossStateMachine _stateMachine;

        public bool HasTarget => target != null && target.gameObject.activeInHierarchy;
        public Transform Target => target;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float AttackDamage => attackDamage;
        public float AttackDelay => attackDelay;
        public float AttackCooldown => attackCooldown;
        public float DistanceToTarget => HasTarget
            ? Vector3.Distance(transform.position, target.position)
            : float.PositiveInfinity;

        private void Awake()
        {
            _health = GetComponent<GolemHealth>();
            _agent = GetComponent<NavMeshAgent>();
            if (_agent == null)
            {
                _agent = gameObject.AddComponent<NavMeshAgent>();
            }

            _agent.speed = chaseSpeed;
            _agent.angularSpeed = rotationSpeed;
            _agent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.8f);
            _animator = GetComponentInChildren<Animator>(includeInactive: true);
            _stateMachine = new BossStateMachine();
            _states.Add(BossStateType.Idle, new BossIdleState(this));
            _states.Add(BossStateType.Chase, new BossChaseState(this));
            _states.Add(BossStateType.Attack, new BossAttackState(this));
            _states.Add(BossStateType.Dead, new BossDeadState(this));
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.Died += HandleDeath;
            }
        }

        private void Start()
        {
            FindTargetIfNeeded();
            ChangeState(BossStateType.Idle);
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= HandleDeath;
            }
        }

        private void Update()
        {
            if (_health == null || !_health.IsAlive)
            {
                return;
            }

            FindTargetIfNeeded();
            _stateMachine?.Update();
        }

        public void ChangeState(BossStateType stateType)
        {
            if (!_states.TryGetValue(stateType, out BossState nextState))
            {
                Debug.LogError($"[Boss] Unregistered state: {stateType}", this);
                return;
            }

            _stateMachine.ChangeState(nextState);
        }

        public bool IsTargetWithinDetectionRange()
        {
            return HasTarget && DistanceToTarget <= detectionRange;
        }

        public void FaceTarget()
        {
            if (!HasTarget)
            {
                return;
            }

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            float blend = 1f - Mathf.Exp(-rotationSpeed * Mathf.Deg2Rad * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                blend);
        }

        public void MoveToTarget()
        {
            if (!HasTarget)
            {
                return;
            }

            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.speed = chaseSpeed;
                _agent.isStopped = false;
                _agent.SetDestination(target.position);
                SetMovementAnimation(1f);
                return;
            }

            // Prototype fallback until the active scene has a baked NavMesh.
            Vector3 destination = target.position;
            destination.y = transform.position.y;
            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                chaseSpeed * Time.deltaTime);
            SetMovementAnimation(1f);
        }

        public void StopMovement()
        {
            SetMovementAnimation(0f);

            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

        public void PlayAttackAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(AttackHash);
            }
        }

        public PlayerHealth GetTargetHealth()
        {
            if (!HasTarget)
            {
                return null;
            }

            PlayerHealth health = target.GetComponent<PlayerHealth>();
            return health != null ? health : target.GetComponentInParent<PlayerHealth>();
        }

        private void FindTargetIfNeeded()
        {
            if (HasTarget)
            {
                return;
            }

            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                target = playerHealth.transform;
                return;
            }

            KeyboardPlayerController playerController =
                FindFirstObjectByType<KeyboardPlayerController>();
            if (playerController != null)
            {
                target = playerController.transform;
            }
        }

        private void HandleDeath()
        {
            ChangeState(BossStateType.Dead);
        }

        private void SetMovementAnimation(float speed)
        {
            if (_animator != null)
            {
                _animator.SetFloat(MoveSpeedHash, speed, 0.1f, Time.deltaTime);
            }
        }
    }
}
