namespace ProjectSoullike
{
    public sealed class BossIdleState : BossState
    {
        public BossIdleState(BossController boss) : base(boss) { }

        public override BossStateType StateType => BossStateType.Idle;

        public override void Enter()
        {
            Boss.StopMovement();
        }

        public override void Update()
        {
            if (Boss.IsTargetWithinDetectionRange())
            {
                Boss.ChangeState(BossStateType.Chase);
            }
        }
    }
}
