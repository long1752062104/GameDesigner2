using System;
using Net.Share;
using Net.Client;
using Cysharp.Threading.Tasks;

public static class ClientBaseExtensions
{
    #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据protocol请求和回应
    public static UniTask<RPCModelTask> Request(this ClientBase self, Enum protocol, params object[] pars)
        => self.Request(NetCmd.CallRpc, (uint)protocol.GetHashCode(), 5000U, true, false, null, pars);
    public static UniTask<RPCModelTask> Request(this ClientBase self, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => self.Request(NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, false, null, pars);
    public static UniTask<RPCModelTask> Request(this ClientBase self, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => self.Request(NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, false, null, pars);
    public static UniTask<RPCModelTask> Request(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => self.Request(cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, false, null, pars);
    public static UniTask<RPCModelTask> Request(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => self.Request(cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, false, null, pars);
    #endregion

    #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据funcAndCb请求和回应
    public static UniTask<T> Request<T>(this ClientBase self, uint protocol, params object[] pars)
        => Request<T>(self, NetCmd.CallRpc, protocol, 5000U, true, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, intercept, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T>(self, cmd, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T>(self, cmd, protocol, timeoutMilliseconds, intercept, null, pars);

    public static UniTask<T> Request<T>(this ClientBase self, Enum protocol, params object[] pars)
        => Request<T>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), 5000U, true, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<T> Request<T>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    #endregion

    #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据funcAndCb请求和回应
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, uint protocol, params object[] pars)
        => Request<T, T1>(self, NetCmd.CallRpc, protocol, 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1>(self, cmd, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1>(self, cmd, protocol, timeoutMilliseconds, intercept, null, pars);

    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, Enum protocol, params object[] pars)
        => Request<T, T1>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, byte cmd, Enum protocol, params object[] pars)
        => Request<T, T1>(self, cmd, (uint)protocol.GetHashCode(), 5000U, true, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1>> Request<T, T1>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    #endregion

    #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据funcAndCb请求和回应
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, uint protocol, params object[] pars)
        => Request<T, T1, T2>(self, NetCmd.CallRpc, protocol, 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2>(self, cmd, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2>(self, cmd, protocol, timeoutMilliseconds, intercept, null, pars);

    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, Enum protocol, params object[] pars)
        => Request<T, T1, T2>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    #endregion

    #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据funcAndCb请求和回应
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, uint protocol, params object[] pars)
        => Request<T, T1, T2, T3>(self, NetCmd.CallRpc, protocol, 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3>(self, cmd, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3>(self, cmd, protocol, timeoutMilliseconds, intercept, null, pars);

    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, Enum protocol, params object[] pars)
        => Request<T, T1, T2, T3>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    #endregion

    #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据funcAndCb请求和回应
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, uint protocol, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, NetCmd.CallRpc, protocol, 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, NetCmd.CallRpc, protocol, timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, cmd, protocol, timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, cmd, protocol, timeoutMilliseconds, intercept, null, pars);

    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, Enum protocol, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), 5000, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, NetCmd.CallRpc, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, true, null, pars);
    public static UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(this ClientBase self, byte cmd, Enum protocol, uint timeoutMilliseconds, bool intercept, params object[] pars)
        => Request<T, T1, T2, T3, T4>(self, cmd, (uint)protocol.GetHashCode(), timeoutMilliseconds, intercept, null, pars);
    #endregion

    private static async UniTask<T> Request<T>(ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, byte[] buffer, params object[] pars)
    {
        var task = await self.Request(cmd, protocol, timeoutMilliseconds, intercept, false, buffer, pars);
        if (!task.IsCompleted)
            return default;
        return (T)task.model.pars[0];
    }

    private static async UniTask<ValueTuple<T, T1>> Request<T, T1>(ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, byte[] buffer, params object[] pars)
    {
        var task = await self.Request(cmd, protocol, timeoutMilliseconds, intercept, false, buffer, pars);
        if (!task.IsCompleted)
            return new ValueTuple<T, T1>(default, default);
        return new ValueTuple<T, T1>((T)task.model.pars[0], (T1)task.model.pars[1]);
    }

    private static async UniTask<ValueTuple<T, T1, T2>> Request<T, T1, T2>(ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, byte[] buffer, params object[] pars)
    {
        var task = await self.Request(cmd, protocol, timeoutMilliseconds, intercept, false, buffer, pars);
        if (!task.IsCompleted)
            return new ValueTuple<T, T1, T2>(default, default, default);
        return new ValueTuple<T, T1, T2>((T)task.model.pars[0], (T1)task.model.pars[1], (T2)task.model.pars[2]);
    }

    private static async UniTask<ValueTuple<T, T1, T2, T3>> Request<T, T1, T2, T3>(ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, byte[] buffer, params object[] pars)
    {
        var task = await self.Request(cmd, protocol, timeoutMilliseconds, intercept, false, buffer, pars);
        if (!task.IsCompleted)
            return new ValueTuple<T, T1, T2, T3>(default, default, default, default);
        return new ValueTuple<T, T1, T2, T3>((T)task.model.pars[0], (T1)task.model.pars[1], (T2)task.model.pars[2], (T3)task.model.pars[3]);
    }

    private static async UniTask<ValueTuple<T, T1, T2, T3, T4>> Request<T, T1, T2, T3, T4>(ClientBase self, byte cmd, uint protocol, uint timeoutMilliseconds, bool intercept, byte[] buffer, params object[] pars)
    {
        var task = await self.Request(cmd, protocol, timeoutMilliseconds, intercept, false, buffer, pars);
        if (!task.IsCompleted)
            return new ValueTuple<T, T1, T2, T3, T4>(default, default, default, default, default);
        return new ValueTuple<T, T1, T2, T3, T4>((T)task.model.pars[0], (T1)task.model.pars[1], (T2)task.model.pars[2], (T3)task.model.pars[3], (T4)task.model.pars[4]);
    }

    #region 远程过程调用扩展
    public static void Call(this ClientBase self, Enum protocol, params object[] pars) => self.Call((uint)protocol.GetHashCode(), pars);
    public static void Call(this ClientBase self, byte cmd, Enum protocol, params object[] pars) => self.Call(cmd, (uint)protocol.GetHashCode(), pars);
    #endregion

    public static void DispatchRpc(this ClientBase self, Enum protocol, params object[] pars) => self.DispatchRpc((uint)protocol.GetHashCode(), pars);
}