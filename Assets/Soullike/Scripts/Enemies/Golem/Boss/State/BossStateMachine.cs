using UnityEngine;

namespace ProjectSoullike
{
    public sealed class BossStateMachine
    {
        public BossState CurrentState { get; private set; }

        public void ChangeState(BossState nextState)
        {
            if (nextState == null || CurrentState == nextState)
            {
                return;
            }

            BossStateType previousType = CurrentState?.StateType ?? BossStateType.None;
            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
            Debug.Log($"[Boss State] {previousType} -> {CurrentState.StateType}");
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}
