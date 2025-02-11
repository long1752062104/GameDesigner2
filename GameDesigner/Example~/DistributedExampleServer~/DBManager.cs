﻿using Cysharp.Threading.Tasks;
using Distributed;
using Net.Client;
using Net.Distributed;
using System.Data;

namespace DistributedExample
{
    public class DBManager
    {
        private ConsistentHashing<TcpClient> consistentHashing;
        private Dictionary<string, DistributedDB> mysqlNodes;

        public async void Init()
        {
            consistentHashing = new ConsistentHashing<TcpClient>();
            mysqlNodes = new Dictionary<string, DistributedDB>();
            int affectedCount = 0;
            string nodeName = "";
            int state = 0;
            Console.WriteLine("1.输入:a dbName 01新增1个数据库节点,机器号为01");
            Console.WriteLine("2.输入:add 10000新增1万个数据");
            Console.WriteLine("3.输入:r dbName移除1个数据库节点");
            while (true)
            {
                var command = Console.ReadLine();
                if (command.StartsWith("add"))
                {
                    var countText = command.Remove(0, 4);
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
                        node.WaitBatchWorker(); //把上面的添加全部执行完成后才能往下执行
                        var dataCount = node.ExecuteScalar<long>($"SELECT COUNT(*) FROM `user`;");
                        Console.WriteLine($"节点:{node.ConnectionBuilder.Database} 数据量:{dataCount}");
                    }
                    continue;
                }
                if (command.StartsWith("r"))
                {
                    nodeName = command.Remove(0, 2);
                    if (!mysqlNodes.ContainsKey(nodeName))
                    {
                        Console.WriteLine("要移除的节点不存在!");
                        continue;
                    }
                    state = 2;
                }
                else if (command.StartsWith("a"))
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
                    affectedNodes = consistentHashing.RemoveNodeGet(nodeName);
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
                        long size = 1000000; //正常情况下100万数据加载是没有问题的
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
                    mysqlNodes[nodeName].ExecuteNonQuery($"DROP DATABASE {nodeName};");
                    mysqlNodes.Remove(nodeName);
                }
                Console.WriteLine("迁移数据量:" + affectedCount);
                affectedCount = 0;
                await Task.Delay(1000);
                long totalDataCount = 0L;
                foreach (var node in mysqlNodes.Values)
                {
                    //检查数据是否冗余
                    var gameDB = node;
                    gameDB.WaitBatchWorker(); //执行所有批处理才能执行下面代码
                    var userIdMax = gameDB.ExecuteScalar<long>(@"SELECT MAX(id) FROM `user`;");
                    long currRowsCount = 0;
                    long dataCount = 0;
                    do
                    {
                        long size = 1000000; //正常情况下100万数据加载是没有问题的
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
                    totalDataCount += dataCount;
                }
                Console.WriteLine("冗余数据量:" + affectedCount);
                Console.WriteLine("总数据量:" + totalDataCount);
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
