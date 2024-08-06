using UnityEngine;

namespace GameDesigner
{
    public class AnimationStateMachineView : StateMachineView
    {
        public new Animation animation;

        public override void Init(Transform root)
        {
            stateMachine.Handler = new AnimationStateMachine(animation);
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
        }
#endif
    }
}