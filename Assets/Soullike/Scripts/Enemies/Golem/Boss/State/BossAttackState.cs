using System.Collections.Generic;
using UnityEngine;

namespace ProjectSoullike
{
    public sealed class BossAttackState : BossState
    {
        private float _windupRemaining;
        private float _cooldownRemaining;
        private bool _isWindingUp;
        private readonly HashSet<PlayerHealth> _hitTargets = new HashSet<PlayerHealth>();
        private bool _combo;
        private int _step;
        private int _stepCount;
        private float _phaseRemaining;
        private float _phaseDuration;
        private BossStrikeSettings _strike;
        private PlayerHealth _attackTarget;

        public BossAttackState(BossController boss) : base(boss) { }

        public override BossStateType StateType => BossStateType.Attack;

        public override void Enter()
        {
            Boss.StopMovement();
            if (Boss.UseGuardianPatterns) StartPattern();
            else StartWindup();
        }

        public override void Update()
        {
            if (Boss.UseGuardianPatterns)
            {
                UpdatePattern(Time.deltaTime);
                return;
            }
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
            _hitTargets.Clear();
            _attackTarget = null;
            Boss.ClearAttackPresentation();
        }

        private void StartPattern()
        {
            _combo = Boss.ComboProbability >= 1f || Random.value < Boss.ComboProbability;
            bool thirdStrike = Boss.ThirdStrikeProbability >= 1f || Random.value < Boss.ThirdStrikeProbability;
            _stepCount = !_combo ? 1 : thirdStrike ? 3 : 2;
            _step = 1;
            _attackTarget = Boss.GetTargetHealth();
            StartStrike();
        }

        private void StartStrike()
        {
            _strike = Boss.GetStrike(_combo, _step);
            _hitTargets.Clear();
            // Lock direction when the tell begins. No homing during the hit or recovery.
            Boss.AimAttack();
            BeginPhase(BossAttackPhase.Telegraph, _strike.telegraph);
        }

        private void BeginPhase(BossAttackPhase phase, float duration)
        {
            _phaseDuration = Mathf.Max(0.01f, duration);
            _phaseRemaining = _phaseDuration;
            Boss.SetAttackPhase(_combo, _step, _strike, phase);
        }

        private void UpdatePattern(float deltaTime)
        {
            // A missing/dead/replaced target cancels the sequence. Merely dodging out of
            // reach does not cancel its recovery or immediately start another attack.
            if (!Boss.HasTarget || _attackTarget == null || !_attackTarget.IsAlive ||
                Boss.GetTargetHealth() != _attackTarget)
            {
                Boss.ChangeState(BossStateType.Idle);
                return;
            }

            if (Boss.AttackPhase == BossAttackPhase.Active)
                Boss.ApplyStrike(_strike, _step, _hitTargets);

            _phaseRemaining -= deltaTime;
            Boss.SetPhaseProgress(1f - _phaseRemaining / _phaseDuration);
            if (_phaseRemaining > 0f) return;

            // One transition per frame ensures even a slow frame cannot skip a hit window.
            switch (Boss.AttackPhase)
            {
                case BossAttackPhase.Telegraph:
                    BeginPhase(BossAttackPhase.Active, _strike.active);
                    Boss.ApplyStrike(_strike, _step, _hitTargets);
                    break;
                case BossAttackPhase.Active:
                    float recovery = _combo && _step == 2 && _stepCount == 2
                        ? Boss.TwoSlashRecovery : _strike.recovery;
                    BeginPhase(BossAttackPhase.Recovery, recovery);
                    break;
                case BossAttackPhase.Recovery:
                    if (_step < _stepCount)
                    {
                        _step++;
                        StartStrike();
                    }
                    else if (!Boss.IsTargetWithinDetectionRange()) Boss.ChangeState(BossStateType.Idle);
                    else if (Boss.DistanceToTarget > Boss.AttackRange) Boss.ChangeState(BossStateType.Chase);
                    else StartPattern();
                    break;
            }
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
