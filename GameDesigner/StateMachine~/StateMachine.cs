﻿using Net.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
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
		public int defaulId;
        /// <summary>
        /// 当前运行的状态索引
        /// </summary>
		public int stateId;
        /// <summary>
        /// 切换的状态id
        /// </summary>
        internal int nextId;
        /// <summary>
        /// 切换下一个状态的动作索引
        /// </summary>
        internal int nextActionId;
        /// <summary>
        /// 所有状态
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public State[] states;
        /// <summary>
        /// 选中的状态,可以多选
        /// </summary>
		public List<int> selectStates;
        /// <summary>
        /// 动画选择模式
        /// </summary>
        public AnimationMode animMode;
        /// <summary>
        /// 旧版动画组件
        /// </summary>
		public new Animation animation;
        /// <summary>
        /// 新版动画组件
        /// </summary>
        public Animator animator;
        /// <summary>
        /// 可播放导演动画
        /// </summary>
        public PlayableDirector director;
#if SHADER_ANIMATED
        /// <summary>
        /// shader动画组件
        /// </summary>
        public ShaderMeshAnimator meshAnimator;
#endif
        /// <summary>
        /// 动画剪辑
        /// </summary>
        public List<string> clipNames;

        /// <summary>
        /// 以状态ID取出状态对象
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public State this[int stateID] => states[stateID];

        /// <summary>
        /// 获取 或 设置 默认状态
        /// </summary>
        public State defaultState
        {
            get
            {
                if (defaulId < states.Length)
                    return states[defaulId];
                return null;
            }
            set { defaulId = value.ID; }
        }

        /// <summary>
        /// 当前状态
        /// </summary>
		public State currState => states[stateId];

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
        private StateManager _stateManager;
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

        private bool isInitialize;

        /// <summary>
        /// 创建状态机实例
        /// </summary>
        /// <param name="name">状态机名称</param>
        /// <returns></returns>
        public static StateMachine CreateStateMachineInstance(string name = "machine")
        {
            var stateMachine = new GameObject(name).AddComponent<StateMachine>();
            stateMachine.name = name;
            stateMachine.states = new State[0];
            stateMachine.selectStates = new List<int>();
            stateMachine.clipNames = new List<string>();
            return stateMachine;
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public State AddState(string name, params StateBehaviour[] behaviours)
        {
            var state = new State(this)
            {
                name = name,
#if UNITY_EDITOR
                rect = new Rect(10, 10, 150, 30)
#endif
            };
            if (behaviours != null)
            {
                state.behaviours = behaviours;
                for (int i = 0; i < behaviours.Length; i++)
                {
                    behaviours[i].name = behaviours[i].GetType().ToString();
                    behaviours[i].stateMachine = this;
                    behaviours[i].Active = true;
                    behaviours[i].OnInit();
                }
            }
            return state;
        }

        public void UpdateStates()
        {
            for (int i = 0; i < states.Length; i++)
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
                        if (behaviour.ID == id)
                            behaviour.ID = i;
                    foreach (var action in state1.actions)
                        foreach (var behaviour in action.behaviours)
                            if (behaviour.ID == id)
                                behaviour.ID = i;
                }
                states[i].ID = i;
            }
        }

        public void Init()
        {
            if (isInitialize)
                return;
            isInitialize = true;
#if SHADER_ANIMATED
            if (animMode == AnimationMode.MeshAnimator)
            {
                meshAnimator.OnAnimationFinished += (name) =>
                {
                    currState.IsPlaying = false;
                };
            }
#endif
            if (states.Length == 0)
                return;
            foreach (var state in states)
                state.Init();
            if (defaultState.actionSystem)
                defaultState.Enter(0);
        }

        internal void Execute()
        {
            if (!isInitialize)
                return;
            if (states.Length == 0)
                return;
            if (stateId != nextId)
            {
                var currIdTemo = stateId;
                var nextIdTemp = nextId; //防止进入或退出行为又执行了EnterNextState切换了状态
                stateId = nextId;
                states[currIdTemo].Exit();
                states[nextIdTemp].Enter(nextActionId);
                return; //有时候你调用Play时，并没有直接更新动画时间，而是下一帧才会更新动画时间，如果Play后直接执行下面的Update计算动画时间会导致鬼畜现象的问题
            }
            currState.Update();
        }

        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="nextStateIndex">下一个状态的ID</param>
		public void EnterNextState(int nextStateIndex, int actionId = 0)
        {
            ChangeState(nextStateIndex, actionId, true);
        }

        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        public void StatusEntry(int stateID, int actionId = 0)
        {
            ChangeState(stateID, actionId);
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="force"></param>
        public void ChangeState(int stateId, int actionId = 0, bool force = false)
        {
            if (force)
            {
                states[this.stateId].Exit();
                states[stateId].Enter(actionId);
                nextId = this.stateId = stateId;
                nextActionId = actionId;
            }
            else if (nextId != stateId)
            {
                nextId = stateId;
                nextActionId = actionId;
            }
        }

        private void OnDestroy()
        {
            foreach (var state in states)
            {
                foreach (var behaviour in state.behaviours)
                    behaviour.OnDestroyComponent();
                foreach (var transition in state.transitions)
                    foreach (var behaviour in transition.behaviours)
                        behaviour.OnDestroyComponent();
                foreach (var action in state.actions)
                    foreach (var behaviour in action.behaviours)
                        behaviour.OnDestroyComponent();
            }
        }

#if UNITY_EDITOR
        public void OnScriptReload()
        {
            foreach (var state in states)
            {
                state.stateMachine = this;
                for (int i = 0; i < state.behaviours.Length; i++)
                {
                    var type = AssemblyHelper.GetType(state.behaviours[i].name);
                    if (type == null)
                    {
                        ArrayExtend.RemoveAt(ref state.behaviours, i);
                        if (i >= 0) i--;
                        continue;
                    }
                    var metadatas = new List<Metadata>(state.behaviours[i].Metadatas);
                    var active = state.behaviours[i].Active;
                    var show = state.behaviours[i].show;
                    state.behaviours[i] = (StateBehaviour)Activator.CreateInstance(type);
                    state.behaviours[i].Reload(type, this, metadatas);
                    state.behaviours[i].ID = state.ID;
                    state.behaviours[i].Active = active;
                    state.behaviours[i].show = show;
                    state.behaviours[i].stateMachine = this;
                }
                foreach (var t in state.transitions)
                {
                    t.stateMachine = this;
                    for (int i = 0; i < t.behaviours.Length; i++)
                    {
                        var type = AssemblyHelper.GetType(t.behaviours[i].name);
                        if (type == null)
                        {
                            ArrayExtend.RemoveAt(ref t.behaviours, i);
                            if (i >= 0) i--;
                            continue;
                        }
                        var metadatas = new List<Metadata>(t.behaviours[i].Metadatas);
                        var active = t.behaviours[i].Active;
                        var show = t.behaviours[i].show;
                        t.behaviours[i] = (TransitionBehaviour)Activator.CreateInstance(type);
                        t.behaviours[i].Reload(type, this, metadatas);
                        t.behaviours[i].ID = state.ID;
                        t.behaviours[i].Active = active;
                        t.behaviours[i].show = show;
                        t.behaviours[i].stateMachine = this;
                    }
                }
                foreach (var a in state.actions)
                {
                    a.stateMachine = this;
                    for (int i = 0; i < a.behaviours.Length; i++)
                    {
                        var type = AssemblyHelper.GetType(a.behaviours[i].name);
                        if (type == null)
                        {
                            ArrayExtend.RemoveAt(ref a.behaviours, i);
                            if (i >= 0) i--;
                            continue;
                        }
                        var metadatas = new List<Metadata>(a.behaviours[i].Metadatas);
                        var active = a.behaviours[i].Active;
                        var show = a.behaviours[i].show;
                        a.behaviours[i] = (ActionBehaviour)Activator.CreateInstance(type);
                        a.behaviours[i].Reload(type, this, metadatas);
                        a.behaviours[i].ID = state.ID;
                        a.behaviours[i].Active = active;
                        a.behaviours[i].show = show;
                        a.behaviours[i].stateMachine = this;
                    }
                }
            }
        }
#endif
    }
}