namespace Net.Serialize
{
    using Net.Event;
    using global::System;
    using global::System.Collections.Generic;
    using Net.Share;
    using Net.System;
    using Net.Helper;

    /// <summary>
    /// 网络转换核心 2019.7.16
    /// </summary>
    public class NetConvert : NetConvertBase
    {
        /// <summary>
        /// 新版网络序列化
        /// </summary>
        /// <param name="model">函数名</param>
        /// <param name="flag"></param>
        /// <param name="recordType"></param>
        /// <returns></returns>
        public static byte[] Serialize(RPCModel model, byte[] flag = null, bool recordType = false)
        {
            var segment = BufferPool.Take();
            Serialize(segment, model, flag, recordType);
            return segment.ToArray(true);
        }

        public static bool Serialize(ISegment segment, RPCModel model, byte[] flag = null, bool recordType = false)
        {
            try
            {
                if (flag != null) segment.Write(flag, 0, flag.Length);
                segment.Write(model.protocol);
                foreach (object obj in model.pars)
                {
                    Type type;
                    if (obj == null)
                    {
                        type = typeof(DBNull);
                        segment.Write(type.ToString());
                        continue;
                    }
                    type = obj.GetType();
                    segment.Write(type.ToString());
                    NetConvertBinary.WriteObject(segment, type, obj, recordType, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                var func = RPCExtensions.GetFunc(model.protocol);
                NDebug.LogError($"序列化:{func}出错,如果提示为索引溢出,你可以在Call或者Response方法直接设置serialize参数为true 详情:{ex}");
                return false;
            }
        }

        /// <summary>
        /// 新版反序列化
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="model"></param>
        /// <param name="recordType"></param>
        public static bool Deserialize(ISegment segment, RPCModel model, bool recordType = false)
        {
            try
            {
                model.protocol = segment.ReadUInt32();
                var list = new List<object>();
                while (segment.Position < segment.Offset + segment.Count)
                {
                    var typeName = segment.ReadString();
                    var type = AssemblyHelper.GetType(typeName);
                    if (type == null)
                        return false;
                    if (type == typeof(DBNull))
                    {
                        list.Add(null);
                        continue;
                    }
                    var obj = NetConvertBinary.ReadObject(segment, type, recordType, true);
                    list.Add(obj);
                }
                model.pars = list.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                var func = RPCExtensions.GetFunc(model.protocol);
                NDebug.LogError($"反序列化:{func}出错:{ex}");
                return false;
            }
        }
    }
}