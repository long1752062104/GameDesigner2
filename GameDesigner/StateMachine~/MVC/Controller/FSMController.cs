using System;
using UnityEngine;
using UnityEngine.Playables;

namespace GameDesigner
{
    [Serializable]
    public class FSMController : IStateMachine, IStateManager
    {
        private FSMView View;
        private FSMModel Model;
        private bool isInitialize;

        public IStateManager StateManager => this;
        public int NextId { get => Model.nextId; set => Model.nextId = value; }
        public Transform transform => View.transform;

        public FSMController(FSMView view)
        {
            View = view;
            Model = new FSMModel
            {
                states = view.states,
                defaulId = view.defaulId
            };
        }

        public FSMController(FSMView view, FSMModel model)
        {
            View = view;
            Model = model;
        }

        public void Init()
        {
            if (isInitialize)
                return;
            isInitialize = true;
#if SHADER_ANIMATED
            if (View.animMode == AnimationMode.MeshAnimator)
            {
                View.meshAnimator.OnAnimationFinished += (name) =>
                {
                    Model.currState.IsPlaying = false;
                };
            }
#endif
            if (Model.states.Length == 0)
                return;
            foreach (var state in Model.states)
                state.Init(this, View);
            if (Model.defaultState.actionSystem)
                Model.defaultState.Enter(0);
        }

        public void Execute()
        {
            if (!isInitialize)
                return;
            if (Model.states.Length == 0)
                return;
            if (Model.stateId != Model.nextId)
            {
                var currIdTemo = Model.stateId;
                var nextIdTemp = Model.nextId; //防止进入或退出行为又执行了EnterNextState切换了状态
                Model.stateId = Model.nextId;
                Model.states[currIdTemo].Exit();
                Model.states[nextIdTemp].Enter(Model.nextActionId);
                return; //有时候你调用Play时，并没有直接更新动画时间，而是下一帧才会更新动画时间，如果Play后直接执行下面的Update计算动画时间会导致鬼畜现象的问题
            }
            Model.currState.Update();
#if UNITY_EDITOR
            View.states = Model.states;
            View.stateId = Model.stateId;
#endif
        }

        public void ChangeState(int stateId, int actionId = 0, bool force = false)
        {
            if (force)
            {
                Model.states[Model.stateId].Exit();
                Model.states[stateId].Enter(actionId);
                Model.nextId = Model.stateId = stateId;
                Model.nextActionId = actionId;
            }
            else if (Model.nextId != stateId)
            {
                Model.nextId = stateId;
                Model.nextActionId = actionId;
            }
        }

        public void EnterNextState(int nextStateIndex, int actionId = 0)
        {
            ChangeState(nextStateIndex, actionId, true);
        }

        public void StatusEntry(int stateID, int actionId = 0)
        {
            ChangeState(stateID, actionId);
        }

        public void OnPlayAnimation(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            switch (View.animMode)
            {
                case AnimationMode.Animation:
                    var animState = View.animation[clipName];
                    animState.speed = state.animSpeed;
                    if (state.isCrossFade)
                    {
                        if (animState.time >= animState.length)
                        {
                            View.animation.Play(clipName);
                            animState.time = 0f;
                        }
                        else View.animation.CrossFade(clipName, state.duration);
                    }
                    else
                    {
                        View.animation.Play(clipName);
                        animState.time = 0f;
                    }
                    break;
                case AnimationMode.Animator:
                    View.animator.speed = state.animSpeed;
                    if (state.isCrossFade)
                    {
                        var stateInfo = View.animator.GetCurrentAnimatorStateInfo(0);
                        if (stateInfo.normalizedTime >= 1f)
                            View.animator.Play(clipName, 0, 0f);
                        else
                            View.animator.CrossFade(clipName, state.duration);
                    }
                    else View.animator.Play(clipName, 0, 0f);
                    break;
                case AnimationMode.Timeline:
                    if (stateAction.clipAsset != null)
                    {
                        View.director.Play(stateAction.clipAsset, DirectorWrapMode.None);
                        var playableGraph = View.director.playableGraph;
                        var playable = playableGraph.GetRootPlayable(0);
                        playable.SetSpeed(state.animSpeed);
                    }
                    else
                    {
                        View.animator.speed = state.animSpeed;
                        if (state.isCrossFade)
                        {
                            var stateInfo = View.animator.GetCurrentAnimatorStateInfo(0);
                            if (stateInfo.normalizedTime >= 1f)
                                View.animator.Play(clipName, 0, 0f);
                            else
                                View.animator.CrossFade(clipName, state.duration);
                        }
                        else View.animator.Play(clipName, 0, 0f);
                    }
                    break;
#if SHADER_ANIMATED
                case AnimationMode.MeshAnimator:
                    View.meshAnimator.speed = state.animSpeed;
                    if (meshAnimator.currentAnimation.animationName == clipName)
                        View.meshAnimator.RestartAnim();
                    else
                        View.meshAnimator.Play(clipIndex);
                    break;
#endif
                case AnimationMode.Time:
                    stateAction.animTime = 0f;
                    break;
            }
        }

