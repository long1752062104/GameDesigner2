using UnityEngine;

namespace GameDesigner
{
    public class TimeStateMachine : IAnimationHandler
    {
        public TimeStateMachine()
        {
        }

        public void OnInit()
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            stateAction.animTime = 0f;
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction)
        {
            var isPlaying = true;
            stateAction.animTime += state.animSpeed * stateAction.animTimeMax * Time.deltaTime;
            return isPlaying;
        }
    }
}