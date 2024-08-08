using Net.System;
using UnityEngine;

namespace GameDesigner
{
    public class StateMachineSystem : MonoBehaviour
    {
        private static StateMachineSystem instance;
        public static StateMachineSystem Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!ThreadManager.IsRuning)
                    return instance;
#endif
                if (instance == null)
                {
                    instance = FindObjectOfType<StateMachineSystem>(true);
                    if (instance == null)
                        instance = new GameObject("StateMachineSystem").AddComponent<StateMachineSystem>();
                }
                return instance;
            }
        }
        public FastList<IStateMachine> stateMachines = new FastList<IStateMachine>();

        private void Awake()
        {
            if (instance == null)
                instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            for (int i = 0; i < stateMachines._size; i++)
            {
                if (stateMachines._items[i].UpdateMode == StateMachineUpdateMode.Update)
                    stateMachines._items[i].Execute();
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < stateMachines._size; i++)
            {
                if (stateMachines._items[i].UpdateMode == StateMachineUpdateMode.LateUpdate)
                    stateMachines._items[i].Execute();
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < stateMachines._size; i++)
            {
                if (stateMachines._items[i].UpdateMode == StateMachineUpdateMode.FixedUpdate)
                    stateMachines._items[i].Execute();
            }
        }

        public static void AddStateMachine(IStateMachine stateMachine)
        {
            var i = Instance;
            if (i == null | stateMachine == null)
                return;
            i.stateMachines.Add(stateMachine);
        }

        public static void RemoveStateMachine(IStateMachine stateMachine)
        {
            var i = Instance;
            if (i == null | stateMachine == null)
                return;
            i.stateMachines.Remove(stateMachine);
        }
    }
}