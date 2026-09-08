namespace ProjectSoullike
{
    public sealed class BossDeadState : BossState
    {
        public BossDeadState(BossController boss) : base(boss) { }

        public override BossStateType StateType => BossStateType.Dead;

        public override void Enter()
        {
            Boss.StopMovement();
        }
    }
}
