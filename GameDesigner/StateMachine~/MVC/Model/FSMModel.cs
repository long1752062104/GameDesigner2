using System;
using UnityEngine;

namespace GameDesigner
{
    [Serializable]
    public class FSMModel
    {
        /// <summary>
        /// 默认状态ID
        /// </summary>
        public int defaulId;
        /// <summary>
        /// 当前运行的状态索引
        /// </summary>
        public int stateId;
        /// <summary>
        /// 切换的状态id
        /// </summary>
        internal int nextId;
        /// <summary>
        /// 切换下一个状态的动作索引
        /// </summary>
        internal int nextActionId;
        /// <summary>
        /// 所有状态
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public State[] states;
        /// <summary>
        /// 以状态ID取出状态对象
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public State this[int stateID] => states[stateID];
        /// <summary>
        /// 获取 或 设置 默认状态
        /// </summary>
        public State defaultState
        {
            get
            {
                if (defaulId < states.Length)
                    return states[defaulId];
                return null;
            }
            set { defaulId = value.ID; }
        }
        /// <summary>
        /// 当前状态
        /// </summary>
        public State currState => states[stateId];
    }
}