#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.LinearMath;
using UnityEngine;

public static class Conversion
{
    public static Vector3 ToVector3(this NVector3 v)
    {
        return new Vector3(-v.X, v.Y, v.Z);
    }

    public static NVector3 ToNVector3(this Vector3 v)
    {
        return new NVector3(-v.x, v.y, v.z);
    }

    public static Quaternion ToQuaternion(this NQuaternion v)
    {
        return new Quaternion(-v.X, v.Y, v.Z, -v.W);
    }

    public static NQuaternion ToQuaternion(this Quaternion v)
    {
        return new NQuaternion(-v.x, v.y, v.z, -v.w);
    }

    public static Vector4 ToVector4(this NQuaternion v)
    {
        return new Vector4(-v.X, v.Y, v.Z, -v.W);
    }

    public static NQuaternion ToQuaternion(this Vector4 v)
    {
        return new NQuaternion(-v.x, v.y, v.z, -v.w);
    }

    public static NVector3 ToNVector3(this Net.Vector3 self)
    {
        return new NVector3(self.x, self.y, self.z);
    }
}
#endif