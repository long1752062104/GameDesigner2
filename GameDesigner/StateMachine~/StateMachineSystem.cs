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
        public FastList<IStateManager> stateManagers = new FastList<IStateManager>();

        private void Awake()
        {
            if (instance == null)
                instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            for (int i = 0; i < stateManagers._size; i++)
            {
                stateManagers._items[i].Execute();
            }
        }

        public static void AddStateManager(StateManager stateManager)
        {
            var i = Instance;
            if (i == null)
                return;
            i.stateManagers.Add(stateManager);
        }

        public static void RemoveStateManager(StateManager stateManager)
        {
            var i = Instance;
            if (i == null)
                return;
            i.stateManagers.Remove(stateManager);
        }
    }
}