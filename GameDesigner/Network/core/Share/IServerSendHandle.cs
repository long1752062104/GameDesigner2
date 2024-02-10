namespace Net.Share
{
    using Net.Server;
    using global::System.Collections.Generic;

    /// <summary>
    /// 服务器发送处理接口
    /// </summary>
    public interface IServerSendHandle<Player> where Player : NetPlayer
    {
        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client">发送数据到的客户端</param>
        /// <param name="buffer">数据缓冲区</param>
        void Send(Player client, byte[] buffer);

        /// <summary>
        /// 发送自定义网络数据
        /// </summary>
        /// <param name="client">发送到客户端</param>
        /// <param name="cmd">网络命令</param>
        /// <param name="buffer">数据缓冲区</param>
        void Send(Player client, byte cmd, byte[] buffer);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client">发送数据到的客户端</param>
        /// <param name="func">RPCFun函数</param>
        /// <param name="pars">RPCFun参数</param>
        void Send(Player client, string func, params object[] pars);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client">发送到的客户端</param>
        /// <param name="cmd">网络命令</param>
        /// <param name="func">RPCFun函数</param>
        /// <param name="pars">RPCFun参数</param>
        void Send(Player client, byte cmd, string func, params object[] pars);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client">发送数据到的客户端</param>
        /// <param name="methodHash">RPCFun函数</param>
        /// <param name="pars">RPCFun参数</param>
        void Send(Player client, ushort methodHash, params object[] pars);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client">发送到的客户端</param>
        /// <param name="cmd">网络命令</param>
        /// <param name="methodHash">RPCFun函数</param>
        /// <param name="pars">RPCFun参数</param>
        void Send(Player client, byte cmd, ushort fmethodHashunc, params object[] pars);

        /// <summary>
        /// 发送灵活数据包
        /// </summary>
        /// <param name="client">客户端集合</param>
        /// <param name="cmd"></param>
        /// <param name="buffer">要包装的数据,你自己来定</param>
        /// <param name="kernel">内核? 你包装的数据在客户端是否被内核NetConvert序列化?</param>
        /// <param name="serialize">序列化? 你包装的数据是否在服务器即将发送时NetConvert序列化?</param>
        void Send(Player client, byte cmd, byte[] buffer, bool kernel, bool serialize);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="func">函数名</param>
        /// <param name="pars">参数</param>
        void SendRT(Player client, string func, params object[] pars);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cmd">网络命令</param>
        /// <param name="func">函数名</param>
        /// <param name="pars">参数</param>
        void SendRT(Player client, byte cmd, string func, params object[] pars);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="methodHash">函数名</param>
        /// <param name="pars">参数</param>
        void SendRT(Player client, ushort methodHash, params object[] pars);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cmd">网络命令</param>
        /// <param name="methodHash">函数名</param>
        /// <param name="pars">参数</param>
        void SendRT(Player client, byte cmd, ushort methodHash, params object[] pars);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        void SendRT(Player client, byte[] buffer);

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cmd">网络命令</param>
        /// <param name="buffer"></param>
        void SendRT(Player client, byte cmd, byte[] buffer);

        /// <summary>
        /// 发送灵活数据包
        /// </summary>
        /// <param name="client">客户端集合</param>
        /// <param name="cmd"></param>
        /// <param name="buffer">要包装的数据,你自己来定</param>
        /// <param name="kernel">内核? 你包装的数据在客户端是否被内核NetConvert序列化?</param>
        /// <param name="serialize">序列化? 你包装的数据是否在服务器即将发送时NetConvert序列化?</param>
        void SendRT(Player client, byte cmd, byte[] buffer, bool kernel, bool serialize);

        /// <summary>
        /// 网络多播, 发送自定义数据到clients集合的客户端
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="buffer">自定义字节数组</param>
        void Multicast(IList<Player> clients, byte[] buffer);

        /// <summary>
        /// 网络多播, 发送自定义数据到clients集合的客户端
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="cmd"></param>
        /// <param name="buffer">自定义字节数组</param>
        void Multicast(IList<Player> clients, byte cmd, byte[] buffer);

        /// <summary>
        /// 网络多播, 发送数据到clients集合的客户端
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="func">本地客户端rpc函数</param>
        /// <param name="pars">本地客户端rpc参数</param>
        void Multicast(IList<Player> clients, string func, params object[] pars);

        /// <summary>
        /// 网络多播, 发送数据到clients集合的客户端
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="cmd">网络命令</param>
        /// <param name="func">本地客户端rpc函数</param>
        /// <param name="pars">本地客户端rpc参数</param>
        void Multicast(IList<Player> clients, byte cmd, string func, params object[] pars);

        /// <summary>
        /// 网络多播, 发送数据到clients集合的客户端
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="methodHash">本地客户端rpc函数</param>
        /// <param name="pars">本地客户端rpc参数</param>
        void Multicast(IList<Player> clients, ushort methodHash, params object[] pars);

        /// <summary>
        /// 网络多播, 发送数据到clients集合的客户端
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="cmd">网络命令</param>
        /// <param name="methodHash">本地客户端rpc函数</param>
        /// <param name="pars">本地客户端rpc参数</param>
        void Multicast(IList<Player> clients, byte cmd, ushort methodHash, params object[] pars);

        /// <summary>
        /// 网络多播, 发送数据到clients集合的客户端 (灵活数据包)
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="reliable">这个包是可靠的吗?</param>
        /// <param name="cmd">网络指令</param>
        /// <param name="buffer">要包装的数据,你自己来定</param>
        /// <param name="kernel">内核? 你包装的数据在客户端是否被内核NetConvert反序列化?</param>
        /// <param name="serialize">序列化? 你包装的数据是否在服务器即将发送时NetConvert序列化?</param>
        void Multicast(IList<Player> clients, byte cmd, byte[] buffer, bool kernel, bool serialize);
    }
}