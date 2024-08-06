using UnityEngine;
using UnityEngine.Playables;

namespace GameDesigner
{
    public class TimelineStateMachineView : StateMachineView
    {
        public Animator animator;
        public PlayableDirector director;

        public override void Init(Transform root)
        {
            stateMachine.Handler = new TimelineStateMachine(animator, director);
            stateMachine.transform = root;
            stateMachine.Init();
        }

#if UNITY_EDITOR
        public override void EditorInit(Transform root)
        {
            if (director == null)
                director = root.GetComponentInChildren<PlayableDirector>();
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
        }
#endif
    }
}