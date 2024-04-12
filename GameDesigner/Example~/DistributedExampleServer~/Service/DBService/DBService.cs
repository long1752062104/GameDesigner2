using Net.Server;
using Net.Client;
using Net.Distributed;
using Net.Share;
using Cysharp.Threading.Tasks;
using Net.System;
using Distributed;
using Net.Event;

namespace DistributedExample
{
    public class DBService : TcpServer
    {
        private DataCacheDictionary<string, DataCache<UserData>> CacheDict = new DataCacheDictionary<string, DataCache<UserData>>();
        public DistributedDB distributedDB;

        public async void Init(string name, string machineId)
        {
            var loadBalance = new LoadBalance<TcpClient>();
            var config = new Net.Config.ClientConfig()
            {
                SendBufferSize = 1024 * 1024 * 20,
                ReceiveBufferSize = 1024 * 1024 * 20,
                LimitQueueCount = 1024 * 1024 * 5,
                ReconnectCount = int.MaxValue,
            };
            var itemConfig = await loadBalance.RemoteConfig<ItemConfig>("127.0.0.1", 10240, config, (int)ProtoType.RegisterConfig, "DBService", name);
            Start((ushort)itemConfig.Port);
            distributedDB = new DistributedDB
            {
                BatchSize = 100000,
                SqlBatchSize = 1024 * 1024 * 5
            };
            distributedDB.ConnectionBuilder.Database = name;
            distributedDB.CreateTables("root", name);
            distributedDB.InitTablesId(5, true, int.Parse(machineId), 16);
            ThreadManager.Invoke(distributedDB.BatchWorker, true);
            AreaName = name;
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
            var token = client.Token;
            var userCache = CacheDict.GetOrCreate(account);
            var data = await userCache.QueryOrGetAsync(() => distributedDB.QueryAsync<UserData>($"account = '{account}'"));
            if (data == null)
            {
                NDebug.LogError($"哈希错乱! {AreaName} {account}");
                Response(client, ProtoType.Login, token, -1, null);
                return;
            }
            if (data.Password != password)
            {
                NDebug.LogError($"密码错误! {AreaName} {account}");
                Response(client, ProtoType.Login, token, -2, null);
                return;
            }
            Response(client, ProtoType.Login, token, 0, data);
        }

        private async UniTaskVoid Register(NetPlayer client, string account, string password)
        {
            var token = client.Token;
            var userCache = CacheDict.GetOrCreate(account);
            int code = -1;
            await userCache.QueryOrGetAsync(() => distributedDB.QueryAsync<UserData>($"account = '{account}'"), () =>
            {
                var data = new UserData
                {
                    //Id = distributedDB.GetUniqueId(DistributedUniqueIdType.User),
                    Account = account,
                    Password = password,
                    Level = 1,
                    Context = distributedDB
                };
                data.NewTableRow();
                code = 0;
                return data;
            });
            Response(client, ProtoType.Register, token, code);
        }
    }
}
