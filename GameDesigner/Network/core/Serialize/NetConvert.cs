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
        /// <returns></returns>
        public static byte[] Serialize(RPCModel model, byte[] flag = null, bool recordType = false)
        {
            var segment = BufferPool.Take();
            Serialize(segment, model, flag, recordType);
            return segment.ToArray(true);
        }

        public static void Serialize(ISegment segment, RPCModel model, byte[] flag = null, bool recordType = false)
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
                NetConvertBinary.SerializeObject(segment, obj, recordType, true);
            }
        }

        /// <summary>
        /// 新版反序列化
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="recordType"></param>
        public static FuncData Deserialize(ISegment segment, bool recordType = false)
        {
            FuncData fdata = default;
            try
            {
                fdata.protocol = segment.ReadUInt32();
                var list = new List<object>();
                while (segment.Position < segment.Offset + segment.Count)
                {
                    var typeName = segment.ReadString();
                    var type = AssemblyHelper.GetType(typeName);
                    if (type == null)
                    {
                        fdata.error = true;
                        break;
                    }
                    if (type == typeof(DBNull))
                    {
                        list.Add(null);
                        continue;
                    }
                    var obj = NetConvertBinary.ReadObject(segment, type, recordType, true);
                    list.Add(obj);
                }
                fdata.pars = list.ToArray();
            }
            catch (Exception ex)
            {
                fdata.error = true;
                var func = RPCExtensions.GetFunc(fdata.protocol);
                NDebug.LogError($"反序列化{func}出错:{ex}");
            }
            return fdata;
        }
    }
}