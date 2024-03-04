using Cysharp.Threading.Tasks;
using Distributed;
using Net.Client;
using Net.Distributed;
using Net.Event;
using Net.Server;
using Net.Share;

namespace DistributedExample
{
    /// <summary>
    /// 登录服务器, 也可以称为账号服务器
    /// </summary>
    public class LoginService : TcpServer
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
            var itemConfig = await loadBalance.RemoteConfig<ItemConfig>("127.0.0.1", 10240, config, (int)ProtoType.RegisterConfig, "LoginService", name);
            var lbConfig = await loadBalance.RemoteConfig<LoadBalanceConfig>("127.0.0.1", 10240, config, (int)ProtoType.LoadBalanceConfig, "DBService");
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
            var token = client.Token; //先保存现场，下面代码有await会切换线程，会导致token丢失
            var node = loadBalance.GetHash(account); //获取此账号负载均衡DB服务器节点
            var dbClient = node.Token; //拿到DB服务器的连接
            var (code, user) = await dbClient.Request<int, UserData>((uint)ProtoType.Login, 1000 * 30, account, password); //向DB服务器发出请求, 查询数据库账号
            Response(client, (int)ProtoType.Login, token, code, user); //响应客户端，客户端用await等等
            //Call(client, (int)ProtoType.Login, code, user); //如果客户端用Call发起，则用Call回应，不需要token
            //LoginHandler(client);
        }

        private async UniTaskVoid Register(NetPlayer client, string account, string password)
        {
            //NDebug.Log("注册");
            var token = client.Token;
            var node = loadBalance.GetHash(account);
            var dbClient = node.Token;
            var code = await dbClient.Request<int>((int)ProtoType.Register, 1000 * 30, account, password);
            Response(client, (int)ProtoType.Register, token, code);
        }
    }
}
