namespace Net.Share
{
    using Net.Server;
    using global::System.Collections.Generic;

    /// <summary>
    /// 服务器发送处理接口
    /// </summary>
    public interface IServerSendHandle<Player> where Player : NetPlayer
    {
        void Call(Player client, byte cmd, byte[] buffer, bool kernel, bool serialize);


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
        void Multicast(IList<Player> clients, int protocol, params object[] pars);

        /// <summary>
        /// 网络多播, 发送数据到clients集合的客户端
        /// </summary>
        /// <param name="clients">客户端集合</param>
        /// <param name="cmd">网络命令</param>
        /// <param name="func">本地客户端rpc函数</param>
        /// <param name="pars">本地客户端rpc参数</param>
        void Multicast(IList<Player> clients, byte cmd, int protocol, params object[] pars);

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