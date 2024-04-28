﻿using Net.Serialize;
using Net.System;
using System;
using System.Reflection;

namespace Net.Share
{
    /// <summary>
    /// 网络同步字段或属性类  --相对[SyncVar]性能要好一些
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SyncVariable<T> : SyncVarInfo
    {
        [UnityEngine.SerializeField]
        private T value;
        /// <summary>
        /// 设置值并同步到云端, 并且调用值修改事件
        /// </summary>
        public T Value
        {
            get => value;
            set => Set(value, true);
        }
        /// <summary>
        /// 设置值不通知，但是会同步到云端
        /// </summary>
        public T ValueNot
        {
            get => value;
            set => Set(value, false);
        }
        private bool isChanged;
        private object target;
        private MemberInfo memberInfo;
        public Action<T> OnValueChanged { get; set; }

        public SyncVariable() { }
        public SyncVariable(T value) : this(0, value, null) { }
        public SyncVariable(Action<T> onValueChanged) : this(0, default, onValueChanged) { }
        public SyncVariable(T value, Action<T> onValueChanged) : this(0, value, onValueChanged) { }
        public SyncVariable(ushort id, T value, Action<T> onValueChanged)
        {
            this.id = id;
            this.value = value;
            OnValueChanged = onValueChanged;
        }

        private void Set(T value, bool notify)
        {
            isChanged = true;
            this.value = value;
            if (notify) OnValueChanged?.Invoke(value);
        }

        public override void Set() //unity编辑器修改属性值
        {
            isChanged = true;
            OnValueChanged?.Invoke(value);
        }

        internal override void SetMemberInfo(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
        }

        internal override SyncVarInfo Clone(object target)
        {
            SyncVariable<T> syncVarInfo;
            if (memberInfo is FieldInfo fieldInfo)
                syncVarInfo = fieldInfo.GetValue(target) as SyncVariable<T>;
            else if (memberInfo is PropertyInfo propertyInfo)
                syncVarInfo = propertyInfo.GetValue(target) as SyncVariable<T>;
            else
                return null;
            syncVarInfo.target = target;
            syncVarInfo.memberInfo = memberInfo;
            return syncVarInfo;
        }

        internal override void SetTarget(object target)
        {
            this.target = target;
        }

        internal override void CheckHandlerValue(ref ISegment segment, bool isWrite)
        {
            if (isWrite)
            {
                if (!isChanged)
                    return;
                isChanged = false;
                segment ??= BufferPool.Take();
                segment.Write(id);
                NetConvertBinary.SerializeObject(segment, value, false, true);
            }
            else
            {
                var value = NetConvertBinary.DeserializeObject<T>(segment, false, false, true);
                this.value = value;
                OnValueChanged?.Invoke(value);
            }
        }

        public override void SetDefaultValue()
        {
        }

        internal override bool EqualsTarget(object target)
        {
            return this.target.Equals(target);
        }

        public override string ToString()
        {
            return $"ID: {id} authorize: {authorize} target: {target.GetType().Name}.{memberInfo.Name}";
        }
    }
}
