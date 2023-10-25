﻿using Net.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

#if SHADER_ANIMATED
using FSG.MeshAnimator.ShaderAnimated;
#endif

namespace GameDesigner
{
    /// <summary>
    /// 状态机 v2017/12/6
    /// </summary>
    public class StateMachine : MonoBehaviour
    {
        /// <summary>
        /// 默认状态ID
        /// </summary>
		public int defaulID = 0;
        /// <summary>
        /// 当前运行的状态索引
        /// </summary>
		public int stateID = 0;
        /// <summary>
        /// 所有状态
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public List<State> states = new List<State>();
        /// <summary>
        /// 选中的状态,可以多选
        /// </summary>
		public List<int> selectStates = new List<int>();
        /// <summary>
        /// 动画选择模式
        /// </summary>
        public AnimationMode animMode = AnimationMode.Animation;
        /// <summary>
        /// 旧版动画组件
        /// </summary>
		public new Animation animation = null;
        /// <summary>
        /// 新版动画组件
        /// </summary>
        public Animator animator = null;

#if SHADER_ANIMATED
        /// <summary>
        /// shader动画组件
        /// </summary>
        public ShaderMeshAnimator meshAnimator;
#endif
        /// <summary>
        /// 动画剪辑
        /// </summary>
        public List<string> clipNames = new List<string>();

        /// <summary>
        /// 以状态ID取出状态对象
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public State this[int stateID]
        {
            get
            {
                return states[stateID];
            }
        }

        /// <summary>
        /// 获取 或 设置 默认状态
        /// </summary>
        public State defaultState
        {
            get
            {
                if (defaulID < states.Count)
                    return states[defaulID];
                return null;
            }
            set { defaulID = value.ID; }
        }

        /// <summary>
        /// 当前状态
        /// </summary>
		public State currState => states[stateID];

        /// <summary>
        /// 选择的状态
        /// </summary>
		public State selectState
        {
            get
            {
                if (selectStates.Count > 0)
                    return states[selectStates[0]];
                return null;
            }
            set
            {
                if (!selectStates.Contains(value.ID))
                    selectStates.Add(value.ID);
            }
        }

        [SerializeField]
        private StateManager _stateManager = null;
        /// <summary>
        /// 状态管理
        /// </summary>
		public StateManager stateManager
        {
            get
            {
                if (_stateManager == null)
                    _stateManager = transform.GetComponentInParent<StateManager>();
                return _stateManager;
            }
            set { _stateManager = value; }
        }

        /// <summary>
        /// 创建状态机实例
        /// </summary>
        /// <param name="name">状态机名称</param>
        /// <returns></returns>
        public static StateMachine CreateStateMachineInstance(string name = "machine")
        {
            StateMachine stateMachine = new GameObject(name).AddComponent<StateMachine>();
            stateMachine.name = name;
            return stateMachine;
        }

        public void UpdateStates()
        {
            for (int i = 0; i < states.Count; i++)
            {
                int id = states[i].ID;
                foreach (var state1 in states)
                {
                    foreach (var transition in state1.transitions)
                    {
                        if (transition.currStateID == id)
                            transition.currStateID = i;
                        if (transition.nextStateID == id)
                            transition.nextStateID = i;
                    }
                    foreach (var behaviour in state1.behaviours)
                    {
                        if (behaviour.ID == id)
                            behaviour.ID = i;
                    }
                    foreach (var action in state1.actions)
                    {
                        foreach (var behaviour in action.behaviours)
                        {
                            if (behaviour.ID == id)
                                behaviour.ID = i;
                        }
                    }
                }
                states[i].ID = i;
            }
        }

        public void Init()
        {
            foreach (var state in states)
            {
                state.Init();
            }
            if (defaultState.actionSystem)
                defaultState.OnEnterState();
        }

        /// <summary>
        /// 当退出状态时处理连接事件
        /// </summary>
        /// <param name="state">要退出的状态</param>
        public void OnStateTransitionExit(State state)
        {
            foreach (var transition in state.transitions)
                if (transition.model == TransitionModel.ExitTime)
                    transition.time = 0;
        }

