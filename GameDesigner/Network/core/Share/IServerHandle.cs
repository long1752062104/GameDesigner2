using Net.Server;
using System.Collections.Concurrent;

namespace Net.Share
{
    /// <summary>
    /// 用户对接基类服务器
    /// </summary>
    public interface IServerHandle<Player> : IServerSendHandle<Player>, IServerEventHandle<Player>, IRpcHandler where Player : NetPlayer
    {
        /// <summary>
        /// 服务器是否处于运行状态, 如果服务器套接字已经被释放则返回False, 否则返回True. 当调用Close方法后将改变状态
        /// </summary>
        bool IsRunServer { get; set; }

        /// <summary>
        /// 从所有在线玩家字典中删除(移除)玩家实体
        /// </summary>
        /// <param name="player"></param>
        void DeletePlayer(Player player);

        /// <summary>
        /// 从所有在线玩家字典中移除玩家实体
        /// </summary>
        /// <param name="player"></param>
        void RemovePlayer(Player player);

        /// <summary>
        /// 从客户端字典中移除客户端
        /// </summary>
        /// <param name="client"></param>
        void RemoveClient(Player client);
    }

    /// <summary>
    /// 用户对接基类服务器
    /// </summary>
    public interface IServerHandle<Player, Scene> : IServerHandle<Player>, INetworkSceneHandle<Player, Scene> where Player : NetPlayer where Scene : NetScene<Player>
    {
        /// <summary>
        /// 服务器场景，每个key都处于一个场景或房间，关卡，value是场景对象
        /// </summary>
        ConcurrentDictionary<string, Scene> Scenes { get; set; }
    }
}
