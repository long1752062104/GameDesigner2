﻿namespace GameDesigner
{
    using Net.Helper;
    using System;

    /// <summary>
    /// 连接行为--用户可以继承此类添加组件 2017年12月6日(星期三)
    /// </summary>
    [Serializable]
    public class TransitionBehaviour : BehaviourBase
    {
        [HideField]
        public int transitionID;
        public Transition Transition => state.transitions[transitionID];
        public virtual void OnUpdate(ref bool isEnterNextState) { }
        public TransitionBehaviour InitBehaviour()
        {
            var type = AssemblyHelper.GetType(name);
            var runtimeBehaviour = (TransitionBehaviour)Activator.CreateInstance(type);
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