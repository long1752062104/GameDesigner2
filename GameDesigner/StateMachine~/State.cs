using System;
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
        Animator,
#if SHADER_ANIMATED
        /// <summary>
        /// Shader GPU网格动画
        /// </summary>
        MeshAnimator,
#endif
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
        /// <summary>
        /// 代码控制模式
        /// </summary>
        Code,
    }

    /// <summary>
    /// 状态 -- v2017/12/6
    /// </summary>
    [Serializable]
    public sealed class State : StateBase
    {
        /// <summary>
        /// 状态连接集合
        /// </summary>
		public Transition[] transitions = new Transition[0];
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
		public StateAction[] actions = new StateAction[0];
        internal bool IsPlaying;

        public State() { }

#if UNITY_EDITOR
        /// <summary>
        /// 创建状态
        /// </summary>
        public static State CreateStateInstance(StateMachine stateMachine, string stateName, Vector2 position)
        {
            var state = new State(stateMachine);
            state.name = stateName;
            state.rect.position = position;
            return state;
        }
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public State(StateMachine _stateMachine)
        {
            stateMachine = _stateMachine;
            ID = stateMachine.states.Length;
            ArrayExtend.Add(ref stateMachine.states, this);
            ArrayExtend.Add(ref actions, new StateAction() { ID = ID, stateMachine = stateMachine });
            stateMachine.UpdateStates();
        }

        /// <summary>
        /// 当前状态动作
        /// </summary>
        public StateAction Action => actions[actionIndex % actions.Length];

        /// <summary>
        /// 进入状态
        /// </summary>
		public void Enter(int actionIdx)
        {
            IsPlaying = true;
            if (animPlayMode == AnimPlayMode.Random)//选择要进入的动作索引
                actionIndex = Random.Range(0, actions.Length);
            else if (animPlayMode == AnimPlayMode.Sequence)
                actionIndex++;
            else
                actionIndex = actionIdx;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as StateBehaviour;
                if (behaviour.Active)
                    behaviour.OnEnter();
            }
            for (int i = 0; i < transitions.Length; i++)
            {
                var transition = transitions[i];
                transition.time = 0;
            }
            if (actionSystem)
                Action.Enter(animSpeed);
        }

        internal void Exit()
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as StateBehaviour;
                if (behaviour.Active)
                    behaviour.OnExit();
            }
            if (actionSystem)
                Action.Exit();
        }

        /// <summary>
        /// 当子动作处于循环播放模式时, 子动作每次播放完成动画都会调用一次
        /// </summary>
        internal void OnActionExit()
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as StateBehaviour;
                if (behaviour.Active)
                    behaviour.OnActionExit();
            }
        }

        /// <summary>
        /// 当动作停止
        /// </summary>
        public void OnActionStop()
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as StateBehaviour;
                if (behaviour.Active)
                    behaviour.OnStop();
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
            foreach (var t in transitions)
                t.Init();
            if (actionSystem)
                foreach (var action in actions)
                    action.Init();
        }

        internal void Update()
        {
            if (actionSystem)
                Action.Update(this);
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as StateBehaviour;
                if (behaviour.Active)
                    behaviour.OnUpdate();
            }
            for (int i = 0; i < transitions.Length; i++)
                transitions[i].Update();
        }
    }
}