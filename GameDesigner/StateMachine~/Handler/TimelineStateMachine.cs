﻿using UnityEngine;
using UnityEngine.Playables;

namespace GameDesigner
{
    public class TimelineStateMachine : IAnimationHandler
    {
        private Animator animator;
        private PlayableDirector director;

        public TimelineStateMachine(Animator animator, PlayableDirector director)
        {
            this.animator = animator;
            this.director = director;
        }

        public void SetParams(params object[] args) //中途修改动画对象用
        {
            animator = args[0] as Animator;
            director = args[1] as PlayableDirector;
        }

        public void OnInit()
        {
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            if (stateAction.clipAsset != null)
            {
                director.Play(stateAction.clipAsset, DirectorWrapMode.None);
                var playableGraph = director.playableGraph;
                var playable = playableGraph.GetRootPlayable(0);
                playable.SetSpeed(state.animSpeed);
            }
            else
            {
                animator.speed = state.animSpeed;
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
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction, StateMachineUpdateMode currMode)
        {
            var isPlaying = true;
            if (stateAction.clipAsset != null)
            {
                var time = director.time;
                var duration = director.duration;
                stateAction.animTime = (float)(time / duration) * 100f;
                isPlaying = director.state == PlayState.Playing;
            }
            else
            {
                var stateInfo1 = animator.GetCurrentAnimatorStateInfo(stateAction.layer);
                stateAction.animTime = stateInfo1.normalizedTime / 1f * 100f;
            }
            return isPlaying;
        }
    }
}