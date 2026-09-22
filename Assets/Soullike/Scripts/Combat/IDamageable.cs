using UnityEngine;

namespace ProjectSoullike
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            float amount,
            Vector3 point,
            Vector3 direction,
            GameObject source,
            int attackIndex)
        {
            Amount = Mathf.Max(0f, amount);
            Point = point;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
            Source = source;
            AttackIndex = attackIndex;
        }

        public float Amount { get; }
        public Vector3 Point { get; }
        public Vector3 Direction { get; }
        public GameObject Source { get; }
        public int AttackIndex { get; }
    }

    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(DamageInfo damageInfo);
    }

}
