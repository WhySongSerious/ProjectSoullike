using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectSoullike
{
    public enum BossAttackPhase { None, Telegraph, Active, Recovery }

    [System.Serializable]
    public sealed class BossStrikeSettings
    {
        [Min(0.01f)] public float telegraph = 0.6f;
        [Min(0.01f)] public float active = 0.2f;
        [Min(0.01f)] public float recovery = 1f;
        [Min(0f)] public float damage = 20f;
        // World metres, independent of the imported model's scale.
        public Vector3 hitCenter = new Vector3(0f, 1f, 1.4f);
        public Vector3 hitSize = new Vector3(0.9f, 2f, 2.8f);
        [Tooltip("Optional full Animator state paths, played at the start of each phase.")]
        public string telegraphState;
        public string activeState;
        public string recoveryState;
    }

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

        [Header("Guardian Patterns (enable on the JIMIN scene instance only)")]
        [SerializeField] private bool useGuardianPatterns;
        [SerializeField, Range(0f, 1f)] private float comboProbability = 0.5f;
        [SerializeField, Range(0f, 1f)] private float thirdStrikeProbability = 1f;
        [SerializeField] private BossStrikeSettings overhead = new BossStrikeSettings
            { telegraph = 0.9f, active = 0.22f, recovery = 1.4f, damage = 28f };
        [SerializeField] private BossStrikeSettings firstSlash = new BossStrikeSettings
            { telegraph = 0.4f, active = 0.16f, recovery = 0.12f, damage = 12f,
                hitCenter = new Vector3(0f, 1f, 1.2f), hitSize = new Vector3(3f, 2f, 2.4f) };
        [SerializeField] private BossStrikeSettings secondSlash = new BossStrikeSettings
            { telegraph = 0.12f, active = 0.16f, recovery = 0.15f, damage = 12f,
                hitCenter = new Vector3(0f, 1f, 1.2f), hitSize = new Vector3(3f, 2f, 2.4f) };
        [SerializeField] private BossStrikeSettings thrust = new BossStrikeSettings
            { telegraph = 0.75f, active = 0.2f, recovery = 1.2f, damage = 32f,
                hitCenter = new Vector3(0f, 1f, 1.7f), hitSize = new Vector3(0.8f, 2f, 3.4f) };
        [SerializeField, Min(0.01f)] private float twoSlashRecovery = 0.9f;
        [SerializeField] private LayerMask attackHitMask = ~0;

        [Header("Guardian Presentation")]
        [Tooltip("Temporary geometric sword and eye marker; disable for the final model.")]
        [SerializeField] private bool showPrototypeWeapon;
        [SerializeField] private GameObject bladeTelegraph;
        [SerializeField] private GameObject eyesTelegraph;

        private Transform _prototypeRoot;
        private Transform _prototypeSword;
        private Renderer _prototypeBlade;
        private GameObject _prototypeEyes;
        private Material _prototypeMaterial;
        private MaterialPropertyBlock _cueProperties;
        private bool _started;

        public bool UseGuardianPatterns => useGuardianPatterns;
        public float ComboProbability => comboProbability;
        public float ThirdStrikeProbability => thirdStrikeProbability;
        public float TwoSlashRecovery => twoSlashRecovery;
        public BossAttackPhase AttackPhase { get; private set; }
        public int AttackStep { get; private set; }
        public bool IsComboAttack { get; private set; }
        public float PhaseProgress { get; private set; }
        public BossStrikeSettings CurrentStrike { get; private set; }

        private readonly Dictionary<BossStateType, BossState> _states =
            new Dictionary<BossStateType, BossState>();
        private GolemHealth _health;
        private NavMeshAgent _agent;
        private Animator _animator;
        private BossStateMachine _stateMachine;

        public bool HasTarget => target != null && target.gameObject.activeInHierarchy;
        public bool HasLivingTarget => HasTarget && GetTargetHealth() != null && GetTargetHealth().IsAlive;
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
            if (_agent == null && (!useGuardianPatterns ||
                NavMesh.SamplePosition(transform.position, out _, 2f, NavMesh.AllAreas)))
            {
                _agent = gameObject.AddComponent<NavMeshAgent>();
            }

            if (_agent != null)
            {
                _agent.speed = chaseSpeed;
                _agent.angularSpeed = rotationSpeed;
                _agent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.8f);
            }
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
            if (_started) ChangeState(BossStateType.Idle);
        }

        private void Start()
        {
            _started = true;
            FindTargetIfNeeded();
            ChangeState(BossStateType.Idle);
        }

        private void OnDisable()
        {
            _stateMachine?.CurrentState?.Exit();
            ClearAttackPresentation();
            StopMovement();
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
            return HasTarget && (!useGuardianPatterns || HasLivingTarget) && DistanceToTarget <= detectionRange;
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
            // Stop is called once on state entry; a damped write would leave a
            // nonzero blend value until another movement update is requested.
            if (_animator != null)
            {
                _animator.SetFloat(MoveSpeedHash, 0f);
            }

            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
            // Assigning velocity can resume an agent; stop after clearing its motion.
            _agent.isStopped = true;
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

        public BossStrikeSettings GetStrike(bool combo, int step)
        {
            return !combo ? overhead : step == 1 ? firstSlash : step == 2 ? secondSlash : thrust;
        }

        public void AimAttack()
        {
            if (!HasTarget) return;
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        public void ApplyStrike(BossStrikeSettings strike, int step, HashSet<PlayerHealth> hitTargets)
        {
            if (_health == null || !_health.IsAlive) return;
            PlayerHealth victim = GetTargetHealth();
            if (victim == null || !victim.IsAlive || hitTargets.Contains(victim)) return;
            Vector3 center = transform.position + transform.rotation * strike.hitCenter;
            Vector3 halfSize = GetHitHalfSize(strike);
            foreach (Collider hit in Physics.OverlapBox(center, halfSize, transform.rotation,
                         attackHitMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.GetComponentInParent<PlayerHealth>() != victim || !hitTargets.Add(victim)) continue;
                victim.TakeDamage(new DamageInfo(strike.damage, hit.ClosestPoint(center),
                    victim.transform.position - transform.position, gameObject, step));
            }
        }

        private static Vector3 GetHitHalfSize(BossStrikeSettings strike)
        {
            return new Vector3(Mathf.Max(0.01f, strike.hitSize.x), Mathf.Max(0.01f, strike.hitSize.y),
                Mathf.Max(0.01f, strike.hitSize.z)) * 0.5f;
        }

        public void SetAttackPhase(bool combo, int step, BossStrikeSettings strike, BossAttackPhase phase)
        {
            IsComboAttack = combo;
            AttackStep = step;
            CurrentStrike = strike;
            AttackPhase = phase;
            PhaseProgress = 0f;
            bool blade = !combo && phase == BossAttackPhase.Telegraph;
            bool eyes = combo && step == 3 && phase == BossAttackPhase.Telegraph;
            if (bladeTelegraph != null) bladeTelegraph.SetActive(blade);
            if (eyesTelegraph != null) eyesTelegraph.SetActive(eyes);
            string state = phase == BossAttackPhase.Telegraph ? strike.telegraphState :
                phase == BossAttackPhase.Active ? strike.activeState : strike.recoveryState;
            if (_animator != null && _animator.runtimeAnimatorController != null &&
                !string.IsNullOrEmpty(state) && _animator.HasState(0, Animator.StringToHash(state)))
                _animator.CrossFadeInFixedTime(state, 0.05f);
        }

        public void SetPhaseProgress(float progress) => PhaseProgress = Mathf.Clamp01(progress);

        public void ClearAttackPresentation()
        {
            AttackPhase = BossAttackPhase.None;
            CurrentStrike = null;
            AttackStep = 0;
            PhaseProgress = 0f;
            if (bladeTelegraph != null) bladeTelegraph.SetActive(false);
            if (eyesTelegraph != null) eyesTelegraph.SetActive(false);
            if (_prototypeEyes != null) _prototypeEyes.SetActive(false);
            if (_prototypeRoot != null) _prototypeRoot.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!useGuardianPatterns || !showPrototypeWeapon || _health == null || !_health.IsAlive)
            {
                if (_prototypeRoot != null) _prototypeRoot.gameObject.SetActive(false);
                return;
            }
            if (_prototypeRoot == null) CreatePrototypeWeapon();
            _prototypeRoot.gameObject.SetActive(true);
            _prototypeRoot.SetPositionAndRotation(transform.position, transform.rotation);
            bool telegraph = AttackPhase == BossAttackPhase.Telegraph;
            bool active = AttackPhase == BossAttackPhase.Active;
            bool recovery = AttackPhase == BossAttackPhase.Recovery;
            float t = PhaseProgress;
            Vector3 position = new Vector3(0.65f, 1f, 0.4f);
            Quaternion rotation = Quaternion.Euler(30f, 0f, 0f);
            if (AttackPhase != BossAttackPhase.None && !IsComboAttack)
            {
                position = new Vector3(0f, 1.8f, 0.35f);
                rotation = Quaternion.Euler(telegraph ? -25f : active ? Mathf.Lerp(-25f, 155f, t) : 155f, 0f, 0f);
                if (recovery) position = new Vector3(0f, 1.4f, 1.8f);
            }
            else if (AttackPhase != BossAttackPhase.None && AttackStep < 3)
            {
                float direction = AttackStep == 1 ? 1f : -1f;
                float yaw = (telegraph ? -70f : active ? Mathf.Lerp(-70f, 70f, t) : 70f) * direction;
                position = new Vector3(0f, 1.2f, 0.4f);
                rotation = Quaternion.Euler(90f, yaw, 0f);
            }
            else if (AttackPhase != BossAttackPhase.None)
            {
                position = new Vector3(0f, 1.2f, active ? Mathf.Lerp(0.3f, 1.5f, t) : recovery ? 1.5f : 0.3f);
                rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            _prototypeSword.localPosition = position;
            _prototypeSword.localRotation = rotation;
            Color color = telegraph && !IsComboAttack ? Color.red : new Color(0.65f, 0.75f, 0.85f);
            _cueProperties.SetColor("_BaseColor", color);
            _cueProperties.SetColor("_Color", color);
            _prototypeBlade.SetPropertyBlock(_cueProperties);
            _prototypeEyes.SetActive(telegraph && IsComboAttack && AttackStep == 3);
        }

        private void CreatePrototypeWeapon()
        {
            // Detached, metre-sized debug geometry: never edits the shared prefab or model bones.
            _prototypeRoot = new GameObject("Guardian Prototype Presentation").transform;
            _prototypeSword = new GameObject("Sword Pivot").transform;
            _prototypeSword.SetParent(_prototypeRoot, false);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            _prototypeMaterial = new Material(shader);
            _cueProperties = new MaterialPropertyBlock();
            _prototypeBlade = CreateMarker("Blade", _prototypeSword, new Vector3(0f, 0.8f, 0f),
                new Vector3(0.16f, 1.6f, 0.07f));
            Renderer eyes = CreateMarker("Red Eye Cue", _prototypeRoot, new Vector3(0f, 2f, 0.55f),
                new Vector3(0.4f, 0.12f, 0.12f));
            _cueProperties.SetColor("_BaseColor", Color.red);
            _cueProperties.SetColor("_Color", Color.red);
            eyes.SetPropertyBlock(_cueProperties);
            _prototypeEyes = eyes.gameObject;
        }

        private Renderer CreateMarker(string markerName, Transform parent, Vector3 position, Vector3 size)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = markerName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = size;
            Collider markerCollider = marker.GetComponent<Collider>();
            markerCollider.enabled = false;
            Destroy(markerCollider);
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.sharedMaterial = _prototypeMaterial;
            return markerRenderer;
        }

        private void OnDestroy()
        {
            if (_prototypeRoot != null) Destroy(_prototypeRoot.gameObject);
            if (_prototypeMaterial != null) Destroy(_prototypeMaterial);
        }

        private void OnDrawGizmosSelected()
        {
            if (!useGuardianPatterns) return;
            BossStrikeSettings strike = CurrentStrike ?? overhead;
            Gizmos.color = AttackPhase == BossAttackPhase.Active ? Color.red : Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(strike.hitCenter, GetHitHalfSize(strike) * 2f);
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
