﻿using System;
using UnityEngine;
#if LOCK_STEP
using FLOAT = SoftFloat.sfloat;
using Random = NonLockStep.NRandom;
#else
using FLOAT = System.Single;
using Random = UnityEngine.Random;
#endif

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
        /// <summary>
        /// 时间轴可播放导演动画
        /// </summary>
        Timeline,
        /// <summary>
        /// 时间驱动模式
        /// </summary>
        Time,
        /// <summary>
        /// 空状态模式
        /// </summary>
        None,
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

    public enum StateType
    {
        None,
        SubStateMachine,
        Parent,
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
		public Transition[] transitions;
        /// <summary>
        /// 动作系统 使用为真 , 不使用为假
        /// </summary>
		public bool actionSystem;
        /// <summary>
        /// 动画循环?
        /// </summary>
        public bool animLoop = true;
        /// <summary>
        /// 动画模式
        /// </summary>
        public AnimPlayMode animPlayMode;
        /// <summary>
        /// 动作索引
        /// </summary>
		[HideInInspector]
        public int actionIndex = -1;
        /// <summary>s
        /// 动画速度
        /// </summary>
		public FLOAT animSpeed = 1;
        /// <summary>
        /// 动画淡入淡出
        /// </summary>
        public bool isCrossFade;
        /// <summary>
        /// 过渡的持续时间
        /// </summary>
        public FLOAT duration = 0.25f;
        /// <summary>
        /// 动画结束是否进入下一个状态
        /// </summary>
        public bool isExitState;
        /// <summary>
        /// 动画结束进入下一个状态的ID
        /// </summary>
        public int DstStateID;
        /// <summary>
        /// 状态动作集合
        /// </summary>
		public StateAction[] actions;
        public StateType Type;
        /// <summary>
        /// 子状态机
        /// </summary>
        public StateMachineCore subStateMachine;

        public State()
        {
        }

#if UNITY_EDITOR
        public static State AddNode(IStateMachine stateMachine, string stateName, Vector2 position)
        {
            var state = new State(stateMachine)
            {
                name = stateName,
                rect = new Rect(position, new Vector2(150, 30))
            };
            return state;
        }
        public static State AddSubStateMachine(IStateMachine stateMachine, string stateName, Vector2 position)
        {
            var state = new State(stateMachine)
            {
                name = stateName,
                rect = new Rect(position, new Vector2(150, 30)),
                Type = StateType.SubStateMachine,
                subStateMachine = new StateMachineCore
                {
                    name = stateName
                }
            };
            return state;
        }
        public static State AddParent(IStateMachine stateMachine, string stateName, Vector2 position)
        {
            var state = new State(stateMachine)
            {
                name = stateName,
                rect = new Rect(position, new Vector2(150, 30)),
                Type = StateType.Parent,
            };
            return state;
        }
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public State(IStateMachine stateMachine)
        {
            behaviours = new BehaviourBase[0];
            transitions = new Transition[0];
            actions = new StateAction[0];
            this.stateMachine = stateMachine;
            ID = stateMachine.States.Length;
            stateMachine.States = ArrayHelper.Add(stateMachine.States, this);
            ArrayHelper.Add(ref actions, new StateAction() { ID = ID, stateMachine = stateMachine, behaviours = new BehaviourBase[0] });
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
            if (Type == StateType.SubStateMachine)
            {
                var states = subStateMachine.States;
                for (int i = 0; i < states.Length; i++)
                {
                    if (states[i].Type == StateType.Parent)
                    {
                        if (states[i].transitions.Length > 0)
                            subStateMachine.ChangeChildState(states[i].transitions[0].nextStateID, 0);
                        else
                            subStateMachine.ChangeChildState(subStateMachine.DefaultState.ID, 0); //需要强制进入，否则可能会不触发行为的OnEnter
                        break;
                    }
                }
                return;
            }
            if (Type == StateType.Parent)
            {
                if (stateMachine.Parent == null)
                    return;
                var states = stateMachine.Parent.States;
                for (int i = 0; i < states.Length; i++)
                {
                    if (states[i].subStateMachine == stateMachine)
                    {
                        if (states[i].transitions.Length > 0)
                            stateMachine.Parent.ChangeChildState(states[i].transitions[0].nextStateID, 0);
                        else
                            stateMachine.Parent.ChangeChildState(stateMachine.Parent.DefaultState.ID, 0);
                        break;
                    }
                }
                return;
            }
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
                Action.Enter(this);
        }

        internal void Exit()
        {
            if (Type == StateType.SubStateMachine)
                return;
            if (Type == StateType.Parent)
                return;
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

        internal void Init(IStateMachine stateMachine)
        {
            if (Type == StateType.SubStateMachine)
            {
                subStateMachine.View = stateMachine.View;
                subStateMachine.Parent = stateMachine;
                subStateMachine.Handler = stateMachine.Handler;
                subStateMachine.Init();
            }
            this.stateMachine = stateMachine;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i].InitBehaviour(stateMachine);
                behaviours[i] = behaviour;
                behaviour.OnInit();
            }
            foreach (var t in transitions)
                t.Init(stateMachine);
            if (actionSystem)
                foreach (var action in actions)
                    action.Init(stateMachine);
        }

        internal void Update(StateMachineUpdateMode currMode)
        {
            if (Type == StateType.SubStateMachine)
            {
                subStateMachine.Execute(currMode);
                return;
            }
            else if (Type == StateType.Parent)
            {
                stateMachine.Parent.Execute(currMode);
                return;
            }
            if (actionSystem)
                Action.Update(this, currMode);
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as StateBehaviour;
                if (behaviour.Active)
                {
                    switch (currMode)
                    {
                        case StateMachineUpdateMode.Update:
                            behaviour.OnUpdate();
                            break;
                        case StateMachineUpdateMode.LateUpdate:
                            behaviour.OnLateUpdate();
                            break;
                        case StateMachineUpdateMode.FixedUpdate:
                            behaviour.OnFixedUpdate();
                            break;
                    }
                }
            }
            for (int i = 0; i < transitions.Length; i++)
                transitions[i].Update(currMode);
        }

        public StateAction AddAction(string clipName, params ActionBehaviour[] behaviours)
        {
            return new StateAction(this, clipName, behaviours);
        }

        public Transition AddTransition(int stateId, params TransitionBehaviour[] behaviours)
        {
            return new Transition(this, stateId, behaviours);
        }

        /// <summary>
        /// 设置动作结束后进入下一个状态
        /// </summary>
        /// <param name="stateId"></param>
        public void ActionEndTransfer(int stateId)
        {
            isExitState = true;
            DstStateID = stateId;
        }

        public StateAction GetAction(int index)
        {
            return actions[index];
        }
    }
}