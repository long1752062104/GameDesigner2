using Net.Client;
using Net.Config;
using Cysharp.Threading.Tasks;
using Net.System;
using System;

namespace Net.Distributed
{
    /// <summary>
    /// 负载均衡类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoadBalance<T> where T : ClientBase, new()
    {
        private ConsistentHashing<T> consistentHashing;
        private ClientPool<T> clientPool;
        private FastList<VirtualNode<T>> virtualNodes;
        private int roundRobinCount;
        /// <summary>
        /// 客户端配置属性
        /// </summary>
        public ClientConfig Config { get; set; }
        /// <summary>
        /// 负载均衡配置属性
        /// </summary>
        public LoadBalanceConfig LBConfig { get; set; }
        /// <summary>
        /// 并发线程数量, 发送线程和接收处理线程数量
        /// </summary>
        public int MaxThread { get; set; } = Environment.ProcessorCount * 2;

        /// <summary>
        /// 初始化负载均衡
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async UniTask Init()
        {
            consistentHashing = new ConsistentHashing<T>(LBConfig.Count);
            clientPool = new ClientPool<T>();
            virtualNodes = new FastList<VirtualNode<T>>();
            clientPool.MaxThread = MaxThread;
            clientPool.Init();
            for (int i = 0; i < LBConfig.Items.Count; i++)
            {
                var config = LBConfig.Items[i];
                var nodes = consistentHashing.AddNodeGet(config.Name);
                for (int j = 0; j < nodes.Count; j++)
                {
                    var client = clientPool.Create();
                    client.SetConfig(Config);
                    client.host = config.Host;
                    client.port = config.Port;
                    var connected = await client.Connect();
                    if (!connected)
                        throw new Exception($"连接服务器失败!");
                    nodes[j].Token = client;
                    virtualNodes.Add(nodes[j]);
                }
            }
            clientPool.Start();
        }

        /// <summary>
        /// 获取远程配置数据, 获取配置服务器的数据
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="config"></param>
        /// <param name="protocol"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async UniTask<DATA> RemoteConfig<DATA>(string host, int port, ClientConfig config, int protocol, params object[] args)
        {
            var client = new T();
            client.UpdateMode = Share.NetworkUpdateMode.CustomExecution;
            client.SetConfig(config);
            var connected = await client.Connect(host, port);
            if (!connected)
                throw new Exception("连接配置服务器失败!");
            var eventId = ThreadManager.Invoke(client.SingleNetworkProcessing);
            var data = await client.Request<DATA>(protocol, args);
            if (data == null)
                throw new Exception("获取配置请求失败!");
            client.Close(false);
            ThreadManager.Event.RemoveEvent(eventId);
            return data;
        }

        /// <summary>
        /// 获取一致性哈希的虚拟节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public VirtualNode<T> GetHash(string key)
        {
            return consistentHashing.GetNode(key);
        }

        /// <summary>
        /// 获取轮询的虚拟节点
        /// </summary>
        /// <returns></returns>
        public VirtualNode<T> GetRoundRobin()
        {
            roundRobinCount++;
            return virtualNodes[roundRobinCount % virtualNodes.Count];
        }
    }
}
