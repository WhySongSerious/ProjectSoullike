using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSoullike
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class KeyboardPlayerController : MonoBehaviour
    {
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

        private static readonly int[] RecoveryPathHashes =
        {
            Animator.StringToHash("Base Layer.Recovery 1"),
            Animator.StringToHash("Base Layer.Recovery 2")
        };

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 2.2f;
        [SerializeField, Min(0f)] private float runSpeed = 4.8f;
        [SerializeField, Range(0f, 1f)] private float attackMovementMultiplier = 0.15f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;
        [SerializeField] private float gravity = -25f;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float animationDampTime = 0.1f;
        [SerializeField, Min(0f)] private float attackBlendDuration = 0.06f;
        [SerializeField, Range(0f, 1f)] private float comboInputOpen = 0.45f;
        [SerializeField, Range(0f, 1f)] private float comboInputClose = 0.92f;
        [SerializeField, Range(0f, 1f)] private float comboAdvanceTime = 0.9f;
        [SerializeField, Range(0f, 1.2f)] private float attackEndTime = 0.98f;
        [SerializeField, Min(0f)] private float recoveryBlendDuration = 0.04f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 0.56f;

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
        private float _verticalVelocity;
        private float _cameraYaw;
        private float _cameraPitch;
        private int _attackStep = -1;
        private bool _attackQueued;
        private bool _isRecovering;
        private float _recoveryElapsed;

        private bool IsAttacking => _attackStep >= 0;

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

            _animator.applyRootMotion = false;
            _cameraYaw = transform.eulerAngles.y;
            _cameraPitch = Mathf.Clamp(initialCameraPitch, minimumCameraPitch, maximumCameraPitch);

            if (lockCursorOnStart)
            {
                SetCursorLocked(true);
            }

            ValidateAnimatorStates();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                SetCursorLocked(false);
            }
        }

        private void Update()
        {
            HandleCursorAndCameraInput();

            Keyboard keyboard = Keyboard.current;
            bool attackPressed = keyboard != null && keyboard.jKey.wasPressedThisFrame;
            UpdateAttackCombo(attackPressed);

            Vector2 input = keyboard == null ? Vector2.zero : ReadMovementInput(keyboard);
            bool isRunning = keyboard != null &&
                (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

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
            Quaternion orbitRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
            Vector3 orbitOffset = orbitRotation * new Vector3(cameraShoulderOffset, 0f, -cameraDistance);
            Vector3 desiredPosition = ResolveCameraCollision(pivot, pivot + orbitOffset);
            float blend = 1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime);

            _mainCamera.transform.position = Vector3.Lerp(
                _mainCamera.transform.position,
                desiredPosition,
                blend);

            Vector3 lookDirection = pivot - _mainCamera.transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                _mainCamera.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        private void MoveCharacter(Vector2 input, bool isRunning)
        {
            Vector3 moveDirection = GetCameraRelativeDirection(input);
            float movementSpeed = isRunning ? runSpeed : walkSpeed;

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

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                float currentRotationSpeed = IsAttacking ? rotationSpeed * 0.25f : rotationSpeed;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    currentRotationSpeed * Time.deltaTime);
            }

            float animationSpeed = input.sqrMagnitude > 0f ? (isRunning ? 1f : 0.5f) : 0f;
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
                if (_attackStep < RecoveryPathHashes.Length)
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
            _animator.CrossFadeInFixedTime(AttackPathHashes[_attackStep], attackBlendDuration, 0, 0f);
        }

        private void StartRecovery()
        {
            _attackQueued = false;
            _isRecovering = true;
            _recoveryElapsed = 0f;
            _animator.CrossFade(
                RecoveryPathHashes[_attackStep],
                recoveryBlendDuration,
                0,
                1f);
        }

        private void UpdateRecovery()
        {
            _recoveryElapsed += Time.deltaTime;
            if (_recoveryElapsed >= recoveryDuration)
            {
                EndAttack();
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

            if (!controlMainCamera || mouse == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2 mouseDelta = mouse.delta.ReadValue();
            _cameraYaw += mouseDelta.x * mouseSensitivity;
            _cameraPitch = Mathf.Clamp(
                _cameraPitch - mouseDelta.y * mouseSensitivity,
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

        private static Vector2 ReadMovementInput(Keyboard keyboard)
        {
            Vector2 input = Vector2.zero;

            if (keyboard.wKey.isPressed)
            {
                input.y += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                input.y -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                input.x += 1f;
            }

            if (keyboard.aKey.isPressed)
            {
                input.x -= 1f;
            }

            return Vector2.ClampMagnitude(input, 1f);
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

            foreach (int recoveryStateHash in RecoveryPathHashes)
            {
                if (!_animator.HasState(0, recoveryStateHash))
                {
                    Debug.LogWarning(
                        "Player Animator is missing one or more Recovery 1/2 states. " +
                        "Run Tools > Project Soullike > Set Up Player Combo and Camera.",
                        this);
                    return;
                }
            }
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
