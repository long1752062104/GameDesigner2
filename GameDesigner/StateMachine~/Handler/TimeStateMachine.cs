#if LOCK_STEP
using Time = LockStep.LSTime;
#else
using Time = UnityEngine.Time;
#endif

namespace GameDesigner
{
    public class TimeStateMachine : IAnimationHandler
    {
        public TimeStateMachine()
        {
        }

        public void SetParams(params object[] args)
        {
        }

        public void OnInit()
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            stateAction.animTime = 0f;
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, StateMachineUpdateMode currMode)
        {
            var isPlaying = true;
            if (currMode == StateMachineUpdateMode.Update)
                stateAction.animTime += state.animSpeed * stateAction.animTimeMax * Time.deltaTime;
            return isPlaying;
        }
    }
}