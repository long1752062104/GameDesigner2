using Net.Component;

namespace GameCore
{
    /// <summary>
    /// 传输协议
    /// </summary>
    public enum TransportProtocol
    {
        Tcp, Gcp, Udx, Kcp, Web
    }

    public class NetworkManager : NetworkManagerBase
    {
    }
}