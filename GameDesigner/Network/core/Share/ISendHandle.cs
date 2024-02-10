using System;
using System.Threading;

namespace Net.Share
{
    /// <summary>
    /// 发送处理程序接口 
    /// 2019.9.23
    /// </summary>
    public interface ISendHandle
    {
        /// <summary>
        /// 发送自定义网络数据
        /// </summary>
        /// <param name="buffer">数据缓冲区</param>
        void Send(byte[] buffer);

        /// <summary>
        /// 发送自定义网络数据
        /// </summary>
        /// <param name="cmd">网络命令</param>
        /// <param name="buffer">发送字节数组缓冲区</param>
        void Send(byte cmd, byte[] buffer);

        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="func">方法</param>
        /// <param name="pars">参数</param>
        void Send(string func, params object[] pars);

        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="cmd">网络命令</param>
        /// <param name="func">方法</param>
        /// <param name="pars">参数</param>
        void Send(byte cmd, string func, params object[] pars);

        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="func">函数名</param>
        /// <param name="pars">参数</param>
        void SendRT(string func, params object[] pars);

        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="cmd">网络命令</param>
        /// <param name="func">函数名</param>
        /// <param name="pars">参数</param>
        void SendRT(byte cmd, string func, params object[] pars);

        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="buffer"></param>
        void SendRT(byte[] buffer);

        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="cmd">网络命令</param>
        /// <param name="buffer"></param>
        void SendRT(byte cmd, byte[] buffer);
    }
}