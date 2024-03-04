using Net.Serialize;
using Net.Share;
using Net.System;

namespace Net.Adapter
{
    /// <summary>
    /// 快速序列化适配器
    /// </summary>
    public class SerializeAdapter : ISerializeAdapter
    {
        public void OnSerializeRpc(ISegment segment, RPCModel model)
        {
            NetConvertBinary.SerializeModel(segment, model);
        }

        public FuncData OnDeserializeRpc(ISegment segment)
        {
            return NetConvertBinary.DeserializeModel(segment);
        }

        public byte[] OnSerializeOpt(in OperationList list)
        {
            return NetConvertFast2.SerializeObject(list).ToArray(true);
        }

        public OperationList OnDeserializeOpt(ISegment segment)
        {
            return NetConvertFast2.DeserializeObject<OperationList>(segment, false);
        }
    }

    /// <summary>
    /// 通用升级版适配器
    /// </summary>
    public class SerializeFastAdapter : ISerializeAdapter
    {
        public void OnSerializeRpc(ISegment segment, RPCModel model)
        {
            NetConvertFast.Serialize(segment, model);
        }

        public FuncData OnDeserializeRpc(ISegment segment)
        {
            return NetConvertFast.Deserialize(segment);
        }

        public byte[] OnSerializeOpt(in OperationList list)
        {
            return NetConvertFast2.SerializeObject(list).ToArray(true);
        }

        public OperationList OnDeserializeOpt(ISegment segment)
        {
            return NetConvertFast2.DeserializeObject<OperationList>(segment, false);
        }
    }

    /// <summary>
    /// 快速序列化2适配器
    /// </summary>
    public class SerializeAdapter2 : ISerializeAdapter
    {
        public void OnSerializeRpc(ISegment segment, RPCModel model)
        {
            NetConvertBinary.SerializeModel(segment, model);
        }

        public FuncData OnDeserializeRpc(ISegment segment)
        {
            return NetConvertBinary.DeserializeModel(segment);
        }

        public byte[] OnSerializeOpt(in OperationList list)
        {
            return NetConvertFast2.SerializeObject(list).ToArray(true);
        }

        public OperationList OnDeserializeOpt(ISegment segment)
        {
            return NetConvertFast2.DeserializeObject<OperationList>(segment, false);
        }
    }

    /// <summary>
    /// 极速序列化3适配器
    /// </summary>
    public class SerializeAdapter3 : ISerializeAdapter
    {
        public void OnSerializeRpc(ISegment segment, RPCModel model)
        {
            NetConvertFast2.SerializeModel(segment, model);
        }

        public FuncData OnDeserializeRpc(ISegment segment)
        {
            return NetConvertFast2.DeserializeModel(segment, false);
        }

        public byte[] OnSerializeOpt(in OperationList list)
        {
            return NetConvertFast2.SerializeObject(list).ToArray(true);
        }

        public OperationList OnDeserializeOpt(ISegment segment)
        {
            return NetConvertFast2.DeserializeObject<OperationList>(segment, false);
        }
    }
}