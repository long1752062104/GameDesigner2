using GameCore;
using Net.Client;
using UnityEngine;

public static class NetworkExtensions
{
    /// <summary>
    /// 注册的事件在support物体被销毁或关闭时自动移除
    /// </summary>
    /// <param name="self"></param>
    /// <param name="support"></param>
    /// <param name="target"></param>
    /// <param name="append"></param>
    public static void AddRpcAuto(this ClientBase self, MonoBehaviour support, object target, bool append = false)
    {
        AddRpcAuto(self, support, UnEventMode.OnDestroy, target, append);
    }

    /// <summary>
    /// 注册的事件在support物体被销毁或关闭时自动移除
    /// </summary>
    /// <param name="self"></param>
    /// <param name="support"></param>
    /// <param name="eventMode"></param>
    /// <param name="target"></param>
    /// <param name="append"></param>
    public static void AddRpcAuto(this ClientBase self, MonoBehaviour support, UnEventMode eventMode, object target, bool append = false)
    {
        self.AddRpc(target, append);
        if (!support.TryGetComponent<UnEventTrigger>(out var trigger))
            trigger = support.gameObject.AddComponent<UnEventTrigger>();
        trigger.RegisterUnEvents(eventMode, 3, default, new RpcUnRegister(self, target));
    }
}