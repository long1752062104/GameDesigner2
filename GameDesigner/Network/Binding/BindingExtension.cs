using Binding;
using Net.System;

public static class BindingExtension
{

    public static ISegment SerializeObject(this Net.Vector2 value)
    {
        var segment = BufferPool.Take();
        var bind = new NetVector2Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Vector2 DeserializeObject(this Net.Vector2 value, ISegment segment, bool isPush = true)
    {
        var bind = new NetVector2Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Vector2 value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineVector2Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Vector2 DeserializeObject(this UnityEngine.Vector2 value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineVector2Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Vector2Int value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineVector2IntBind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Vector2Int DeserializeObject(this UnityEngine.Vector2Int value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineVector2IntBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Vector3 value)
    {
        var segment = BufferPool.Take();
        var bind = new NetVector3Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Vector3 DeserializeObject(this Net.Vector3 value, ISegment segment, bool isPush = true)
    {
        var bind = new NetVector3Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Vector3 value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineVector3Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Vector3 DeserializeObject(this UnityEngine.Vector3 value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineVector3Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Vector3Int value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineVector3IntBind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Vector3Int DeserializeObject(this UnityEngine.Vector3Int value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineVector3IntBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Vector4 value)
    {
        var segment = BufferPool.Take();
        var bind = new NetVector4Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Vector4 DeserializeObject(this Net.Vector4 value, ISegment segment, bool isPush = true)
    {
        var bind = new NetVector4Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Vector4 value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineVector4Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Vector4 DeserializeObject(this UnityEngine.Vector4 value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineVector4Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Quaternion value)
    {
        var segment = BufferPool.Take();
        var bind = new NetQuaternionBind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Quaternion DeserializeObject(this Net.Quaternion value, ISegment segment, bool isPush = true)
    {
        var bind = new NetQuaternionBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Quaternion value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineQuaternionBind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Quaternion DeserializeObject(this UnityEngine.Quaternion value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineQuaternionBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Rect value)
    {
        var segment = BufferPool.Take();
        var bind = new NetRectBind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Rect DeserializeObject(this Net.Rect value, ISegment segment, bool isPush = true)
    {
        var bind = new NetRectBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Rect value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineRectBind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Rect DeserializeObject(this UnityEngine.Rect value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineRectBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.RectInt value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineRectIntBind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.RectInt DeserializeObject(this UnityEngine.RectInt value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineRectIntBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Color value)
    {
        var segment = BufferPool.Take();
        var bind = new NetColorBind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Color DeserializeObject(this Net.Color value, ISegment segment, bool isPush = true)
    {
        var bind = new NetColorBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Color32 value)
    {
        var segment = BufferPool.Take();
        var bind = new NetColor32Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Color32 DeserializeObject(this Net.Color32 value, ISegment segment, bool isPush = true)
    {
        var bind = new NetColor32Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Color value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineColorBind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Color DeserializeObject(this UnityEngine.Color value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineColorBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this UnityEngine.Color32 value)
    {
        var segment = BufferPool.Take();
        var bind = new UnityEngineColor32Bind();
        bind.Write(value, segment);
        return segment;
    }

    public static UnityEngine.Color32 DeserializeObject(this UnityEngine.Color32 value, ISegment segment, bool isPush = true)
    {
        var bind = new UnityEngineColor32Bind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Share.Operation value)
    {
        var segment = BufferPool.Take();
        var bind = new NetShareOperationBind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Share.Operation DeserializeObject(this Net.Share.Operation value, ISegment segment, bool isPush = true)
    {
        var bind = new NetShareOperationBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

    public static ISegment SerializeObject(this Net.Share.OperationList value)
    {
        var segment = BufferPool.Take();
        var bind = new NetShareOperationListBind();
        bind.Write(value, segment);
        return segment;
    }

    public static Net.Share.OperationList DeserializeObject(this Net.Share.OperationList value, ISegment segment, bool isPush = true)
    {
        var bind = new NetShareOperationListBind();
        bind.Read(ref value, segment);
        if (isPush) BufferPool.Push(segment);
        return value;
    }

}