        public bool OnAnimationUpdate(State state, StateAction stateAction)
        {
            var clipName = stateAction.clipName;
            var isPlaying = true;
            switch (View.animMode)
            {
                case AnimationMode.Animation:
                    var animState = View.animation[clipName];
                    stateAction.animTime = animState.time / animState.length * 100f;
                    isPlaying = View.animation.isPlaying;
                    break;
                case AnimationMode.Animator:
                    var stateInfo = View.animator.GetCurrentAnimatorStateInfo(0);
                    stateAction.animTime = stateInfo.normalizedTime / 1f * 100f;
                    break;
                case AnimationMode.Timeline:
                    if (stateAction.clipAsset != null)
                    {
                        var time = View.director.time;
                        var duration = View.director.duration;
                        stateAction.animTime = (float)(time / duration) * 100f;
                        isPlaying = View.director.state == PlayState.Playing;
                    }
                    else
                    {
                        var stateInfo1 = View.animator.GetCurrentAnimatorStateInfo(0);
                        stateAction.animTime = stateInfo1.normalizedTime / 1f * 100f;
                    }
                    break;
#if SHADER_ANIMATED
                case AnimationMode.MeshAnimator:
                    stateAction.animTime = View.meshAnimator.currentFrame / (float)View.meshAnimator.currentAnimation.TotalFrames * 100f;
                    isPlaying = state.IsPlaying;
                    break;
#endif
                case AnimationMode.Time:
                    stateAction.animTime += state.animSpeed * stateAction.animTimeMax * Time.deltaTime;
                    break;
            }
            return isPlaying;
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public State AddState(string name, params StateBehaviour[] behaviours)
        {
            var state = new State
            {
                name = name,
#if UNITY_EDITOR
                rect = new Rect(5000, 5000, 150, 30),
#endif
                ID = Model.states.Length,
                fsmView = View,
                stateMachine = this,
            };
            ArrayExtend.Add(ref Model.states, state);
            ArrayExtend.Add(ref state.actions, new StateAction() { ID = state.ID, stateMachine = this, fsmView = View, behaviours = new BehaviourBase[0] });
            View.UpdateStates();
            state.AddComponent(behaviours);
            return state;
        }

        public void RemoveState(string name)
        {
            for (int i = 0; i < Model.states.Length; i++)
            {
                if (Model.states[i].name == name)
                {
                    RemoveState(i);
                    break;
                }
            }
        }

        public void RemoveState(int stateIdx)
        {
            ArrayExtend.Remove(ref Model.states, Model.states[stateIdx]);
        }

        public State GetState(string name)
        {
            for (int i = 0; i < Model.states.Length; i++)
            {
                if (Model.states[i].name == name)
                {
                    return GetState(i);
                }
            }
            return default;
        }

        public State GetState(int stateIdx)
        {
            return Model.states[stateIdx];
        }
    }
}