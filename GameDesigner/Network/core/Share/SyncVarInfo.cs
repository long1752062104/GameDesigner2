using Net.System;
using System;
using System.Reflection;

namespace Net.Share
{
    public delegate void SyncVarInfoDelegate<T, V>(T t, ref V v, ushort id, ref Segment segment, bool isWrite, Action<V, V> onValueChanged);

    [Serializable]
    public class SyncVarInfo
    {
        public ushort id;
        public bool authorize;
        internal MethodInfo onValueChanged;
        internal bool isDispose;
        internal uint tick;

        internal virtual void SetTarget(object target) { }
        public virtual void SetDefaultValue() { }
        internal virtual void CheckHandlerValue(ref Segment segment, bool isWrite)
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
    }
    
    public class SyncVarInfoPtr<T, V> : SyncVarInfo
    {
        internal T target;
        internal V value;
        internal SyncVarInfoDelegate<T, V> action;
        internal Action<V, V> action1;

        public SyncVarInfoPtr(SyncVarInfoDelegate<T, V> action)
        {
            this.action = action;
        }

        internal override SyncVarInfo Clone(object target)
        {
            Action<V, V> action2 = null;
            if (onValueChanged != null)
                action2 = (Action<V, V>)onValueChanged.CreateDelegate(typeof(Action<V, V>), target);
            var segment = BufferPool.Take();
            V value1 = default;
            action((T)target, ref value1, id, ref segment, true, action2);
            segment.Dispose();
            return new SyncVarInfoPtr<T, V>(action) 
            {
                target = (T)target, id = id, authorize = authorize, action1 = action2, value = value1
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

        internal override void CheckHandlerValue(ref Segment segment, bool isWrite)
        {
            action(target, ref value, id, ref segment, isWrite, action1);
        }

        internal override bool EqualsTarget(object target)
        {
            return this.target.Equals(target);
        }

        public override string ToString()
        {
            return $"ID: {id} authorize: {authorize} target: {target.GetType().Name}.{action.Method.Name}";
        }
    }
}