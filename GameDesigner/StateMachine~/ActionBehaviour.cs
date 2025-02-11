﻿namespace GameDesigner
{
    using System;

    /// <summary>
    /// 动作行为--用户添加的组件 v2017/12/6
    /// </summary>
    [Serializable]
    public class ActionBehaviour : BehaviourBase
    {
        /// <summary>
        /// 当进入状态
        /// </summary>
        /// <param name="action">当前动作</param>
        public virtual void OnEnter(StateAction action) { }

        /// <summary>
        /// 当更新状态
        /// </summary>
        /// <param name="action">当前动作</param>
        public virtual void OnUpdate(StateAction action) { }

        /// <summary>
        /// 当状态晚于更新
        /// </summary>
        public virtual void OnLateUpdate(StateAction action) { }

        /// <summary>
        /// 当状态固定更新
        /// </summary>
        public virtual void OnFixedUpdate(StateAction action) { }

        /// <summary>
        /// 当退出状态
        /// </summary>
        /// <param name="action">当前动作</param>
        public virtual void OnExit(StateAction action) { }

        /// <summary>
        /// 当停止动作 : 当动作不使用动画循环时, 动画时间到达100%后调用
        /// </summary>
        /// <param name="action"></param>
        public virtual void OnStop(StateAction action) { }
    }
}