﻿
using Cysharp.Threading.Tasks;

namespace Net.Share
{
    /// <summary>
    /// 发送处理程序接口 
    /// 2019.9.23
    /// </summary>
    public interface ISendHandle
    {
        #region 远程过程调用
        void Call(int protocol, params object[] pars);
        void Call(byte cmd, int protocol, params object[] pars);
        void Call(byte[] buffer);
        void Call(byte cmd, byte[] buffer);
        void Call(string func, params object[] pars);
        void Call(byte cmd, string func, params object[] pars);
        void Call(byte cmd, int protocol, byte[] buffer, params object[] pars);
        void Call(RPCModel model);
        #endregion

        #region 同步远程调用, 跟Http协议一样, 请求必须有回应 请求和回应方法都是相同的, 都是根据protocol请求和回应
        UniTask<RPCModelTask> Request(int protocol, params object[] pars);
        UniTask<RPCModelTask> Request(int protocol, int timeoutMilliseconds, params object[] pars);
        UniTask<RPCModelTask> Request(int protocol, int timeoutMilliseconds, bool intercept, params object[] pars);
        UniTask<RPCModelTask> Request(byte cmd, int protocol, int timeoutMilliseconds, params object[] pars);
        UniTask<RPCModelTask> Request(byte cmd, int protocol, int timeoutMilliseconds, bool intercept, params object[] pars);
        UniTask<RPCModelTask> Request(byte cmd, int protocol, int timeoutMilliseconds, bool intercept, bool serialize, byte[] buffer, params object[] pars);
        #endregion
    }
}