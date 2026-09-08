using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSoullike
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class KeyboardPlayerController : MonoBehaviour
    {
        private const string PlayerActionMapName = "Player";
        private const string MoveActionName = "Move";
        private const string LookActionName = "Look";
        private const string AttackActionName = "Attack";
        private const string SprintActionName = "Sprint";
        private const string LockOnActionName = "LockOn";

        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int LocomotionPathHash = Animator.StringToHash("Base Layer.Locomotion");
        private static readonly int LocomotionShortHash = Animator.StringToHash("Locomotion");

        private static readonly int[] AttackPathHashes =
        {
            Animator.StringToHash("Base Layer.Attack 1"),
            Animator.StringToHash("Base Layer.Attack 2"),
            Animator.StringToHash("Base Layer.Attack 3")
        };

        private static readonly int[] AttackShortHashes =
        {
            Animator.StringToHash("Attack 1"),
            Animator.StringToHash("Attack 2"),
            Animator.StringToHash("Attack 3")
        };

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 2.2f;
        [SerializeField, Min(0f)] private float runSpeed = 4.8f;
        [SerializeField, Range(0f, 1f)] private float attackMovementMultiplier = 0.15f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;
        [SerializeField] private float gravity = -25f;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Min(0f)] private float gamepadLookSpeed = 150f;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float animationDampTime = 0.1f;
        [SerializeField, Min(0f)] private float attackBlendDuration = 0.06f;
        [SerializeField, Range(0f, 1f)] private float comboInputOpen = 0.45f;
        [SerializeField, Range(0f, 1f)] private float comboInputClose = 0.92f;
        [SerializeField, Range(0f, 1f)] private float comboAdvanceTime = 0.9f;
        [SerializeField, Range(0f, 1.2f)] private float attackEndTime = 0.98f;
        [SerializeField, Min(0f)] private float recoveryBlendDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 0.26f;

        [Header("Combat Hit Detection")]
        [SerializeField, Min(0f)] private float attackOneDamage = 20f;
        [SerializeField, Min(0f)] private float attackTwoDamage = 30f;
        [SerializeField, Min(0f)] private float attackThreeDamage = 50f;
        [SerializeField, Range(0f, 1f)] private float attackHitWindowOpen = 0.22f;
        [SerializeField, Range(0f, 1f)] private float attackHitWindowClose = 0.68f;
        [SerializeField, Min(0f)] private float attackHitHeight = 1.05f;
        [SerializeField, Min(0f)] private float attackHitDistance = 1.05f;
        [SerializeField, Min(0.05f)] private float attackHitRadius = 0.85f;
        [SerializeField] private LayerMask attackHitMask = ~0;

        [Header("Lock On")]
        [SerializeField, Min(0.1f)] private float lockOnRange = 18f;
        [SerializeField, Range(0f, 180f)] private float lockOnAcquisitionAngle = 75f;
        [SerializeField, Min(0f)] private float lockOnCharacterTurnSpeed = 900f;
        [SerializeField, Min(0f)] private float lockOnCameraTurnSpeed = 360f;
        [SerializeField] private float lockOnCameraPitch = 18f;
        [SerializeField, Range(0f, 1f)] private float lockOnCameraFocusWeight = 0.6f;
        [SerializeField] private bool showLockOnIndicator = true;
        [SerializeField, Min(8f)] private float lockOnIndicatorSize = 28f;
        [SerializeField] private Color lockOnIndicatorColor = new Color(1f, 0.85f, 0.2f, 1f);

        [Header("Soullike Camera")]
        [SerializeField] private bool controlMainCamera = true;
        [SerializeField, Min(0f)] private float cameraPivotHeight = 1.45f;
        [SerializeField, Min(0.1f)] private float cameraDistance = 4.6f;
        [SerializeField] private float cameraShoulderOffset = 0.35f;
        [SerializeField, Min(0f)] private float cameraFollowSpeed = 14f;
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [SerializeField] private float initialCameraPitch = 18f;
        [SerializeField] private float minimumCameraPitch = -10f;
        [SerializeField] private float maximumCameraPitch = 55f;
        [SerializeField, Min(0f)] private float cameraCollisionRadius = 0.22f;
        [SerializeField, Min(0.1f)] private float minimumCameraDistance = 0.7f;
        [SerializeField] private LayerMask cameraCollisionMask = ~0;
        [SerializeField] private bool lockCursorOnStart = true;

        private CharacterController _characterController;
        private Animator _animator;
        private Camera _mainCamera;
        private InputActionMap _playerActionMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _sprintAction;
        private InputAction _lockOnAction;
        private readonly Collider[] _attackHitBuffer = new Collider[24];
        private readonly HashSet<IDamageable> _damagedTargetsThisAttack = new HashSet<IDamageable>();
        private GolemHealth _lockOnTarget;
        private float _verticalVelocity;
        private float _cameraYaw;
        private float _cameraPitch;
        private int _attackStep = -1;
        private bool _attackQueued;
        private bool _isRecovering;
        private float _recoveryElapsed;

        private bool IsAttacking => _attackStep >= 0;
        public bool IsLockedOn => HasValidLockOnTarget();
        public Transform LockOnTarget => IsLockedOn ? _lockOnTarget.transform : null;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>(includeInactive: true);
            _mainCamera = Camera.main;

            if (_animator == null)
            {
                Debug.LogError("KeyboardPlayerController requires an Animator in the player prefab.", this);
                enabled = false;
                return;
            }

            if (!InitializeInputActions())
            {
                enabled = false;
                return;
            }

            _animator.applyRootMotion = false;
            _cameraYaw = transform.eulerAngles.y;
            _cameraPitch = Mathf.Clamp(initialCameraPitch, minimumCameraPitch, maximumCameraPitch);

            if (lockCursorOnStart)
            {
                SetCursorLocked(true);
            }

            ValidateAnimatorStates();
        }

        private void OnEnable()
        {
            _playerActionMap?.Enable();
        }

        private void OnDisable()
        {
            _playerActionMap?.Disable();
            ClearLockOnTarget();

            if (Application.isPlaying)
            {
                SetCursorLocked(false);
            }
        }

        private void Update()
        {
            bool wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
            UpdateLockOnState(wasCursorLocked);
            HandleCursorAndCameraInput();

            bool attackPressed = _attackAction.WasPressedThisFrame();
            if (_attackAction.activeControl?.device is Mouse && !wasCursorLocked)
            {
                attackPressed = false;
            }

            UpdateAttackCombo(attackPressed);

            Vector2 input = Vector2.ClampMagnitude(_moveAction.ReadValue<Vector2>(), 1f);
            bool isRunning = _sprintAction.IsPressed();

            MoveCharacter(input, isRunning);
        }

        private void LateUpdate()
        {
            if (!controlMainCamera)
            {
                return;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera == null)
            {
                return;
            }

            Vector3 pivot = transform.position + Vector3.up * cameraPivotHeight;
            UpdateLockOnCameraAngles(pivot);
            Quaternion orbitRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
            Vector3 orbitOffset = orbitRotation * new Vector3(cameraShoulderOffset, 0f, -cameraDistance);
            Vector3 desiredPosition = ResolveCameraCollision(pivot, pivot + orbitOffset);
            float blend = 1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime);

            _mainCamera.transform.position = Vector3.Lerp(
                _mainCamera.transform.position,
                desiredPosition,
                blend);

            Vector3 cameraLookPoint = IsLockedOn
                ? Vector3.Lerp(pivot, _lockOnTarget.TargetPoint, lockOnCameraFocusWeight)
                : pivot;
            Vector3 lookDirection = cameraLookPoint - _mainCamera.transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                _mainCamera.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        private void MoveCharacter(Vector2 input, bool isRunning)
        {
            Vector3 moveDirection = GetCameraRelativeDirection(input);
            float inputMagnitude = Mathf.Clamp01(input.magnitude);
            float movementSpeed = (isRunning ? runSpeed : walkSpeed) * inputMagnitude;

            if (IsAttacking)
            {
                movementSpeed *= attackMovementMultiplier;
            }

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = moveDirection * movementSpeed + Vector3.up * _verticalVelocity;
            _characterController.Move(velocity * Time.deltaTime);

            if (IsLockedOn)
            {
                RotateTowardsLockOnTarget();
            }
            else if (moveDirection.sqrMagnitude > 0.001f)
            {
                float currentRotationSpeed = IsAttacking ? rotationSpeed * 0.25f : rotationSpeed;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    currentRotationSpeed * Time.deltaTime);
            }

            float animationSpeed = _isRecovering || inputMagnitude <= 0.01f
                ? 0f
                : inputMagnitude * (isRunning ? 1f : 0.5f);
            _animator.SetFloat(MoveSpeedHash, animationSpeed, animationDampTime, Time.deltaTime);
        }

        private void UpdateAttackCombo(bool attackPressed)
        {
            if (_isRecovering)
            {
                UpdateRecovery();
                return;
            }

            if (!IsAttacking)
            {
                if (attackPressed)
                {
                    StartAttack(0);
                }

                return;
            }

            if (!TryGetCurrentAttackNormalizedTime(out float normalizedTime))
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
                if (!_animator.IsInTransition(0) && currentState.shortNameHash == LocomotionShortHash)
                {
                    ResetAttackState();
                }

                return;
            }

            UpdateAttackHitDetection(normalizedTime);

            bool hasNextAttack = _attackStep < AttackPathHashes.Length - 1;
            if (attackPressed && hasNextAttack &&
                normalizedTime >= comboInputOpen && normalizedTime <= comboInputClose)
            {
                _attackQueued = true;
            }

            if (_attackQueued && hasNextAttack && normalizedTime >= comboAdvanceTime)
            {
                StartAttack(_attackStep + 1);
                return;
            }

            if (normalizedTime >= attackEndTime)
            {
                if (_attackStep < AttackPathHashes.Length - 1)
                {
                    StartRecovery();
                }
                else
                {
                    EndAttack();
                }
            }
        }

        private void StartAttack(int step)
        {
            _attackStep = Mathf.Clamp(step, 0, AttackPathHashes.Length - 1);
            _attackQueued = false;
            _isRecovering = false;
            _recoveryElapsed = 0f;
            _damagedTargetsThisAttack.Clear();
            _animator.CrossFadeInFixedTime(AttackPathHashes[_attackStep], attackBlendDuration, 0, 0f);
        }

        private void StartRecovery()
        {
            _attackQueued = false;
            _isRecovering = true;
            _recoveryElapsed = 0f;
            _animator.CrossFadeInFixedTime(
                LocomotionPathHash,
                recoveryBlendDuration,
                0,
                0f);
        }

        private void UpdateRecovery()
        {
            _recoveryElapsed += Time.deltaTime;
            if (_recoveryElapsed >= recoveryDuration)
            {
                ResetAttackState();
            }
        }

        private void EndAttack()
        {
            _animator.CrossFadeInFixedTime(LocomotionPathHash, attackBlendDuration, 0, 0f);
            ResetAttackState();
        }

        private void ResetAttackState()
        {
            _attackStep = -1;
            _attackQueued = false;
            _isRecovering = false;
            _recoveryElapsed = 0f;
        }

        private bool TryGetCurrentAttackNormalizedTime(out float normalizedTime)
        {
            int expectedShortHash = AttackShortHashes[_attackStep];
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

            if (currentState.shortNameHash == expectedShortHash)
            {
                normalizedTime = currentState.normalizedTime;
                return true;
            }

            if (_animator.IsInTransition(0))
            {
                AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
                if (nextState.shortNameHash == expectedShortHash)
                {
                    normalizedTime = nextState.normalizedTime;
                    return true;
                }
            }

            normalizedTime = 0f;
            return false;
        }

        private void UpdateAttackHitDetection(float normalizedTime)
        {
            if (normalizedTime < attackHitWindowOpen || normalizedTime > attackHitWindowClose)
            {
                return;
            }

            Vector3 hitCenter = GetAttackHitCenter();
            int hitCount = Physics.OverlapSphereNonAlloc(
                hitCenter,
                attackHitRadius,
                _attackHitBuffer,
                attackHitMask,
                QueryTriggerInteraction.Ignore);

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider = _attackHitBuffer[index];
                _attackHitBuffer[index] = null;
                IDamageable damageable = FindDamageable(hitCollider);

                if (damageable == null || !damageable.IsAlive ||
                    !_damagedTargetsThisAttack.Add(damageable))
                {
                    continue;
                }

                Vector3 hitPoint = hitCollider.ClosestPoint(hitCenter);
                Vector3 hitDirection = hitCollider.bounds.center - transform.position;
                DamageInfo damageInfo = new DamageInfo(
                    GetCurrentAttackDamage(),
                    hitPoint,
                    hitDirection,
                    gameObject,
                    _attackStep + 1);
                damageable.TakeDamage(damageInfo);
            }
        }

        private Vector3 GetAttackHitCenter()
        {
            return transform.position +
                Vector3.up * attackHitHeight +
                transform.forward * attackHitDistance;
        }

        private float GetCurrentAttackDamage()
        {
            return _attackStep switch
            {
                0 => attackOneDamage,
                1 => attackTwoDamage,
                _ => attackThreeDamage
            };
        }

        private static IDamageable FindDamageable(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return null;
            }

            MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(includeInactive: false);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IDamageable damageable)
                {
                    return damageable;
                }
            }

            return null;
        }

        private void UpdateLockOnState(bool wasCursorLocked)
        {
            if (_lockOnTarget != null && !HasValidLockOnTarget())
            {
                ClearLockOnTarget();
            }

            bool togglePressed = _lockOnAction.WasPressedThisFrame();
            if (_lockOnAction.activeControl?.device is Mouse && !wasCursorLocked)
            {
                togglePressed = false;
            }

            if (!togglePressed)
            {
                return;
            }

            if (_lockOnTarget != null)
            {
                ClearLockOnTarget();
                return;
            }

            _lockOnTarget = FindBestLockOnTarget();
        }

        private GolemHealth FindBestLockOnTarget()
        {
            GolemHealth[] candidates = FindObjectsByType<GolemHealth>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            Vector3 viewOrigin = _mainCamera == null
                ? transform.position + Vector3.up * cameraPivotHeight
                : _mainCamera.transform.position;
            Vector3 viewForward = _mainCamera == null
                ? transform.forward
                : _mainCamera.transform.forward;
            GolemHealth bestTarget = null;
            float bestScore = float.PositiveInfinity;

            foreach (GolemHealth candidate in candidates)
            {
                if (candidate == null || !candidate.isActiveAndEnabled || !candidate.IsAlive)
                {
                    continue;
                }

                Vector3 toTarget = candidate.TargetPoint - viewOrigin;
                float distanceFromPlayer = Vector3.Distance(transform.position, candidate.transform.position);
                if (toTarget.sqrMagnitude <= 0.001f || distanceFromPlayer > lockOnRange)
                {
                    continue;
                }

                float viewAngle = Vector3.Angle(viewForward, toTarget);
                if (viewAngle > lockOnAcquisitionAngle)
                {
                    continue;
                }

                float score = viewAngle + (distanceFromPlayer / lockOnRange) * 15f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
        }

        private bool HasValidLockOnTarget()
        {
            return _lockOnTarget != null &&
                   _lockOnTarget.isActiveAndEnabled &&
                   _lockOnTarget.IsAlive &&
                   Vector3.Distance(transform.position, _lockOnTarget.transform.position) <= lockOnRange;
        }

        private void ClearLockOnTarget()
        {
            _lockOnTarget = null;
        }

        private void RotateTowardsLockOnTarget()
        {
            Vector3 targetDirection = _lockOnTarget.TargetPoint - transform.position;
            targetDirection.y = 0f;
            if (targetDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                lockOnCharacterTurnSpeed * Time.deltaTime);
        }

        private void UpdateLockOnCameraAngles(Vector3 pivot)
        {
            if (!IsLockedOn)
            {
                return;
            }

            Vector3 targetDirection = _lockOnTarget.TargetPoint - pivot;
            targetDirection.y = 0f;
            if (targetDirection.sqrMagnitude > 0.001f)
            {
                float targetYaw = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
                _cameraYaw = Mathf.MoveTowardsAngle(
                    _cameraYaw,
                    targetYaw,
                    lockOnCameraTurnSpeed * Time.deltaTime);
            }

            float targetPitch = Mathf.Clamp(
                lockOnCameraPitch,
                minimumCameraPitch,
                maximumCameraPitch);
            _cameraPitch = Mathf.MoveTowards(
                _cameraPitch,
                targetPitch,
                lockOnCameraTurnSpeed * Time.deltaTime);
        }

        private void HandleCursorAndCameraInput()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
            }
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame &&
                     Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorLocked(true);
            }

            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                _cameraYaw = transform.eulerAngles.y;
                _cameraPitch = Mathf.Clamp(initialCameraPitch, minimumCameraPitch, maximumCameraPitch);
            }

            if (!controlMainCamera)
            {
                return;
            }

            if (IsLockedOn)
            {
                return;
            }

            Vector2 lookInput = _lookAction.ReadValue<Vector2>();
            InputDevice lookDevice = _lookAction.activeControl?.device;
            bool isStickInput = lookDevice is Gamepad || lookDevice is Joystick;

            if (!isStickInput && Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            float lookScale = isStickInput
                ? gamepadLookSpeed * Time.unscaledDeltaTime
                : mouseSensitivity;
            _cameraYaw += lookInput.x * lookScale;
            _cameraPitch = Mathf.Clamp(
                _cameraPitch - lookInput.y * lookScale,
                minimumCameraPitch,
                maximumCameraPitch);
        }

        private Vector3 ResolveCameraCollision(Vector3 pivot, Vector3 desiredPosition)
        {
            Vector3 direction = desiredPosition - pivot;
            float desiredDistance = direction.magnitude;
            if (desiredDistance <= 0.001f)
            {
                return desiredPosition;
            }

            direction /= desiredDistance;
            float resolvedDistance = desiredDistance;
            RaycastHit[] hits = Physics.SphereCastAll(
                pivot,
                cameraCollisionRadius,
                direction,
                desiredDistance,
                cameraCollisionMask,
                QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                resolvedDistance = Mathf.Min(resolvedDistance, hit.distance - cameraCollisionRadius);
            }

            resolvedDistance = Mathf.Clamp(resolvedDistance, minimumCameraDistance, desiredDistance);
            return pivot + direction * resolvedDistance;
        }

        private bool InitializeInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogError(
                    "KeyboardPlayerController requires the project Input Action Asset.",
                    this);
                return false;
            }

            _playerActionMap = inputActions.FindActionMap(PlayerActionMapName, throwIfNotFound: false);
            if (_playerActionMap == null)
            {
                Debug.LogError(
                    $"Input Action Asset is missing the '{PlayerActionMapName}' action map.",
                    this);
                return false;
            }

            _moveAction = _playerActionMap.FindAction(MoveActionName, throwIfNotFound: false);
            _lookAction = _playerActionMap.FindAction(LookActionName, throwIfNotFound: false);
            _attackAction = _playerActionMap.FindAction(AttackActionName, throwIfNotFound: false);
            _sprintAction = _playerActionMap.FindAction(SprintActionName, throwIfNotFound: false);
            _lockOnAction = _playerActionMap.FindAction(LockOnActionName, throwIfNotFound: false);

            if (_moveAction == null || _lookAction == null ||
                _attackAction == null ||
                _sprintAction == null ||
                _lockOnAction == null)
            {
                Debug.LogError(
                    "Player action map requires Move, Look, Attack, Sprint, and LockOn actions.",
                    this);
                return false;
            }

            return true;
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= 0f)
            {
                return Vector3.zero;
            }

            Transform cameraTransform = _mainCamera == null ? null : _mainCamera.transform;
            Vector3 forward = cameraTransform == null ? Vector3.forward : cameraTransform.forward;
            Vector3 right = cameraTransform == null ? Vector3.right : cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }

        private void ValidateAnimatorStates()
        {
            foreach (int attackStateHash in AttackPathHashes)
            {
                if (!_animator.HasState(0, attackStateHash))
                {
                    Debug.LogWarning(
                        "Player Animator is missing one or more Attack 1/2/3 states. " +
                        "Run Tools > Project Soullike > Set Up Player Combo and Camera.",
                        this);
                    return;
                }
            }

        }

        private void OnGUI()
        {
            if (!showLockOnIndicator || !IsLockedOn || _mainCamera == null)
            {
                return;
            }

            Vector3 screenPoint = _mainCamera.WorldToScreenPoint(_lockOnTarget.TargetPoint);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            float halfSize = lockOnIndicatorSize * 0.5f;
            float cornerLength = lockOnIndicatorSize * 0.28f;
            float thickness = 2f;
            float left = screenPoint.x - halfSize;
            float top = Screen.height - screenPoint.y - halfSize;
            float right = screenPoint.x + halfSize;
            float bottom = Screen.height - screenPoint.y + halfSize;
            Color previousColor = GUI.color;
            GUI.color = lockOnIndicatorColor;

            GUI.DrawTexture(new Rect(left, top, cornerLength, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(left, top, thickness, cornerLength), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(right - cornerLength, top, cornerLength, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(right - thickness, top, thickness, cornerLength), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(left, bottom - thickness, cornerLength, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(left, bottom - cornerLength, thickness, cornerLength), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(right - cornerLength, bottom - thickness, cornerLength, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(right - thickness, bottom - cornerLength, thickness, cornerLength), Texture2D.whiteTexture);

            GUI.color = previousColor;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.35f);
            Gizmos.DrawSphere(GetAttackHitCenter(), attackHitRadius);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
