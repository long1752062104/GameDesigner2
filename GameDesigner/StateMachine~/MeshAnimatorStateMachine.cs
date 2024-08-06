#if SHADER_ANIMATED
using FSG.MeshAnimator.ShaderAnimated;

namespace GameDesigner
{
    public class MeshAnimatorStateMachine : IAnimationHandler
    {
        private readonly ShaderMeshAnimator meshAnimator;

        public MeshAnimatorStateMachine(ShaderMeshAnimator meshAnimator)
        {
            this.meshAnimator = meshAnimator;
        }

        public void OnInit()
        {
            meshAnimator.OnAnimationFinished += (name) =>
            {
                currState.IsPlaying = false;
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
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction)
        {
            stateAction.animTime = meshAnimator.currentFrame / (float)meshAnimator.currentAnimation.TotalFrames * 100f;
            bool isPlaying = state.IsPlaying;
            return isPlaying;
        }
    }
}
#endif