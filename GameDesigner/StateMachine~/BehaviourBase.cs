using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Net.Helper;
using Unity;
using Object = UnityEngine.Object;
using TypeCode = Unity.TypeCode;

namespace GameDesigner
{
    /// <summary>
    /// 状态机脚本不显示的字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class HideField : Attribute { }

    /// <summary>
    /// 状态行为基类 2019.3.3
    /// </summary>
    [Serializable]
    public class BehaviourBase
    {
        [HideField]
        public string name;
        [HideField]
        public int ID;
        /// <summary>
        /// 展开编辑器检视面板
        /// </summary>
        [HideField]
        [HideInInspector]
        public bool show = true;
        /// <summary>
        /// 脚本是否启用?
        /// </summary>
        [HideField]
        public bool Active = true;
        [HideField]
        public List<Metadata> metadatas;
        public List<Metadata> Metadatas
        {
            get
            {
                metadatas ??= new List<Metadata>();
                return metadatas;
            }
            set { metadatas = value; }
        }
        public IStateMachine stateMachine;
        /// <summary>
        /// 当前状态
        /// </summary>
        public State state => stateMachine.States[ID];
        /// <summary>
        /// 当前状态机挂载在物体的父转换对象
        /// </summary>
        public Transform transform => stateMachine.transform;
        public Type Type { get { return AssemblyHelper.GetType(name); } }
        public void InitMetadatas()
        {
            var type = GetType();
            InitMetadatas(type);
        }
        public void InitMetadatas(Type type)
        {
            name = type.ToString();
            var fields = type.GetFields();
            Metadatas.Clear();
            foreach (var field in fields)
            {
                if (field.IsStatic | field.GetCustomAttribute<HideField>() != null)
                    continue;
                InitField(field);
            }
        }

        private void InitField(FieldInfo field)
        {
            var code = Type.GetTypeCode(field.FieldType);
            if (code == System.TypeCode.Object)
            {
                if (field.FieldType.IsSubclassOf(typeof(Object)) | field.FieldType == typeof(Object))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Object, this, field));
                else if (field.FieldType == typeof(Vector2))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector2, this, field));
                else if (field.FieldType == typeof(Vector3))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector3, this, field));
                else if (field.FieldType == typeof(Vector4))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector4, this, field));
                else if (field.FieldType == typeof(Quaternion))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Quaternion, this, field));
                else if (field.FieldType == typeof(Rect))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Rect, this, field));
                else if (field.FieldType == typeof(Color))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Color, this, field));
                else if (field.FieldType == typeof(Color32))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Color32, this, field));
                else if (field.FieldType == typeof(AnimationCurve))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.AnimationCurve, this, field));
                else if (field.FieldType.IsGenericType)
                {
                    var gta = field.FieldType.GenericTypeArguments;
                    if (gta.Length > 1)
                        return;
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.GenericType, this, field));
                }
                else if (field.FieldType.IsArray)
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Array, this, field));
            }
            else if (field.FieldType.IsEnum)
                Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Enum, this, field));
            else Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), (TypeCode)code, this, field));
        }

        public void Reload(Type type, List<Metadata> metadatas)
        {
            InitMetadatas(type);
            foreach (var item in Metadatas)
            {
                foreach (var item1 in metadatas)
                {
                    if (item.name == item1.name & item.typeName == item1.typeName)
                    {
                        item.data = item1.data;
                        item.Value = item1.Value;
                        item.Values = item1.Values;
#if UNITY_EDITOR
                        item.arraySize = item1.arraySize;
                        item.foldout = item1.foldout;
#endif
                        item.field.SetValue(this, item1.value);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 当初始化调用
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// 当组件被删除调用一次
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// 当绘制编辑器检视面板 (重要提示!你想自定义编辑器检视面板则返回真,否则显示默认编辑器检视面板)
        /// </summary>
        /// <param name="state">当前状态</param>
        /// <returns></returns>
        public virtual bool OnInspectorGUI(State state)
        {
            return false; //返回假: 绘制默认监视面板 | 返回真: 绘制扩展自定义监视面板
        }

        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        public void EnterState(int stateID, int actionId = 0) => stateMachine.StatusEntry(stateID, actionId);

        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="stateID">下一个状态的ID</param>
        public void OnEnterNextState(int stateID, int actionId = 0) => stateMachine.EnterNextState(stateID, actionId);

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="force"></param>
        public void ChangeState(int stateId, int actionId = 0, bool force = false) => stateMachine.ChangeState(stateId, actionId, force);

        /// <summary>
        /// 初始化真实类型并且赋值记录的值
        /// </summary>
        /// <returns></returns>
        public BehaviourBase InitBehaviour(IStateMachine stateMachine)
        {
            var type = AssemblyHelper.GetType(name);
            var runtimeBehaviour = (BehaviourBase)Activator.CreateInstance(type);
            runtimeBehaviour.stateMachine = stateMachine;
            runtimeBehaviour.Active = Active;
            runtimeBehaviour.ID = ID;
            runtimeBehaviour.name = name;
            runtimeBehaviour.Metadatas = Metadatas;
            runtimeBehaviour.show = show;
            foreach (var metadata in Metadatas)
            {
                var field = type.GetField(metadata.name);
                if (field == null)
                    continue;
                var value = metadata.Read();//必须先读值才能赋值下面字段和对象
                metadata.field = field;
                metadata.target = runtimeBehaviour;
                field.SetValue(runtimeBehaviour, value);
            }
            var lateUpdateMethod = type.GetMethod("OnLateUpdate");
            var fixedUpdateMethod = type.GetMethod("OnFixedUpdate");
            var root = stateMachine;
            while (root != null)
            {
                if (root.Parent == null) //最后一层
                    break;
                root = root.Parent;
            }
            if ((lateUpdateMethod.DeclaringType == type) && (stateMachine.UpdateMode & StateMachineUpdateMode.LateUpdate) == 0)
                root.UpdateMode |= StateMachineUpdateMode.LateUpdate;
            if ((fixedUpdateMethod.DeclaringType == type) && (stateMachine.UpdateMode & StateMachineUpdateMode.FixedUpdate) == 0)
                root.UpdateMode |= StateMachineUpdateMode.FixedUpdate;
            return runtimeBehaviour;
        }
    }
}