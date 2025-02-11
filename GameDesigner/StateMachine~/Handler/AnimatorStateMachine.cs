﻿using UnityEngine;

namespace GameDesigner
{
    public class AnimatorStateMachine : IAnimationHandler
    {
        private Animator animator;

        public AnimatorStateMachine(Animator animator)
        {
            this.animator = animator;
        }

        public void SetParams(params object[] args) //中途修改动画对象用
        {
            animator = args[0] as Animator;
        }

        public void OnInit()
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            animator.speed = state.animSpeed;
            StateAction.SetBlendTreeParameter(stateAction, animator);
            if (state.isCrossFade)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(stateAction.layer);
                if (stateInfo.normalizedTime >= 1f)
                    animator.Play(clipName, stateAction.layer, 0f);
                else
                    animator.CrossFade(clipName, state.duration);
            }
            else animator.Play(clipName, stateAction.layer, 0f);
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, StateMachineUpdateMode currMode)
        {
            var isPlaying = true;
            var stateInfo = animator.GetCurrentAnimatorStateInfo(stateAction.layer);
            stateAction.animTime = stateInfo.normalizedTime / 1f * 100f;
            return isPlaying;
        }
    }
}