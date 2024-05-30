using Cysharp.Threading.Tasks;
using Distributed;
using Net.Client;
using Net.Distributed;
using System.Data;

namespace DistributedExample
{
    public class DBManager
    {
        private readonly LoadBalance<TcpClient> loadBalance = new LoadBalance<TcpClient>();
        private ConsistentHashing<TcpClient> consistentHashing;
        private Dictionary<string, DistributedDB> mysqlNodes;

        public async void Init()
        {
            //var config = new Net.Config.ClientConfig()
            //{
            //    SendBufferSize = 1024 * 1024 * 20,
            //    ReceiveBufferSize = 1024 * 1024 * 20,
            //    LimitQueueCount = 1024 * 1024 * 5,
            //    ReconnectCount = int.MaxValue,
            //};
            //var lbConfig = await loadBalance.RemoteConfig<LoadBalanceConfig>("127.0.0.1", 10240, config, (int)ProtoType.LoadBalanceConfig, GlobalConfig.DBService);
            //await UniTask.SwitchToThreadPool();
            consistentHashing = new ConsistentHashing<TcpClient>(/*lbConfig.Count*/);
            mysqlNodes = new Dictionary<string, DistributedDB>();
            //for (int i = 0; i < lbConfig.Items.Count; i++)
            //{
            //    var item = lbConfig.Items[i];
            //    CreateDB(item.Name, item.Args);
            //}
            int affectedCount = 0;
            string nodeName = "";
            int state = 0;
            while (true)
            {
                Console.WriteLine("1.输入:n nodeName 01新增1个数据库节点,机器号为01");
                Console.WriteLine("2.输入:d 10000新增1万个数据");
                Console.WriteLine("3.输入:n- nodeName移除1个数据库节点");
                var command = Console.ReadLine();
                if (command.StartsWith("d"))
                {
                    var countText = command.Remove(0, 2);
                    var count = int.Parse(countText);
                    var random = new Random(977579129);
                    for (int i = 0; i < count; i++)
                    {
                        var data = new UserData
                        {
                            Account = Guid.NewGuid().ToString() + random.Next(int.MinValue, int.MaxValue).ToString()
                        };
                        var virtualNode = consistentHashing.GetNode(data.Account);
                        data.Context = mysqlNodes[virtualNode.PhysicalNodeName];
                        data.NewTableRow();
                    }
                    affectedCount = 0;
                    await Task.Delay(1000);
                    foreach (var node in mysqlNodes.Values)
                    {
                        var dataCount = node.ExecuteScalar<long>($"SELECT COUNT(*) FROM `user`;");
                        Console.WriteLine($"节点:{node.ConnectionBuilder.Database} 数据量:{dataCount}");
                    }
                    continue;
                }
                if (command.StartsWith("n-"))
                {
                    nodeName = command.Remove(0, 3);
                    if (!mysqlNodes.ContainsKey(nodeName))
                    {
                        Console.WriteLine("要移除的节点不存在!");
                        continue;
                    }
                    state = 2;
                }
                else if (command.StartsWith("n"))
                {
                    nodeName = command.Split(" ")[1];
                    if (mysqlNodes.ContainsKey(nodeName))
                    {
                        Console.WriteLine("新增的节点已存在!");
                        continue;
                    }
                    state = 1;
                }
                // 获取受影响的节点列表
                var affectedNodes = consistentHashing.GetAffectedNodes(nodeName);
                if (state == 1)
                {
                    var agrs = command.Split(" ");
                    CreateDB(agrs[1], agrs[2]);
                }
                else
                {
                    consistentHashing.RemoveNode(nodeName);
                }
                // 输出受影响的节点
                Console.WriteLine("受影响的节点：");
                foreach (var node in affectedNodes)
                {
                    Console.WriteLine($"虚拟节点:{node.VirtualNodeName} 物理节点:{node.PhysicalNodeName}");
                }
                var physicalNodeNames = affectedNodes.Select(item => item.PhysicalNodeName).ToHashSet();
                foreach (var node in physicalNodeNames)
                {
                    //找出这些节点所在的数据库
                    var gameDB = mysqlNodes[node];
                    var userIdMax = gameDB.ExecuteScalar<long>(@"SELECT MAX(id) FROM `user`;");
                    long currRowsCount = 0;
                    do
                    {
                        long size = 200000;
                        if (currRowsCount + size >= userIdMax)
                            size = userIdMax - currRowsCount;
                        var users = gameDB.ExecuteQueryList<UserData>($"SELECT * FROM `user` WHERE id >= {currRowsCount} AND id <= {currRowsCount + size};");
                        currRowsCount += size;
                        for (int i = users.Length - 1; i >= 0; i--)
                        {
                            var data = users[i];
                            var virtualNode = consistentHashing.GetNode(data.Account); //重新计算要迁移到哪个节点
                            if (virtualNode.PhysicalNodeName == node) //重新计算得出, 这个数据不需要迁移
                                continue;
                            data.Delete(); //从旧节点移除这个数据
                            var newData = new UserData()
                            {
                                Account = data.Account,
                                Password = data.Password,
                                Name = data.Name,
                                Level = data.Level,
                            };
                            newData.Context = mysqlNodes[virtualNode.PhysicalNodeName]; //迁移数据到新的节点去
                            newData.NewTableRow();
                            affectedCount++;
                        }
                    }
                    while (currRowsCount < userIdMax);
                }
                if (state == 2)
                {
                    mysqlNodes[nodeName].Stop();
                    mysqlNodes.Remove(nodeName);
                }
                Console.WriteLine("迁移数据量:" + affectedCount);
                affectedCount = 0;
                await Task.Delay(1000);
                foreach (var node in mysqlNodes.Values)
                {
                    //检查数据是否冗余
                    var gameDB = node;
                    var userIdMax = gameDB.ExecuteScalar<long>(@"SELECT MAX(id) FROM `user`;");
                    long currRowsCount = 0;
                    long dataCount = 0;
                    do
                    {
                        long size = 200000;
                        if (currRowsCount + size >= userIdMax)
                            size = userIdMax - currRowsCount;
                        var users = gameDB.ExecuteQueryList<UserData>($"SELECT * FROM `user` WHERE id >= {currRowsCount} AND id <= {currRowsCount + size};");
                        currRowsCount += size;
                        dataCount += users.LongLength;
                        for (int i = users.Length - 1; i >= 0; i--)
                        {
                            var data = users[i];
                            var virtualNode = consistentHashing.GetNode(data.Account); //重新计算要迁移到哪个节点
                            if (virtualNode.PhysicalNodeName == node.ConnectionBuilder.Database) //重新计算得出, 这个数据不需要迁移
                                continue;
                            affectedCount++;
                        }
                    }
                    while (currRowsCount < userIdMax);
                    Console.WriteLine($"节点:{node.ConnectionBuilder.Database} 数据量:{dataCount}");
                }
                Console.WriteLine("冗余数据量:" + affectedCount);
            }
        }

        private void CreateDB(string name, string machineId)
        {
            var distributedDB = new DistributedDB
            {
                BatchSize = 100000,
                SqlBatchSize = 1024 * 1024 * 5
            };
            distributedDB.ConnectionBuilder.Database = name;
            distributedDB.CreateTables("root", name);
            distributedDB.InitTablesId(5, true, int.Parse(machineId), 10);
            distributedDB.Start();
            consistentHashing.AddNode(name);
            mysqlNodes.Add(name, distributedDB);
        }
    }
}
