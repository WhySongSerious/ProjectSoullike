using System;
using System.Collections;
using UnityEngine;

namespace ProjectSoullike
{
    [DisallowMultipleComponent]
    public sealed class GolemHealth : MonoBehaviour, IDamageable
    {
        [Header("Temporary Health")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float deactivateDelay = 1f;

        [Header("Temporary World Health Bar")]
        [SerializeField] private bool showHealthBar = true;
        [SerializeField, Min(40f)] private float healthBarWidth = 150f;
        [SerializeField, Min(4f)] private float healthBarHeight = 16f;
        [SerializeField, Min(0f)] private float healthBarWorldPadding = 0.35f;
        [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.85f, 0.25f, 1f);
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.15f, 0.1f, 1f);

        private Collider[] _combatColliders;
        private Renderer[] _renderers;
        private Animator _animator;
        private float _animatorSpeed = 1f;
        private float _currentHealth;
        private bool _deactivatedByDeath;
        private GUIStyle _healthLabelStyle;

        public event Action<float, float> HealthChanged;
        public event Action<DamageInfo> DamageTaken;
        public event Action Died;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive { get; private set; }
        public Vector3 TargetPoint => TryGetWorldRendererBounds(out Bounds bounds)
            ? bounds.center
            : transform.position + Vector3.up;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            _animator = GetComponentInChildren<Animator>(includeInactive: true);
            if (_animator != null)
            {
                _animatorSpeed = _animator.speed;
            }

            EnsureHitCollider();
            _combatColliders = GetComponentsInChildren<Collider>(includeInactive: true);
            ResetHealth();
        }

        private void OnEnable()
        {
            if (_deactivatedByDeath)
            {
                ResetHealth();
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive || damageInfo.Amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - damageInfo.Amount);
            DamageTaken?.Invoke(damageInfo);
            HealthChanged?.Invoke(_currentHealth, maxHealth);

            Debug.Log(
                $"[Golem Health] Hit {damageInfo.AttackIndex}: " +
                $"-{damageInfo.Amount:F0} HP ({_currentHealth:F0}/{maxHealth:F0})",
                this);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        public void ResetHealth()
        {
            StopAllCoroutines();
            _deactivatedByDeath = false;
            _currentHealth = maxHealth;
            IsAlive = true;

            if (_combatColliders != null)
            {
                foreach (Collider combatCollider in _combatColliders)
                {
                    if (combatCollider != null)
                    {
                        combatCollider.enabled = true;
                    }
                }
            }

            if (_animator != null)
            {
                _animator.speed = _animatorSpeed;
            }

            HealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void Die()
        {
            IsAlive = false;
            Died?.Invoke();

            if (_combatColliders != null)
            {
                foreach (Collider combatCollider in _combatColliders)
                {
                    if (combatCollider != null)
                    {
                        combatCollider.enabled = false;
                    }
                }
            }

            if (_animator != null)
            {
                _animator.speed = 0f;
            }

            Debug.Log("[Golem Health] Golem defeated.", this);

            if (deactivateDelay <= 0f)
            {
                DeactivateAfterDeath();
            }
            else
            {
                StartCoroutine(DeactivateAfterDelay());
            }
        }

        private IEnumerator DeactivateAfterDelay()
        {
            yield return new WaitForSeconds(deactivateDelay);
            DeactivateAfterDeath();
        }

        private void DeactivateAfterDeath()
        {
            _deactivatedByDeath = true;
            gameObject.SetActive(false);
        }

        private void EnsureHitCollider()
        {
            if (GetComponentInChildren<Collider>(includeInactive: true) != null)
            {
                return;
            }

            if (!TryGetLocalRendererBounds(out Bounds localBounds))
            {
                Debug.LogWarning(
                    "[Golem Health] Could not generate a hit collider because no Renderer was found.",
                    this);
                return;
            }

            BoxCollider hitCollider = gameObject.AddComponent<BoxCollider>();
            hitCollider.center = localBounds.center;
            hitCollider.size = localBounds.size;
        }

        private bool TryGetLocalRendererBounds(out Bounds localBounds)
        {
            localBounds = default;
            bool initialized = false;

            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                Bounds worldBounds = targetRenderer.bounds;
                Vector3 min = worldBounds.min;
                Vector3 max = worldBounds.max;

                for (int x = 0; x <= 1; x++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        for (int z = 0; z <= 1; z++)
                        {
                            Vector3 worldCorner = new Vector3(
                                x == 0 ? min.x : max.x,
                                y == 0 ? min.y : max.y,
                                z == 0 ? min.z : max.z);
                            Vector3 localCorner = transform.InverseTransformPoint(worldCorner);

                            if (!initialized)
                            {
                                localBounds = new Bounds(localCorner, Vector3.zero);
                                initialized = true;
                            }
                            else
                            {
                                localBounds.Encapsulate(localCorner);
                            }
                        }
                    }
                }
            }

            return initialized;
        }

        private bool TryGetWorldRendererBounds(out Bounds worldBounds)
        {
            worldBounds = default;
            bool initialized = false;

            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null || !targetRenderer.enabled)
                {
                    continue;
                }

                if (!initialized)
                {
                    worldBounds = targetRenderer.bounds;
                    initialized = true;
                }
                else
                {
                    worldBounds.Encapsulate(targetRenderer.bounds);
                }
            }

            return initialized;
        }

        private void OnGUI()
        {
            if (!showHealthBar || !TryGetWorldRendererBounds(out Bounds bounds))
            {
                return;
            }

            Camera targetCamera = Camera.main;
            if (targetCamera == null)
            {
                return;
            }

            Vector3 worldPosition = new Vector3(
                bounds.center.x,
                bounds.max.y + healthBarWorldPadding,
                bounds.center.z);
            Vector3 screenPosition = targetCamera.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f)
            {
                return;
            }

            float uiScale = Mathf.Clamp(Screen.height / 1080f, 0.7f, 1.4f);
            float width = healthBarWidth * uiScale;
            float height = healthBarHeight * uiScale;
            Rect backgroundRect = new Rect(
                screenPosition.x - width * 0.5f,
                Screen.height - screenPosition.y - height * 0.5f,
                width,
                height);
            Rect fillRect = new Rect(
                backgroundRect.x + 2f,
                backgroundRect.y + 2f,
                Mathf.Max(0f, (backgroundRect.width - 4f) * (_currentHealth / maxHealth)),
                Mathf.Max(0f, backgroundRect.height - 4f));

            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
            GUI.color = Color.Lerp(lowHealthColor, fullHealthColor, _currentHealth / maxHealth);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (_healthLabelStyle == null)
            {
                _healthLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(11f * uiScale)
                };
                _healthLabelStyle.normal.textColor = Color.white;
            }

            GUI.Label(
                backgroundRect,
                $"{Mathf.CeilToInt(_currentHealth)} / {Mathf.CeilToInt(maxHealth)}",
                _healthLabelStyle);
            GUI.color = previousColor;
        }
    }
}
