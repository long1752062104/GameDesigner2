﻿using Net.Helper;
using System;

namespace GameDesigner
{
    /// <summary>
    /// 状态行为脚本 v2017/12/6 - 2020.6.7
    /// </summary>
    [Serializable]
    public class StateBehaviour : IBehaviour
    {
        /// <summary>
        /// 当状态进入时
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 当状态每一帧
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// 当状态退出后
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 当停止动作 : 当动作不使用动画循环时, 动画时间到达100%后调用
        /// </summary>
        public virtual void OnStop() { }

        /// <summary>
        /// 当动作处于循环模式时, 子动作动画每次结束都会调用一次
        /// </summary>
        public virtual void OnActionExit() { }

        public StateBehaviour InitBehaviour()
        {
            var type = AssemblyHelper.GetType(name);
            var runtimeBehaviour = (StateBehaviour)Activator.CreateInstance(type);
            runtimeBehaviour.stateMachine = stateMachine;
            runtimeBehaviour.Active = Active;
            runtimeBehaviour.ID = ID;
            runtimeBehaviour.name = name;
            runtimeBehaviour.metadatas = metadatas;
            foreach (var metadata in metadatas)
            {
                var field = type.GetField(metadata.name);
                if (field == null)
                    continue;
                var value = metadata.Read();//必须先读值才能赋值下面字段和对象
                metadata.field = field;
                metadata.target = runtimeBehaviour;
                field.SetValue(runtimeBehaviour, value);
            }
            return runtimeBehaviour;
        }
    }
}