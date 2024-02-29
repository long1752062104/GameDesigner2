using System.Collections.Generic;
using UnityEngine;

namespace GameDesigner
{
    /// <summary>
    /// 状态基类
    /// </summary>
    [System.Serializable]
    public class StateBase
    {
        public string name;
        public int ID, perID;
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public BehaviourBase[] behaviours;// = new BehaviourBase[0];
        public StateMachine stateMachine;
        public StateManager stateManager => stateMachine.stateManager;

#if UNITY_EDITOR
        [HideInInspector]
        public bool foldout = true;
        public Rect rect;// = new Rect(10, 10, 150, 30);
#endif

        /// <summary>
        /// 添加状态行为组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : BehaviourBase, new()
        {
            return AddComponent(new T());
        }

        /// <summary>
        /// 添加状态行为组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public T AddComponent<T>(T component) where T : BehaviourBase
        {
            component.name = component.GetType().ToString();
            component.ID = ID;
            component.stateMachine = stateMachine;
            ArrayExtend.Add(ref behaviours, component);
            return component;
        }

        public void AddComponent(params BehaviourBase[] behaviours)
        {
            if (behaviours == null)
                return;
            foreach (var component in behaviours)
            {
                component.name = component.GetType().ToString();
                component.ID = ID;
                component.stateMachine = stateMachine;
                ArrayExtend.Add(ref this.behaviours, component);
            }
        }

        /// <summary>
        /// 获取状态组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : BehaviourBase
        {
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] is T component)
                    return component;
            return null;
        }

        /// <summary>
        /// 获取多个状态组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetComponents<T>() where T : BehaviourBase
        {
            var components = new List<T>();
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] is T component)
                    components.Add(component);
            return components.ToArray();
        }
    }
}