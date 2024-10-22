using Net.System;
using System;
using System.Reflection;

namespace Net.Share
{
    public delegate void SyncVarInfoDelegate<T, V>(T t, ref V v, ushort id, ref ISegment segment, SyncVarInfo syncVar, bool isWrite, Action<V> onValueChanged);

    [Serializable]
    public class SyncVarInfo
    {
        public ushort id;
        public bool authorize;
        internal MethodInfo onValueChanged;
        internal bool isDispose;
        public int writeCount;
        public int readCount;
        public int writeBytes;
        public int readBytes;

        public bool IsDispose => isDispose;

        internal virtual void SetTarget(object target) { }
        public virtual void SetDefaultValue() { }
        internal virtual void CheckHandlerValue(ref ISegment segment, bool isWrite)
        {
        }
        internal virtual SyncVarInfo Clone(object target)
        {
            return null;
        }
        internal virtual bool EqualsTarget(object target)
        {
            return false;
        }
        internal virtual void SetMemberInfo(MemberInfo memberInfo) { }
        public virtual void Set() { }

        public virtual string ToColorString(string colorName)
        {
            return $"<color={colorName}>{ToString()}</color>";
        }
    }
    
    public class SyncVarInfoPtr<T, V> : SyncVarInfo
    {
        internal T target;
        internal V value;
        internal SyncVarInfoDelegate<T, V> action;
        internal Action<V> onValueChangedEvent;

        public SyncVarInfoPtr(SyncVarInfoDelegate<T, V> action)
        {
            this.action = action;
        }

        internal override SyncVarInfo Clone(object target)
        {
            Action<V> onValueChangedEvent = null;
            if (onValueChanged != null)
                onValueChangedEvent = (Action<V>)onValueChanged.CreateDelegate(typeof(Action<V>), target);
            var segment = BufferPool.Take();
            V value1 = default;
            action((T)target, ref value1, id, ref segment, this, true, onValueChangedEvent);
            segment.Dispose();
            return new SyncVarInfoPtr<T, V>(action) 
            {
                target = (T)target, id = id, authorize = authorize,
                onValueChangedEvent = onValueChangedEvent, value = value1
            };
        }

        internal override void SetTarget(object target)
        {
            this.target = (T)target;
        }

        public override void SetDefaultValue()
        {
            value = default;
        }

        internal override void CheckHandlerValue(ref ISegment segment, bool isWrite)
        {
            action(target, ref value, id, ref segment, this, isWrite, onValueChangedEvent);
        }

        internal override bool EqualsTarget(object target)
        {
            return this.target.Equals(target);
        }

        public override string ToString()
        {
            return $"ID: {id} authorize: {authorize} target: {target.GetType().Name}.{action.Method.Name} writeCount: {writeCount} writeBytes: {writeBytes} readCount: {readCount} readBytes: {readBytes}";
        }

        public override string ToColorString(string colorName)
        {
            return $"<color={colorName}>ID:{id} {target.GetType().Name}.{action.Method.Name}</color> <color=#B78024>writeCount:{writeCount} writeBytes:{writeBytes} readCount:{readCount} readBytes:{readBytes}</color>";
        }
    }
}