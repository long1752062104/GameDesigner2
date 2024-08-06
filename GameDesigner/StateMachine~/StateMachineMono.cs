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
    public class StateMachineMono : StateMachineView
    {
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

        // 旧版本兼容问题，如果去掉这个字段，之前的状态会全部丢失！
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public State[] states;

        public static StateMachineMono CreateSupport(string name = "machine")
        {
            var monoStateMachine = new GameObject(name).AddComponent<StateMachineMono>();
            monoStateMachine.name = name;
            var stateMachine = new StateMachineCore()
            {
                name = name,
                states = new State[0],
                selectStates = new List<int>(),
                clipNames = new List<string>(),
                transform = monoStateMachine.transform,
            };
            monoStateMachine.stateMachine = stateMachine;
            return monoStateMachine;
        }

        public override void Init(Transform root)
        {
            switch (animMode)
            {
                case AnimationMode.Animation:
                    stateMachine.Handler = new AnimationStateMachine(animation);
                    break;
                case AnimationMode.Animator:
                    stateMachine.Handler = new AnimatorStateMachine(animator);
                    break;
                case AnimationMode.Timeline:
                    stateMachine.Handler = new TimelineStateMachine(animator, director);
                    break;
                case AnimationMode.Time:
                    stateMachine.Handler = new TimeStateMachine();
                    break;
#if SHADER_ANIMATED
                case AnimationMode.MeshAnimator:
                    stateMachine.Handler = new MeshAnimatorStateMachine(meshAnimator);
                    break;
#endif
            }
            stateMachine.transform = root;
            stateMachine.Init();
        }

#if UNITY_EDITOR
        public override void EditorInit(Transform root)
        {
            if (animation == null)
                animation = root.GetComponentInChildren<Animation>();
            if (animation != null)
            {
                var clips = UnityEditor.AnimationUtility.GetAnimationClips(animation.gameObject);
                stateMachine.ClipNames.Clear();
                foreach (var clip in clips)
                    stateMachine.ClipNames.Add(clip.name);
            }
            if (animator == null)
                animator = root.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                if (animator.runtimeAnimatorController is UnityEditor.Animations.AnimatorController controller)
                {
                    if (controller.layers.Length > 0) //打AB包后选择这里是0
                    {
                        var layer = controller.layers[0];
                        var states = layer.stateMachine.states;
                        stateMachine.ClipNames.Clear();
                        foreach (var state in states)
                            stateMachine.ClipNames.Add(state.state.name);
                    }
                }
            }
            if (director == null)
                director = root.GetComponentInChildren<PlayableDirector>();
#if SHADER_ANIMATED
            if (stateMachine.meshAnimator == null)
                stateMachine.meshAnimator = root.GetComponentInChildren<ShaderMeshAnimator>();
            if (stateMachine.meshAnimator != null)
            {
                var clips = stateMachine.meshAnimator.animations;
                if (stateMachine.clipNames.Count != clips.Length)
                {
                    stateMachine.ClipNames.Clear();
                    foreach (var clip in clips)
                    {
                        stateMachine.ClipNames.Add(clip.AnimationName);
                    }
                }
            }
#endif
        }

        public override void OnScriptReload()
        {
            if (states != null && states.Length > 0)
            {
                stateMachine.states = states;
                states = null;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            if (stateMachine == null)
                return;
            stateMachine.transform = transform;
            stateMachine.OnScriptReload();
        }
#endif
    }
}