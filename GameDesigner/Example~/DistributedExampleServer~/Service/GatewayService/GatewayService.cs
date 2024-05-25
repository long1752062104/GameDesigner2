using Cysharp.Threading.Tasks;
using Distributed;
using Net.Client;
using Net.Distributed;
using Net.Server;
using Net.Share;

namespace DistributedExample
{
    internal class GatewayService : TcpServer
    {
        private readonly LoadBalance<TcpClient> loadBalance = new LoadBalance<TcpClient>();

        public async void Init(string name)
        {
            var config = new Net.Config.ClientConfig()
            {
                SendBufferSize = 1024 * 1024 * 20,
                ReceiveBufferSize = 1024 * 1024 * 20,
                LimitQueueCount = 1024 * 1024 * 5,
                ReconnectCount = int.MaxValue,
            };
            var itemConfig = await loadBalance.RemoteConfig<ItemConfig>("127.0.0.1", 10240, config, (int)ProtoType.RegisterConfig, GlobalConfig.GatewayService, name);
            var lbConfig = await loadBalance.RemoteConfig<LoadBalanceConfig>("127.0.0.1", 10240, config, (int)ProtoType.LoadBalanceConfig, GlobalConfig.LoginService);
            loadBalance.Config = config;
            loadBalance.LBConfig = lbConfig;
            //loadBalance.MaxThread = 1;
            //loadBalance.LBConfig.Count = 1;
            await loadBalance.Init();
            Start((ushort)itemConfig.Port);
            //Console.Title = $"{name} {itemConfig.Port}";
            OnNetworkDataTraffic += (df) =>
            {
                Console.Title = $"{name} {itemConfig.Port} {df}";
            };
        }

        protected override void SceneUpdateHandle()
        {
        }

        protected override void OnHasConnect(NetPlayer client)
        {
            base.OnHasConnect(client);
        }

        protected override bool OnUnClientRequest(NetPlayer unClient, RPCModel model)
        {
            switch ((ProtoType)model.protocol)
            {
                case ProtoType.Register:
                    _ = Register(unClient, model.AsString, model.AsString);
                    break;
                case ProtoType.Login:
                    _ = Login(unClient, model.AsString, model.AsString);
                    break;
            }
            return false;
        }

        private async UniTaskVoid Login(NetPlayer client, string account, string password)
        {
            var token = client.Token; //记录请求响应token, 避免await后丢失
            var loginClient = loadBalance.GetRoundRobin().Token; //获取负载均衡轮询客户端对象
            var (code, user) = await loginClient.Request<int, UserData>(ProtoType.Login, GlobalConfig.RequestTimeoutMilliseconds, account, password); //网关向登录服务器发出请求
            Response(client, ProtoType.Login, token, code, user); //发给unity客户端
            //if (code == 0)
            //    LoginHandler(client);
        }

        private async UniTaskVoid Register(NetPlayer client, string account, string password)
        {
            //NDebug.Log("注册");
            var token = client.Token;
            var loginClient = loadBalance.GetRoundRobin().Token;
            var code = await loginClient.Request<int>(ProtoType.Register, GlobalConfig.RequestTimeoutMilliseconds, account, password);
            Response(client, ProtoType.Register, token, code);
        }
    }
}
