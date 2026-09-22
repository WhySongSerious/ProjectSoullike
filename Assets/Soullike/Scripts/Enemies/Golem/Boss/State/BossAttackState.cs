using UnityEngine;

namespace ProjectSoullike
{
    public sealed class BossAttackState : BossState
    {
        private float _windupRemaining;
        private float _cooldownRemaining;
        private bool _isWindingUp;

        public BossAttackState(BossController boss) : base(boss) { }

        public override BossStateType StateType => BossStateType.Attack;

        public override void Enter()
        {
            Boss.StopMovement();
            StartWindup();
        }

        public override void Update()
        {
            if (!Boss.HasTarget)
            {
                Boss.ChangeState(BossStateType.Idle);
                return;
            }

            if (Boss.DistanceToTarget > Boss.AttackRange)
            {
                Boss.ChangeState(BossStateType.Chase);
                return;
            }

            Boss.FaceTarget();

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= Time.deltaTime;
                if (_cooldownRemaining <= 0f)
                {
                    StartWindup();
                }

                return;
            }

            if (!_isWindingUp)
            {
                StartWindup();
            }

            _windupRemaining -= Time.deltaTime;
            if (_windupRemaining <= 0f)
            {
                PerformAttack();
                _isWindingUp = false;
                _cooldownRemaining = Boss.AttackCooldown;
            }
        }

        public override void Exit()
        {
            _isWindingUp = false;
            _cooldownRemaining = 0f;
        }

        private void StartWindup()
        {
            _isWindingUp = true;
            _windupRemaining = Boss.AttackDelay;
            Boss.PlayAttackAnimation();
        }

        private void PerformAttack()
        {
            if (Boss.DistanceToTarget > Boss.AttackRange)
            {
                return;
            }

            PlayerHealth playerHealth = Boss.GetTargetHealth();
            if (playerHealth == null || !playerHealth.IsAlive)
            {
                return;
            }

            Vector3 direction = playerHealth.transform.position - Boss.transform.position;
            playerHealth.TakeDamage(new DamageInfo(
                Boss.AttackDamage,
                playerHealth.transform.position,
                direction,
                Boss.gameObject,
                attackIndex: 0));
        }
    }
}
