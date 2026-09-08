using System;
using UnityEngine;

namespace ProjectSoullike
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        [Header("Temporary Health Bar")]
        [SerializeField] private bool showHealthBar = true;
        [SerializeField, Min(80f)] private float healthBarWidth = 240f;
        [SerializeField, Min(4f)] private float healthBarHeight = 18f;

        private float _currentHealth;
        private GUIStyle _labelStyle;

        public event Action<float, float> HealthChanged;
        public event Action<DamageInfo> DamageTaken;
        public event Action Died;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive { get; private set; }

        private void Awake()
        {
            ResetHealth();
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
                $"[Player Health] -{damageInfo.Amount:F0} HP " +
                $"({_currentHealth:F0}/{maxHealth:F0})",
                this);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            IsAlive = true;
            KeyboardPlayerController controller = GetComponent<KeyboardPlayerController>();
            if (controller != null)
            {
                controller.enabled = true;
            }

            HealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void Die()
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            KeyboardPlayerController controller = GetComponent<KeyboardPlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            Died?.Invoke();
            Debug.Log("[Player Health] Player defeated.", this);
        }

        private void OnGUI()
        {
            if (!showHealthBar)
            {
                return;
            }

            float uiScale = Mathf.Clamp(Screen.height / 1080f, 0.7f, 1.4f);
            float width = healthBarWidth * uiScale;
            float height = healthBarHeight * uiScale;
            Rect backgroundRect = new Rect(24f * uiScale, 24f * uiScale, width, height);
            Rect fillRect = new Rect(
                backgroundRect.x + 2f,
                backgroundRect.y + 2f,
                Mathf.Max(0f, (backgroundRect.width - 4f) * (_currentHealth / maxHealth)),
                Mathf.Max(0f, backgroundRect.height - 4f));

            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.75f, 0.08f, 0.08f, 1f);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(11f * uiScale)
                };
                _labelStyle.normal.textColor = Color.white;
            }

            GUI.Label(
                backgroundRect,
                $"PLAYER  {Mathf.CeilToInt(_currentHealth)} / {Mathf.CeilToInt(maxHealth)}",
                _labelStyle);
            GUI.color = previousColor;
        }
    }
}
