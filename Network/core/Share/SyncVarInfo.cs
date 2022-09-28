using System;
using System.Reflection;

namespace Net.Share
{
    [Serializable]
    public class SyncVarInfo
    {
        internal object value;
        internal ushort id;
        internal Type type;
        internal bool passive;
        internal bool authorize;
        internal object target;
        internal MethodInfo OnValueChanged;
        internal bool isEnum;
        internal bool baseType;
        internal bool isClass;
        internal bool isDispose;
        internal bool isList;
        internal bool isUnityObject;
        internal MemberInfo member;
        public virtual object GetValue()
        {
            return null;
        }
        public virtual void SetValue(object value)
        {
        }
        public virtual void Init()
        {
        }
        public object GetDefaultValue()
        {
            return isClass & !isUnityObject ? Clone.Instance(GetValue()) : GetValue();
        }
    }
    public class SyncVarFieldInfo : SyncVarInfo
    {
        public FieldInfo fieldInfo;
        public override object GetValue()
        {
            return fieldInfo.GetValue(target);
        }
        public override void SetValue(object value)
        {
            fieldInfo.SetValue(target, value);
        }
        public override void Init()
        {
            fieldInfo = member as FieldInfo;
        }
    }
    public class SyncVarPropertyInfo : SyncVarInfo
    {
        public PropertyInfo propertyInfo;
        public override object GetValue()
        {
            return propertyInfo.GetValue(target);
        }
        public override void SetValue(object value)
        {
            propertyInfo.SetValue(target, value);
        }
        public override void Init()
        {
            propertyInfo = member as PropertyInfo;
        }
    }
}
