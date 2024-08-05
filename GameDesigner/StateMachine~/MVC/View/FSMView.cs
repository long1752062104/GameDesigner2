using Net.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameDesigner
{
    public class FSMView : MonoBehaviour, IStateMachineView
    {
        /// <summary>
        /// 所有状态
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public State[] states;
        /// <summary>
        /// 默认状态
        /// </summary>
        public int defaulId;
        /// <summary>
        /// 当前状态ID
        /// </summary>
        public int stateId;
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

        public State[] States { get => states; set => states = value; }
        public State SelectState
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
        public List<int> SelectStates { get => selectStates; set => selectStates = value; }
        public State DefaultState
        {
            get
            {
                if (defaulId < states.Length)
                    return states[defaulId];
                return null;
            }
            set { defaulId = value.ID; }
        }
        public int StateId { get => stateId; set => stateId = value; }
        public List<string> ClipNames { get => clipNames; set => clipNames = value; }

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

#if UNITY_EDITOR
        public void OnValidate()
        {
            states ??= new State[0];
            clipNames ??= new List<string>();
            selectStates ??= new List<int>();
            foreach (var state in states)
            {
                state.fsmView = this;
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
                    state.behaviours[i].Reload(type, metadatas);
                    state.behaviours[i].ID = state.ID;
                    state.behaviours[i].Active = active;
                    state.behaviours[i].show = show;
                    //state.behaviours[i].stateMachine = this;
                    state.behaviours[i].fsmView = this;
                }
                foreach (var t in state.transitions)
                {
                    t.fsmView = this;
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
                        t.behaviours[i].Reload(type, metadatas);
                        t.behaviours[i].ID = state.ID;
                        t.behaviours[i].Active = active;
                        t.behaviours[i].show = show;
                        //t.behaviours[i].stateMachine = this;
                        t.behaviours[i].fsmView = this;
                    }
                }
                foreach (var a in state.actions)
                {
                    a.fsmView = this;
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
                        a.behaviours[i].Reload(type, metadatas);
                        a.behaviours[i].ID = state.ID;
                        a.behaviours[i].Active = active;
                        a.behaviours[i].show = show;
                        //a.behaviours[i].stateMachine = this;
                        a.behaviours[i].fsmView = this;
                    }
                }
            }
        }
#endif
    }
}