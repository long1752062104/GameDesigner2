#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    /// 方向向量的x分量来自“Horizontal”输入，y分量来自“Vertical”输入，z分量为0。
    /// </remarks>
    public static Vector3 Direction3D => new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

    /// <summary>
    /// 获取当前的方向向量，基于用户的输入。
    /// </summary>
    /// <remarks>
    /// 方向向量的x分量来自“Horizontal”输入，y分量来自“Vertical”输入。
    /// </remarks>
    public static Vector2 Direction2D => new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
}

public static class UIDragHandlerEx
{
    /// <summary>
    /// 获取拖动偏移，这是在OnBeginDrag事件获取偏移并且保存起来
    /// </summary>
    /// <param name="self"></param>
    /// <param name="panelRt">面板的矩形</param>
    /// <param name="eventData">事件数据</param>
    /// <returns></returns>
    public static Vector2 GetDragOffset(this Graphic self, RectTransform panelRt, PointerEventData eventData)
    {
        Vector2 dragOffset = default;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            dragOffset = self.rectTransform.anchoredPosition - localPoint;
        return dragOffset;
    }

    /// <summary>
    /// 获取拖动偏移，这是在OnBeginDrag事件获取偏移并且保存起来
    /// </summary>
    /// <param name="self"></param>
    /// <param name="panelRt">面板的矩形</param>
    /// <param name="eventData">事件数据</param>
    /// <returns></returns>
    public static void GetDragOffset(this Graphic self, RectTransform panelRt, PointerEventData eventData, out Vector2 dragOffset)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        dragOffset = self.rectTransform.anchoredPosition - localPoint;
    }

    /// <summary>
    /// 拖动UI，这里在OnDrag方法使用
    /// </summary>
    /// <param name="self"></param>
    /// <param name="panelRt">面板的矩形</param>
    /// <param name="eventData">事件数据</param>
    /// <param name="dragOffset">拖动偏移量</param>
    public static void DragUI(this Graphic self, RectTransform panelRt, PointerEventData eventData, Vector2 dragOffset)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            self.rectTransform.anchoredPosition = localPoint + dragOffset;
    }

    /// <summary>
    /// 拖动UI在Update，可在Update进行调用
    /// </summary>
    /// <param name="self"></param>
    /// <param name="panelRt">面板的矩形</param>
    /// <param name="uiCamera">UI相机</param>
    /// <param name="dragOffset">拖动偏移量</param>
    public static void UpdateDragUI(this Graphic self, RectTransform panelRt, Camera uiCamera, Vector2 dragOffset)
    {
        Vector2 mousePosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRt, mousePosition, uiCamera, out Vector2 localPoint);
        self.rectTransform.anchoredPosition = localPoint + dragOffset;
    }

    /// <summary>
    /// 设置拖动阈值, 提高拖动灵敏度
    /// 由于Unity的事件系统有一个默认的拖动阈值（通常是 5 像素），只有当鼠标移动超过这个阈值时，OnDrag 才会被调用。为了解决这个问题，你可以通过自定义一个拖动处理来实现更灵敏的响应。
    /// </summary>
    /// <param name="self"></param>
    public static void DragThreshold(int dragThreshold = 1)
    {
        EventSystem.current.pixelDragThreshold = dragThreshold;
    }

    /// <summary>
    /// 获取屏幕位置偏移, 获取鼠标点击的位置和图像自身矩形的位置偏移
    /// </summary>
    /// <param name="self"></param>
    /// <param name="eventData">事件数据</param>
    /// <returns></returns>
    public static Vector2 GetScreenPositionOffset(this Graphic self, RectTransform panelRt, PointerEventData eventData)
    {
        var screenPosition = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, self.rectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRt, eventData.position, eventData.pressEventCamera, out Vector2 mouseLocalPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRt, screenPosition, eventData.pressEventCamera, out Vector2 thisLocalPoint);
        return thisLocalPoint - mouseLocalPoint;
    }
}

#endif