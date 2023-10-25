using System;
using UnityEngine;

namespace GameDesigner
{
    /// <summary>
    /// 状态连接组件 2017年12月6日
    /// 版本修改2019.8.27
    /// </summary>
    [System.Serializable]
    public sealed class Transition : StateBase
    {
        public int currStateID, nextStateID;
        /// <summary>
        /// 当前状态
        /// </summary>
		public State currState 
        { 
            get 
            {
                foreach (var item in stateMachine.states)
                    if (item.ID == currStateID)
                        return item;
                return null;
            } 
        }
        /// <summary>
        /// 下一个状态
        /// </summary>
		public State nextState 
        {
            get
            {
                foreach (var item in stateMachine.states)
                    if (item.ID == nextStateID)
                        return item;
                return null;
            }
        }
        /// <summary>
        /// 连接控制模式
        /// </summary>
		public TransitionModel model = TransitionModel.ScriptControl;
        /// <summary>
        /// 当前时间
        /// </summary>
		public float time;
        /// <summary>
        /// 结束时间
        /// </summary>
		public float exitTime = 1f;
        /// <summary>
        /// 是否进入下一个状态?
        /// </summary>
		public bool isEnterNextState = false;

        public Transition() { }

        /// <summary>
        /// 创建连接实例
        /// </summary>
        /// <param name="state">连接的开始状态</param>
        /// <param name="nextState">连接的结束状态</param>
        /// <param name="transitionName">连接名称</param>
        /// <returns></returns>
		public static Transition CreateTransitionInstance(State state, State nextState, string transitionName = "New Transition")
        {
            var t = new Transition
            {
                name = transitionName,
                currStateID = state.ID,
                nextStateID = nextState.ID,
                stateMachine = state.stateMachine
            };
            state.transitions.Add(t);
            for (int i = 0; i < state.transitions.Count; i++)
                state.transitions[i].ID = i;
            return t;
        }

        internal void Init()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                var behaviour = (TransitionBehaviour)behaviours[i].InitBehaviour();
                behaviour.transitionID = i;
                behaviours[i] = behaviour;
                behaviour.OnInit();
            }
        }

        internal void Update()
        {
            foreach (TransitionBehaviour behaviour in behaviours)
                if (behaviour.Active)
                    behaviour.OnUpdate(ref isEnterNextState);
            if (model == TransitionModel.ExitTime)
            {
                time += Time.deltaTime;
                if (time > exitTime)
                    isEnterNextState = true;
            }
            if (isEnterNextState)
            {
                stateMachine.EnterNextState(stateMachine.currState, nextState);
                time = 0;
                isEnterNextState = false;
            }
        }
    }
}