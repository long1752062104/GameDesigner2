﻿using UnityEngine;

namespace GameDesigner
{
    public class AnimationStateMachine : IAnimationHandler
    {
        private Animation animation;

        public AnimationStateMachine(Animation animation)
        {
            this.animation = animation;
        }

        public void SetParams(params object[] args) //中途修改动画对象用
        {
            animation = args[0] as Animation;
        }

        public void OnInit()
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            var animState = animation[clipName];
            animState.speed = state.animSpeed;
            if (state.isCrossFade)
            {
                if (animState.time >= animState.length)
                {
                    animation.Play(clipName);
                    animState.time = 0f;
                }
                else animation.CrossFade(clipName, state.duration);
            }
            else
            {
                animation.Play(clipName);
                animState.time = 0f;
            }
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, StateMachineUpdateMode currMode)
        {
            var clipName = stateAction.clipName;
            var animState = animation[clipName];
            stateAction.animTime = animState.time / animState.length * 100f;
            bool isPlaying = animation.isPlaying;
            return isPlaying;
        }
    }
}