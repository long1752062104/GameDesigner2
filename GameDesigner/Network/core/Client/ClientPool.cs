using System;
using Net.Share;
using Net.System;
using Cysharp.Threading.Tasks;
using Net.Config;

namespace Net.Client
{
    /// <summary>
    /// 客户端对象池
    /// </summary>
    /// <typeparam name="Client"></typeparam>
    public class ClientPool<Client> where Client : ClientBase, new()
    {
        private readonly ThreadPipeline<Client> pool = new ThreadPipeline<Client>();
        /// <summary>
        /// 并发线程数量, 发送线程和接收处理线程数量
        /// </summary>
        public int MaxThread { get; set; } = Environment.ProcessorCount * 2;
        /// <summary>
        /// 服务器主机IP
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 初始化客户端对象池
        /// </summary>
        /// <param name="name"></param>
        public void Init(string name = "networkProcess_")
        {
            pool.MaxThread = MaxThread;
            pool.OnProcess = Process;
            pool.Init(name);
        }

        /// <summary>
        /// 初始化客户端对象池
        /// </summary>
        /// <param name="clientCount">客户端连接池数量</param>
        /// <param name="config">客户端配置</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async UniTaskVoid Init(int clientCount, ClientConfig config)
        {
            Init();
            for (int i = 0; i < clientCount; i++)
            {
                var client = Create();
                client.SetConfig(config);
                client.host = Host;
                client.port = Port;
                var connected = await client.Connect();
                if (!connected)
                    throw new Exception($"连接服务器失败!");
            }
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        public void Start()
        {
            pool.Start();
        }

        /// <summary>
        /// 创建客户端并放入任务池中
        /// </summary>
        /// <param name="config">客户端配置</param>
        /// <returns></returns>
        public Client Create(ClientConfig config = null)
        {
            var client = new Client
            {
                UpdateMode = NetworkUpdateMode.CustomExecution
            };
            client.SetConfig(config);
            pool.AddWorker(client);
            return client;
        }

        /// <summary>
        /// 销毁客户端, 移除任务
        /// </summary>
        /// <param name="client"></param>
        public void Destroy(Client client)
        {
            pool.RemoveWorker(client);
        }

        private void Process(ThreadGroup<Client> group)
        {
            for (int i = 0; i < group.Workers.Count; i++)
            {
                var client = group.Workers[i];
                if (client == null)
                    continue;
                client.SingleNetworkProcessing();
            }
        }
    }
}