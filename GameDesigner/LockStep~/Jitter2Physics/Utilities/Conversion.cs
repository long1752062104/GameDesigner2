#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.LinearMath;
using UnityEngine;

public static class Conversion
{
    public static Vector3 ToVector3(this JVector v)
    {
        return new Vector3(-v.X, v.Y, v.Z);
    }

    public static JVector ToJVector(this Vector3 v)
    {
        return new JVector(-v.x, v.y, v.z);
    }

    public static Quaternion ToQuaternion(this JQuaternion v)
    {
        return new Quaternion(-v.X, v.Y, v.Z, -v.W);
    }

    public static JQuaternion ToQuaternion(this Quaternion v)
    {
        return new JQuaternion(-v.x, v.y, v.z, -v.w);
    }

    public static Vector4 ToVector4(this JQuaternion v)
    {
        return new Vector4(-v.X, v.Y, v.Z, -v.W);
    }

    public static JQuaternion ToQuaternion(this Vector4 v)
    {
        return new JQuaternion(-v.x, v.y, v.z, -v.w);
    }

    public static JVector ToJVector(this Net.Vector3 self)
    {
        return new JVector(self.x, self.y, self.z);
    }
}
#endif