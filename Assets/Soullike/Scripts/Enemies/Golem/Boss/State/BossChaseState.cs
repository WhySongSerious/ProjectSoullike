namespace ProjectSoullike
{
    public sealed class BossChaseState : BossState
    {
        public BossChaseState(BossController boss) : base(boss) { }

        public override BossStateType StateType => BossStateType.Chase;

        public override void Update()
        {
            if (!Boss.HasTarget)
            {
                Boss.ChangeState(BossStateType.Idle);
                return;
            }

            if (Boss.DistanceToTarget <= Boss.AttackRange)
            {
                Boss.ChangeState(BossStateType.Attack);
                return;
            }

            Boss.FaceTarget();
            Boss.MoveToTarget();
        }
    }
}
