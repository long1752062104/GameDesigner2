#if SHADER_ANIMATED
using FSG.MeshAnimator.ShaderAnimated;

namespace GameDesigner
{
    public class MeshAnimatorStateMachine : IAnimationHandler
    {
        private ShaderMeshAnimator meshAnimator;
        private bool isPlaying;

        public MeshAnimatorStateMachine(ShaderMeshAnimator meshAnimator)
        {
            this.meshAnimator = meshAnimator;
        }

        public void SetParams(params object[] args)
        {
            meshAnimator = (ShaderMeshAnimator)args[0];
        }

        public void OnInit()
        {
            meshAnimator.OnAnimationFinished += (name) =>
            {
                isPlaying = false;
            };
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            meshAnimator.speed = state.animSpeed;
            if (meshAnimator.currentAnimation.animationName == clipName)
                meshAnimator.RestartAnim();
            else
                meshAnimator.Play(stateAction.clipIndex);
            isPlaying = true;
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, StateMachineUpdateMode currMode)
        {
            stateAction.animTime = meshAnimator.currentFrame / (float)meshAnimator.currentAnimation.TotalFrames * 100f;
            return isPlaying;
        }
    }
}
#endif