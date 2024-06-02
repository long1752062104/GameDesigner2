using System;
using System.Reflection;
using System.Collections.Generic;
using Net.Share;
using Net.Event;

namespace Net.Helper
{
    /// <summary>
    /// MySqlBuild属性同步接口
    /// </summary>
    public interface ISyncProperty
    {
        /// <summary>
        /// 设置属性同步
        /// </summary>
        /// <param name="pars"></param>
        void SetProperty(object[] pars);
    }

    /// <summary>
    /// 属性同步基础类，是单个类，如UserData
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyncProperty<T> : ISyncProperty where T : IDataEntity
    {
        private readonly T data;
        private readonly int index;
        private readonly string name;

        /// <summary>
        /// 属性同步构造
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        public SyncProperty(T data, int index, string name)
        {
            this.data = data;
            this.index = index;
            this.name = name;
        }

        /// <inheritdoc/>
        public void SetProperty(object[] pars)
        {
            data[index] = pars[0];
#if SERVICE
            data.Update(false);
#endif
        }

        public override string ToString()
        {
            return $"name = {name} index = {index} data = {data}";
        }
    }

    /// <summary>
    /// 列表或者数组型的属性同步类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyncPropertyList<T> : ISyncProperty where T : IDataEntity
    {
        private readonly IList<T> datas;
        private readonly int index;
        private readonly string name;

        /// <summary>
        /// 列表或数组类型构造
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        public SyncPropertyList(IList<T> datas, int index, string name)
        {
            this.datas = datas;
            this.index = index;
            this.name = name;
        }

        /// <inheritdoc/>
        public void SetProperty(object[] pars)
        {
            if (pars.Length == 1)
            {
                NDebug.LogError($"{typeof(T)}类的{name}属性同步设置错误，请使用SyncID{name} = xxx;进行属性同步!");
                return;
            }
            for (int i = 0; i < datas.Count; i++)
            {
                if (Equals(datas[i][0], pars[0]))
                {
                    datas[i][index] = pars[1];
#if SERVICE
                    datas[i].Update(false);
#endif
                    return;
                }
            }
            NDebug.LogError($"{typeof(T)}属性同步设置失败: Id:{pars[0]} FieldIdx:{index} Value:{pars[1]}");
        }

        public override string ToString()
        {
            return $"name = {name} index = {index} datas = {datas}";
        }
    }

    /// <summary>
    /// MySqlBuild属性同步帮助类
    /// </summary>
    public class SyncPropertyHelper
    {
        /// <summary>
        /// 属性同步字典
        /// </summary>
        public Dictionary<uint, ISyncProperty> SyncPropertyDict = new Dictionary<uint, ISyncProperty>();

        /// <summary>
        /// 添加属性同步，数组或者List类型，并且这个是引用地址
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="syncProperty"></param>
        public void AddSyncPropertys<T>(IList<T> datas, ISyncProperty syncProperty = null) where T : IDataEntity
        {
            AddSyncProperty<T>((rpc, field) => syncProperty ?? new SyncPropertyList<T>(datas, field.index, field.name)); //用引用地址，后面增删才会同步
        }

        /// <summary>
        /// 添加基础的属性同步类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void AddSyncProperty<T>(T data) where T : IDataEntity
        {
            AddSyncProperty<T>((rpc, field) => new SyncProperty<T>(data, field.index, field.name));
        }

        /// <summary>
        /// 添加属性同步，通过onCreateSyncProperty委托自己构造SyncProperty对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onCreateSyncProperty"></param>
        public void AddSyncProperty<T>(Func<Rpc, DataRowField, ISyncProperty> onCreateSyncProperty) where T : IDataEntity
        {
            var methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                Rpc rpc = null;
                DataRowField field = null;
                foreach (var attribute in method.GetCustomAttributes())
                {
                    if (attribute is Rpc rpc1)
                        rpc = rpc1;
                    else if (attribute is DataRowField field1)
                        field = field1;
                }
                if (rpc != null && field != null)
                    SyncPropertyDict[rpc.hash] = onCreateSyncProperty(rpc, field);
            }
        }

        /// <summary>
        /// 属性同步处理
        /// </summary>
        /// <param name="model"></param>
        public void SyncPropertyHandler(RPCModel model)
        {
            if (SyncPropertyDict.TryGetValue(model.protocol, out var syncProperty))
            {
                syncProperty.SetProperty(model.pars);
            }
        }
    }
}
