using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDesigner
{
    /// <summary>
    /// 动画模式
    /// </summary>
    public enum AnimationMode
    {
        /// <summary>
        /// 旧版动画
        /// </summary>
        Animation,
        /// <summary>
        /// 新版动画
        /// </summary>
        Animator
    }

    /// <summary>
    /// 动画播放模式
    /// </summary>
	public enum AnimPlayMode
    {
        /// <summary>
        /// 随机播放动画
        /// </summary>
		Random,
        /// <summary>
        /// 顺序播放动画
        /// </summary>
		Sequence,
    }

    /// <summary>
    /// 状态 -- v2017/12/6
    /// </summary>
    [System.Serializable]
    public sealed class State : IState
    {
        /// <summary>
        /// 状态连接集合
        /// </summary>
		public List<Transition> transitions = new List<Transition>();
        /// <summary>
        /// 状态行为集合
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public List<StateBehaviour> behaviours = new List<StateBehaviour>();
        /// <summary>
        /// 动作系统 使用为真 , 不使用为假
        /// </summary>
		public bool actionSystem = false;
        /// <summary>
        /// 动画循环?
        /// </summary>
        public bool animLoop = true;
        /// <summary>
        /// 动画模式
        /// </summary>
        public AnimPlayMode animPlayMode = AnimPlayMode.Random;
        /// <summary>
        /// 动作索引
        /// </summary>
		public int actionIndex = 0;
        /// <summary>
        /// 动画速度
        /// </summary>
		public float animSpeed = 1;
        /// <summary>
        /// 动画结束是否进入下一个状态
        /// </summary>
        public bool isExitState = false;
        /// <summary>
        /// 动画结束进入下一个状态的ID
        /// </summary>
        public int DstStateID = 0;
        /// <summary>
        /// 状态动作集合
        /// </summary>
		public List<StateAction> actions = new List<StateAction>();

        public State() { }

        /// <summary>
        /// 创建状态
        /// </summary>
		public static State CreateStateInstance(StateMachine stateMachine, string stateName, Vector2 position)
        {
            State state = new State(stateMachine);
            state.name = stateName;
            state.rect.position = position;
            return state;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
		public State(StateMachine _stateMachine)
        {
            stateMachine = _stateMachine;
            ID = stateMachine.states.Count;
            stateMachine.states.Add(this);
            actions.Add(new StateAction() { stateMachine = stateMachine });
            stateMachine.UpdateStates();
        }

        /// <summary>
        /// 当前状态动作
        /// </summary>
        public StateAction Action
        {
            get
            {
                if (actionIndex >= actions.Count)
                    actionIndex = 0;
                return actions[actionIndex];
            }
        }

        /// <summary>
        /// 进入状态
        /// </summary>
		public void OnEnterState()
        {
            if (animPlayMode == AnimPlayMode.Random)//选择要进入的动作索引
                actionIndex = Random.Range(0, actions.Count);
            else
                actionIndex = (actionIndex < actions.Count - 1) ? actionIndex + 1 : 0;
            foreach (var behaviour in Action.behaviours) //当子动作的动画开始进入时调用
                if (behaviour.Active)
                    behaviour.OnEnter(Action);
            switch (stateMachine.animMode)
            {
                case AnimationMode.Animation:
                    stateMachine.animation[Action.clipName].speed = animSpeed;
                    //stateMachine.animation.Rewind(Action.clipName);
                    stateMachine.animation.Play(Action.clipName);
                    break;
                case AnimationMode.Animator:
                    stateMachine.animator.speed = animSpeed;
                    //stateMachine.animator.Rebind();
                    stateMachine.animator.Play(Action.clipName);
                    break;
            }
        }

        /// <summary>
        /// 状态每一帧
        /// </summary>
		public void OnUpdateState()
        {
            bool isPlaying = true;
            switch (stateMachine.animMode)
            {
                case AnimationMode.Animation:
                    Action.animTime = stateMachine.animation[Action.clipName].time / stateMachine.animation[Action.clipName].length * 100f;
                    isPlaying = stateMachine.animation.isPlaying;
                    break;
                case AnimationMode.Animator:
                    Action.animTime = stateMachine.animator.GetCurrentAnimatorStateInfo(0).normalizedTime / 1f * 100f;
                    break;
            }
            if (Action.animTime >= Action.animTimeMax | !isPlaying)
            {
                if (isExitState & transitions.Count > 0)
                {
                    transitions[DstStateID].isEnterNextState = true;
                    return;
                }
                if (animLoop)
                {
                    OnExitState();//退出函数
                    OnActionExit();
                    if (stateMachine.stateID == ID)//如果在动作行为里面有却换状态代码, 则不需要重载函数了, 否则重载当前状态
                        OnEnterState();//重载进入函数
                    return;
                }
                else OnStop();
            }
            foreach (var behaviour in Action.behaviours)
                if (behaviour.Active)
                    behaviour.OnUpdate(Action);
        }

        /// <summary>
        /// 当退出状态
        /// </summary>
		public void OnExitState()
        {
            foreach (var behaviour in Action.behaviours) //当子动作结束
                if (behaviour.Active)
                    behaviour.OnExit(Action);
        }

        /// <summary>
        /// 当子动作处于循环播放模式时, 子动作每次播放完成动画都会调用一次
        /// </summary>
        private void OnActionExit()
        {
            foreach (var behaviour in behaviours) //当子动作停止
                if (behaviour.Active)
                    behaviour.OnActionExit();
        }

        /// <summary>
        /// 当动作停止
        /// </summary>
        public void OnStop()
        {
            foreach (var behaviour in behaviours) //当子动作停止
                if (behaviour.Active)
                    behaviour.OnStop();
            foreach (var behaviour in Action.behaviours) //当子动作停止
                if (behaviour.Active)
                    behaviour.OnStop(Action);
        }
    }
}