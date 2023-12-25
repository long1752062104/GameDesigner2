using WebSocketSharp;

namespace Net.Server
{
    /// <summary>
    /// http客户端对象
    /// </summary>
    public class HttpPlayer : NetPlayer
    {
        /// <summary>
        /// webSocket套接字
        /// </summary>
        public WebSocket WSClient { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            WSClient?.Close();
        }
    }
}