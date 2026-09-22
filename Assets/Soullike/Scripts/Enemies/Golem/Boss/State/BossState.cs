namespace ProjectSoullike
{
    public abstract class BossState
    {
        protected BossState(BossController boss)
        {
            Boss = boss;
        }

        protected BossController Boss { get; }
        public abstract BossStateType StateType { get; }
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }
}
