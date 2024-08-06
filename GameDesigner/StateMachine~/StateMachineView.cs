using UnityEngine;

namespace GameDesigner
{
    public abstract class StateMachineView : MonoBehaviour
    {
        public StateMachineCore stateMachine;

        public virtual void Init(Transform root)
        {
        }

        public virtual void Execute()
        {
            stateMachine.Execute();
        }

        public virtual void OnDestroy()
        {
            stateMachine.OnDestroy();
        }

#if UNITY_EDITOR
        public virtual void EditorInit(Transform root) { }

        public void OnValidate()
        {
            OnScriptReload();
        }

        public virtual void OnScriptReload()
        {
            if (stateMachine == null)
                return;
            stateMachine.transform = transform;
            stateMachine.OnScriptReload();
        }
#endif
    }
}