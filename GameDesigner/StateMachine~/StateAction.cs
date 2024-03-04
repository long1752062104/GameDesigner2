using System;

namespace GameDesigner
{
    /// <summary>
    /// ARPG状态动作
    /// </summary>
	[Serializable]
    public sealed class StateAction : StateBase
    {
        /// <summary>
        /// 动画剪辑名称
        /// </summary>
		public string clipName;
        /// <summary>
        /// 动画剪辑索引
        /// </summary>
		public int clipIndex;
        /// <summary>
        /// 当前动画时间
        /// </summary>
		public float animTime;
        /// <summary>
        /// 动画结束时间
        /// </summary>
		public float animTimeMax = 100f;
        private bool isStop;
        /// <summary>
        /// 动作是否完成?, 当动画播放结束后为True, 否则为false
        /// </summary>
        public bool IsComplete => animTime >= animTimeMax - 1;

        public StateAction() { }

        public StateAction(State state, string clipName, params ActionBehaviour[] behaviours)
        {
            ID = state.ID;
            stateMachine = state.stateMachine;
            SetAnimClip(clipName);
            if (behaviours != null)
            {
                this.behaviours = behaviours;
                for (int i = 0; i < behaviours.Length; i++)
                {
                    behaviours[i].name = behaviours[i].GetType().ToString();
                    behaviours[i].stateMachine = stateMachine;
                    behaviours[i].Active = true;
                    behaviours[i].OnInit();
                }
            }
            else this.behaviours = new ActionBehaviour[0];
            ArrayExtend.Add(ref state.actions, this);
        }

        internal void Enter(State state)
        {
            isStop = false;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                    behaviour.OnEnter(this);
            }
            switch (stateMachine.animMode)
            {
                case AnimationMode.Animation:
                    stateMachine.animation[clipName].speed = state.animSpeed;
                    if (state.isCrossFade)
                    {
                        var animState = stateMachine.animation[clipName];
                        if (animState.time >= animState.length)
                        {
                            stateMachine.animation.Play(clipName);
                            stateMachine.animation[clipName].time = 0f;
                        }
                        else stateMachine.animation.CrossFade(clipName, state.duration);
                    }
                    else 
                    {
                        stateMachine.animation.Play(clipName);
                        stateMachine.animation[clipName].time = 0f;
                    }
                    break;
                case AnimationMode.Animator:
                    stateMachine.animator.speed = state.animSpeed;
                    if (state.isCrossFade)
                    {
                        var stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0);
                        if (stateInfo.normalizedTime >= 1f)
                            stateMachine.animator.Play(clipName, 0, 0f);
                        else
                            stateMachine.animator.CrossFade(clipName, state.duration);
                    }
                    else stateMachine.animator.Play(clipName, 0, 0f);
                    break;
#if SHADER_ANIMATED
                case AnimationMode.MeshAnimator:
                    stateMachine.meshAnimator.speed = state.animSpeed;
                    if (stateMachine.meshAnimator.currentAnimation.animationName == clipName)
                        stateMachine.meshAnimator.RestartAnim();
                    else
                        stateMachine.meshAnimator.Play(clipIndex);
                    break;
#endif
            }
        }

        internal void Exit()
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                    behaviour.OnExit(this);
            }
        }

        internal void Init()
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i].InitBehaviour();
                behaviours[i] = behaviour;
                behaviour.OnInit();
            }
        }

        internal void Update(State state)
        {
            if (isStop)
                return;
            var isPlaying = true;
            switch (stateMachine.animMode)
            {
                case AnimationMode.Animation:
                    var animState = stateMachine.animation[clipName];
                    animTime = animState.time / animState.length * 100f;
                    isPlaying = stateMachine.animation.isPlaying;
                    break;
                case AnimationMode.Animator:
                    var stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0);
                    animTime = stateInfo.normalizedTime / 1f * 100f;
                    break;
#if SHADER_ANIMATED
                case AnimationMode.MeshAnimator:
                    animTime = stateMachine.meshAnimator.currentFrame / (float)stateMachine.meshAnimator.currentAnimation.TotalFrames * 100f;
                    isPlaying = state.IsPlaying;
                    break;
#endif
            }
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                    behaviour.OnUpdate(this);
            }
            if (animTime >= animTimeMax | !isPlaying)
            {
                if (state.isExitState & state.transitions.Length > 0)
                {
                    state.transitions[state.DstStateID].isEnterNextState = true;
                    return;
                }
                if (state.animLoop)
                {
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        var behaviour = behaviours[i] as ActionBehaviour;
                        if (behaviour.Active)
                            behaviour.OnExit(this);
                    }
                    state.OnActionExit();
                    if (stateMachine.nextId == state.ID)//如果在动作行为里面有切换状态代码, 则不需要重载函数了, 否则重载当前状态
                        state.Enter(state.actionIndex);//重载进入函数
                    return;
                }
                else
                {
                    isStop = true;
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        var behaviour = behaviours[i] as ActionBehaviour;
                        if (behaviour.Active)
                            behaviour.OnStop(this);
                    }
                    state.OnActionStop();
                }
            }
        }

        public void SetAnimClip(string clipName)
        {
            this.clipName = clipName;
            for (int i = 0; i < stateMachine.clipNames.Count; i++)
            {
                if (clipName == stateMachine.clipNames[i])
                {
                    clipIndex = i;
                    break;
                }
            }
        }
    }
}