#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.LinearMath;
using SoftFloat;
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
}

public static class JQuaternionEx
{
    public static JVector Mul(this JQuaternion rotation, JVector point)
    {
        sfloat num = rotation.X * 2f;
        sfloat num2 = rotation.Y * 2f;
        sfloat num3 = rotation.Z * 2f;
        sfloat num4 = rotation.X * num;
        sfloat num5 = rotation.Y * num2;
        sfloat num6 = rotation.Z * num3;
        sfloat num7 = rotation.X * num2;
        sfloat num8 = rotation.X * num3;
        sfloat num9 = rotation.Y * num3;
        sfloat num10 = rotation.W * num;
        sfloat num11 = rotation.W * num2;
        sfloat num12 = rotation.W * num3;
        JVector result = default;
        result.X = (1f - (num5 + num6)) * point.X + (num7 - num12) * point.Y + (num8 + num11) * point.Z;
        result.Y = (num7 + num12) * point.X + (1f - (num4 + num6)) * point.Y + (num9 - num10) * point.Z;
        result.Z = (num8 - num11) * point.X + (num9 + num10) * point.Y + (1f - (num4 + num5)) * point.Z;
        return result;
    }

    public static JQuaternion Lerp(this JQuaternion quaternion1, JQuaternion quaternion2, sfloat amount)
    {
        sfloat num1 = amount;
        sfloat num2 = 1f - num1;
        var quaternion = new JQuaternion();
        if (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y +
            quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W >= 0.0)
        {
            quaternion.X = num2 * quaternion1.X + num1 * quaternion2.X;
            quaternion.Y = num2 * quaternion1.Y + num1 * quaternion2.Y;
            quaternion.Z = num2 * quaternion1.Z + num1 * quaternion2.Z;
            quaternion.W = num2 * quaternion1.W + num1 * quaternion2.W;
        }
        else
        {
            quaternion.X = num2 * quaternion1.X - num1 * quaternion2.X;
            quaternion.Y = num2 * quaternion1.Y - num1 * quaternion2.Y;
            quaternion.Z = num2 * quaternion1.Z - num1 * quaternion2.Z;
            quaternion.W = num2 * quaternion1.W - num1 * quaternion2.W;
        }

        sfloat num3 = 1f / libm.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
        quaternion.X *= num3;
        quaternion.Y *= num3;
        quaternion.Z *= num3;
        quaternion.W *= num3;
        return quaternion;
    }

    public static JQuaternion LookRotation(JVector forward, JVector upwards)
    {
        forward = JVector.Normalize(forward);
        var right = JVector.Normalize(JVector.Cross(upwards, forward));
        upwards = JVector.Cross(forward, right);

        var m00 = right.X;
        var m01 = upwards.X;
        var m02 = forward.X;
        var m10 = right.Y;
        var m11 = upwards.Y;
        var m12 = forward.Y;
        var m20 = right.Z;
        var m21 = upwards.Z;
        var m22 = forward.Z;

        var trace = m00 + m11 + m22;
        var q = new JQuaternion();

        if (trace > 0f)
        {
            var num = libm.Sqrt(1f + trace);
            q.W = num * 0.5f;
            num = 0.5f / num;
            q.X = (m21 - m12) * num;
            q.Y = (m02 - m20) * num;
            q.Z = (m10 - m01) * num;
            return q;
        }

        if (m00 >= m11 && m00 >= m22)
        {
            sfloat num = libm.Sqrt(1f + m00 - m11 - m22);
            sfloat num4 = 0.5f / num;
            q.X = 0.5f * num;
            q.Y = (m01 + m10) * num4;
            q.Z = (m02 + m20) * num4;
            q.W = (m12 - m21) * num4;
            return q;
        }

        if (m11 > m22)
        {
            sfloat num6 = libm.Sqrt(1f + m11 - m00 - m22);
            sfloat num3 = 0.5f / num6;
            q.X = (m10 + m01) * num3;
            q.Y = 0.5f * num6;
            q.Z = (m21 + m12) * num3;
            q.W = (m02 - m20) * num3;
            return q;
        }

        var num5 = libm.Sqrt(1f + m22 - m00 - m11);
        var num2 = 0.5f / num5;
        q.X = (m20 + m02) * num2;
        q.Y = (m21 + m12) * num2;
        q.Z = 0.5f * num5;
        q.W = (m01 - m10) * num2;
        return q;
    }
}
#endif