        /// <summary>
        /// 当进入下一个状态
        /// </summary>
        /// <param name="currState">当前状态</param>
        /// <param name="enterState">要进入的状态</param>
        public void EnterNextState(State currState, State enterState)
        {
            foreach (StateBehaviour behaviour in currState.behaviours)//先退出当前的所有行为状态OnExitState的方法
                if (behaviour.Active)
                    behaviour.OnExit();
            OnStateTransitionExit(currState);
            foreach (StateBehaviour behaviour in enterState.behaviours)//最后进入新的状态前调用这个新状态的所有行为类的OnEnterState方法
                if (behaviour.Active)
                    behaviour.OnEnter();
            if (currState.actionSystem)
                currState.OnExitState();
            if (enterState.actionSystem)
                enterState.OnEnterState();
            stateID = enterState.ID;
        }

        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="nextStateIndex">下一个状态的ID</param>
		public void EnterNextState(int nextStateIndex)
        {
            EnterNextState(currState, states[nextStateIndex]);
        }

        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        public void StatusEntry(int stateID)
        {
            if (this.stateID == stateID)
                return;
            EnterNextState(stateID);
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateID"></param>
        /// <param name="force"></param>
        public void ChangeState(int stateID, bool force = false) 
        {
            if (this.stateID == stateID & !force)
                return;
            EnterNextState(stateID);
        }

        private void OnDestroy()
        {
            foreach (var state in states)
            {
                foreach (var behaviour in state.behaviours)
                {
                    behaviour.OnDestroyComponent();
                }
                foreach (var transition in state.transitions)
                {
                    foreach (var behaviour in transition.behaviours)
                    {
                        behaviour.OnDestroyComponent();
                    }
                }
                foreach (var action in state.actions)
                {
                    foreach (var behaviour in action.behaviours)
                    {
                        behaviour.OnDestroyComponent();
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void OnScriptReload()
        {
            foreach (var state in states)
            {
                for (int i = 0; i < state.behaviours.Count; i++)
                {
                    var type = AssemblyHelper.GetType(state.behaviours[i].name);
                    if (type == null)
                    {
                        state.behaviours.RemoveAt(i);
                        if (i >= 0) i--;
                        continue;
                    }
                    var metadatas = new List<Metadata>(state.behaviours[i].metadatas);
                    var active = state.behaviours[i].Active;
                    var show = state.behaviours[i].show;
                    state.behaviours[i] = (StateBehaviour)Activator.CreateInstance(type);
                    state.behaviours[i].Reload(type, this, metadatas);
                    state.behaviours[i].ID = state.ID;
                    state.behaviours[i].Active = active;
                    state.behaviours[i].show = show;
                }
                foreach (var t in state.transitions)
                {
                    for (int i = 0; i < t.behaviours.Count; i++)
                    {
                        var type = AssemblyHelper.GetType(t.behaviours[i].name);
                        if (type == null)
                        {
                            t.behaviours.RemoveAt(i);
                            if (i >= 0) i--;
                            continue;
                        }
                        var metadatas = new List<Metadata>(t.behaviours[i].metadatas);
                        var active = t.behaviours[i].Active;
                        var show = t.behaviours[i].show;
                        t.behaviours[i] = (TransitionBehaviour)Activator.CreateInstance(type);
                        t.behaviours[i].Reload(type, this, metadatas);
                        t.behaviours[i].ID = state.ID;
                        t.behaviours[i].Active = active;
                        t.behaviours[i].show = show;
                    }
                }
                foreach (var a in state.actions)
                {
                    for (int i = 0; i < a.behaviours.Count; i++)
                    {
                        var type = AssemblyHelper.GetType(a.behaviours[i].name);
                        if (type == null)
                        {
                            a.behaviours.RemoveAt(i);
                            if (i >= 0) i--;
                            continue;
                        }
                        var metadatas = new List<Metadata>(a.behaviours[i].metadatas);
                        var active = a.behaviours[i].Active;
                        var show = a.behaviours[i].show;
                        a.behaviours[i] = (ActionBehaviour)Activator.CreateInstance(type);
                        a.behaviours[i].Reload(type, this, metadatas);
                        a.behaviours[i].ID = state.ID;
                        a.behaviours[i].Active = active;
                        a.behaviours[i].show = show;
                    }
                }
            }
        }
#endif
    }
}