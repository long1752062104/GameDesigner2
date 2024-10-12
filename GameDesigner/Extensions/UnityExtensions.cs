#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;

public static class UnityExtensions
{
    /// <summary>
    /// 将给定方向向量转换为相对于指定变换（通常是摄像机）的方向。
    /// </summary>
    /// <param name="cameraTarget">目标变换，通常是摄像机的变换。</param>
    /// <param name="direction">要转换的方向向量。</param>
    /// <returns>转换后的方向向量。</returns>
    public static Vector3 Transform3Dir(this Transform cameraTarget, Vector3 direction)
    {
        var f = Mathf.Deg2Rad * (-cameraTarget.rotation.eulerAngles.y);
        direction.Normalize();
        var ret = new Vector3(direction.x * Mathf.Cos(f) - direction.z * Mathf.Sin(f), 0, direction.x * Mathf.Sin(f) + direction.z * Mathf.Cos(f));
        return ret;
    }

    /// <summary>
    /// 将给定方向向量转换为相对于指定变换（通常是摄像机）的方向。
    /// </summary>
    /// <param name="cameraTarget">目标变换，通常是摄像机的变换。</param>
    /// <returns>转换后的方向向量。</returns>
    public static Vector3 Transform3Dir(this Transform cameraTarget) => Transform3Dir(cameraTarget, InputEx.Direction);

    /// <summary>
    /// 对指定的变换进行线性插值旋转，朝向给定的方向。
    /// </summary>
    /// <param name="transform">要旋转的变换。</param>
    /// <param name="direction">目标方向向量。</param>
    /// <param name="t">插值因子，范围通常在0到1之间，默认值为0.5。</param>
    public static void LerpRotation(this Transform transform, Vector3 direction, float t = 0.5f)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), t);
    }

    /// <summary>
    /// 向量相乘
    /// </summary>
    /// <param name="self"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Vector3 Multiply(this Vector3 self, Vector3 value)
    {
        return new Vector3(self.x * value.x, self.y * value.y, self.z * value.z);
    }

    public static Vector2 PointToAxis(this Rect rectangle, Vector2 point)
    {
        var normalized = Rect.PointToNormalized(rectangle, point);
        var axis = new Vector2(normalized.x * 2f - 1f, normalized.y * 2f - 1f);
        return axis;
    }
}

public static class InputEx
{
    /// <summary>
    /// 获取当前的方向向量，基于用户的输入。
    /// </summary>
    /// <remarks>
    /// 方向向量的x分量来自“Horizontal”输入，y分量为0，z分量来自“Vertical”输入。
    /// </remarks>
    public static Vector3 Direction => new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

    /// <summary>
    /// 获取当前的方向向量，基于用户的输入。
    /// </summary>
    /// <remarks>
    /// 方向向量的x分量来自“Horizontal”输入，y分量来自“Vertical”输入。
    /// </remarks>
    public static Vector2 Direction2D => new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
}

#endif