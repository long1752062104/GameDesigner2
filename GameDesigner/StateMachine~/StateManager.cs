using UnityEngine;
#if SHADER_ANIMATED
using FSG.MeshAnimator.ShaderAnimated;
#endif

namespace GameDesigner
{
    public enum RuntimeInitMode
    {
        Awake, Start,
    }

    /// <summary>
    /// 状态执行管理类
    /// V2017.12.6
    /// 版本修改V2019.8.27
    /// </summary>
    public sealed class StateManager : MonoBehaviour
    {
        /// <summary>
        /// 状态机
        /// </summary>
		public StateMachine stateMachine = null;
        public RuntimeInitMode initMode = RuntimeInitMode.Awake;

        void Awake()
        {
            if (initMode == RuntimeInitMode.Awake)
                Init();
        }

        void Start()
        {
            if (initMode == RuntimeInitMode.Start)
                Init();
        }

        private void Init()
        {
            if (stateMachine == null)
            {
                enabled = false;
                return;
            }
            if (stateMachine.GetComponentInParent<StateManager>() == null)//当使用本地公用状态机时
            {
                var sm = Instantiate(stateMachine, transform);
                sm.name = stateMachine.name;
                sm.transform.localPosition = Vector3.zero;
                if (sm.animation == null)
                    sm.animation = GetComponentInChildren<Animation>();
                else if (!sm.animation.gameObject.scene.isLoaded)
                    sm.animation = GetComponentInChildren<Animation>();
                if (sm.animator == null)
                    sm.animator = GetComponentInChildren<Animator>();
                else if (!sm.animator.gameObject.scene.isLoaded)
                    sm.animator = GetComponentInChildren<Animator>();
#if SHADER_ANIMATED
                if (sm.meshAnimator == null)
                    sm.meshAnimator = GetComponentInChildren<ShaderMeshAnimator>();
                else if (!sm.meshAnimator.gameObject.scene.isLoaded)
                    sm.meshAnimator = GetComponentInChildren<ShaderMeshAnimator>();
#endif
                stateMachine = sm;
            }
            stateMachine.Init();
        }

        public void Execute()
        {
            stateMachine.Execute();
        }

        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="nextStateIndex">下一个状态的ID</param>
		public void EnterNextState(int nextStateIndex) => stateMachine.EnterNextState(nextStateIndex);

        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        public void StatusEntry(int stateID) => stateMachine.StatusEntry(stateID);

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="force"></param>
        public void ChangeState(int stateId, bool force = false) => stateMachine.ChangeState(stateId, force);

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (stateMachine == null)
                return;
            stateMachine.OnScriptReload();
        }
#endif

        private void OnEnable()
        {
            StateMachineSystem.AddStateManager(this);
        }

        private void OnDisable()
        {
            StateMachineSystem.RemoveStateManager(this);
        }
    }
}