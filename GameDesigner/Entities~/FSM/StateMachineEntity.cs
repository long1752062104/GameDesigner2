using Net.Entities;
using System.Collections.Generic;

namespace Net.FSM
{
    public class StateMachine : Entities.Component, IEntityStart, IEntityUpdate
    {
        public int defaulID = -1;
        public int stateID;
        public List<State> states = new List<State>();

#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
        public UnityEngine.GameObject gameObject { get; set; }
        public UnityEngine.Transform transform => gameObject.transform;
#endif

        public void Start()
        {
            stateID = defaulID;
        }

        public void Update()
        {
            if (stateID == -1 | states.Count == 0)
                return;
            states[stateID].Update();
        }

        public void ChangeState(int stateID, bool force = false)
        {
            var currState = states[this.stateID];
            var nextState = states[stateID];
            if (currState == nextState && !force)
                return;
            foreach (StateBehaviour behaviour in currState.behaviours)//先退出当前的所有行为状态OnExitState的方法
                if (behaviour.Active)
                    behaviour.OnExit();
            //OnStateTransitionExit(currState);
            foreach (StateBehaviour behaviour in nextState.behaviours)//最后进入新的状态前调用这个新状态的所有行为类的OnEnterState方法
                if (behaviour.Active)
                    behaviour.OnEnter();
            //if (currState.actionSystem)
            //    currState.OnExitState();
            //if (enterState.actionSystem)
            //    enterState.OnEnterState();
            this.stateID = nextState.ID;
        }

        public void AddState(State state)
        {
            states.Add(state);
            for (int i = 0; i < states.Count; i++)
                states[i].ID = i;
        }

        public State CreateState(string name)
        {
            var state = new State(name)
            {
                StateMachine = this
            };
            return state;
        }
    }

    public class StateBase
    {
        public string name;
        public int ID, perID;
        public StateMachine StateMachine { get; internal set; }
        public List<BehaviourBase> behaviours = new List<BehaviourBase>();

        public T AddComponent<T>() where T : BehaviourBase, new()
        {
            return AddComponent(new T());
        }

        public T AddComponent<T>(T component) where T : BehaviourBase
        {
            //component.stateMachine = stateMachine;
            component.ID = ID;
            component.name = component.GetType().ToString();
            component.State = this as State;
            behaviours.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : BehaviourBase
        {
            for (int i = 0; i < behaviours.Count; i++)
                if (behaviours[i] is T component)
                    return component;
            return null;
        }

        public T[] GetComponents<T>() where T : BehaviourBase
        {
            var components = new List<T>();
            for (int i = 0; i < behaviours.Count; i++)
                if (behaviours[i] is T component)
                    components.Add(component);
            return components.ToArray();
        }
    }

    public class State : StateBase
    {
        protected State()
        {
        }

        internal State(string name)
        {
            this.name = name;
        }

        public void Update()
        {
            //if (state.actionSystem)
            //    state.OnUpdateState();
            foreach (StateBehaviour behaviour in behaviours)
                if (behaviour.Active)
                    behaviour.OnUpdate();
            //for (int i = 0; i < state.transitions.Count; i++)
            //    OnTransition(state.transitions[i]);
        }
    }

    public class AnimState : State
    {
        public float AnimSpeed { get; set; }
    }

    public class BehaviourBase
    {
        public string name;
        public int ID;
        public bool Active = true;
        public State State { get; internal set; }
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
        public UnityEngine.GameObject gameObject => State.StateMachine.gameObject;
        public UnityEngine.Transform transform => gameObject.transform;
#endif
        public virtual void OnInit() { }

        public void ChangeState(int stateID, bool force = false)
        {
            State.StateMachine.ChangeState(stateID, force);
        }
    }

    public class StateBehaviour : BehaviourBase
    {
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnExit() { }
    }

    public class AnimStateBehaviour : StateBehaviour
    {
        public AnimState AnimState { get; internal set; }
    }

    public sealed class StateAction : StateBase
    {
        /// <summary>
        /// 动画剪辑名称
        /// </summary>
		public string clipName = string.Empty;
        /// <summary>
        /// 动画剪辑索引
        /// </summary>
		public int clipIndex = 0;
        /// <summary>
        /// 当前动画时间
        /// </summary>
		public float animTime = 0;
        /// <summary>
        /// 动画结束时间
        /// </summary>
		public float animTimeMax = 100;
        /// <summary>
        /// 动画倒入, 当在一帧内调用了多次Play方法，只会应用前一次调用的动画，最后要播放的动画没有真正被播放出来的问题
        /// </summary>
        public bool rewind;

        /// <summary>
        /// 动作是否完成?, 当动画播放结束后为True, 否则为false
        /// </summary>
        public bool IsComplete => animTime >= animTimeMax - 1;
    }

    public class ActionBehaviour : BehaviourBase
    {
        public virtual void OnEnter(StateAction action) { }

        public virtual void OnUpdate(StateAction action) { }

        public virtual void OnExit(StateAction action) { }

        public virtual void OnStop(StateAction action) { }
    }
}