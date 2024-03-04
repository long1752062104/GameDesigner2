using Net.Server;
using Net.Client;
using Net.Distributed;
using Net.Share;
using Cysharp.Threading.Tasks;
using Net.System;
using Net.Common;
using Distributed;
using Net.Event;

namespace DistributedExample
{
    public class DBService : TcpServer
    {
        private DataCacheDictionary<string, DataCache<UserData>> CacheDict = new DataCacheDictionary<string, DataCache<UserData>>();
        private readonly FastLocking locking = new FastLocking();
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
            locking.Enter();
            bool firstAdd = false;
            if (!CacheDict.TryGetValue(account, out var user))
            {
                CacheDict.Add(account, user = new DataCache<UserData>());
                firstAdd = true; //小锁, 速度快
            }
            locking.Exit();
            user.Locking.Enter(); //查询锁, 查询时间比较久
            if (firstAdd)
                await user.QueryOrGetAsync(() => distributedDB.QueryAsync<UserData>($"account = '{account}'"));
            user.Locking.Exit();
            if (user.Data == null)
            {
                NDebug.LogError($"哈希错乱! {AreaName} {account}");
                Response(client, (int)ProtoType.Login, token, -1, null);
                return;
            }
            if (user.Data.Password != password)
            {
                NDebug.LogError($"密码错误! {AreaName} {account}");
                Response(client, (int)ProtoType.Login, token, -2, null);
                return;
            }
            Response(client, (int)ProtoType.Login, token, 0, user.Data);
        }

        private async UniTaskVoid Register(NetPlayer client, string account, string password)
        {
            var token = client.Token;
            locking.Enter();
            int code = -1;
            bool firstAdd = false;
            if (!CacheDict.TryGetValue(account, out var user))
            {
                CacheDict.Add(account, user = new DataCache<UserData>());
                firstAdd = true; //小锁, 速度快
            }
            locking.Exit();
            user.Locking.Enter(); //查询锁, 查询时间比较久
            if (firstAdd)
                await user.QueryOrGetAsync(() => distributedDB.QueryAsync<UserData>($"account = '{account}'"));
            if (user.Data == null)
            {
                var data = new UserData
                {
                    Id = distributedDB.GetUniqueId(DistributedUniqueIdType.User),
                    Account = account,
                    Password = password,
                    Level = 1,
                    Context = distributedDB
                };
                data.NewTableRow();
                user.Data = data;
                code = 0;
            }
            user.Locking.Exit();
            Response(client, (int)ProtoType.Register, token, code);
        }
    }
}
