using System;
using System.Collections.Generic;
using System.Linq;

namespace Net.Distributed
{
    class DataTest
    {
        public string key;
    }

    class MySqlDataBase
    {
        private HashSet<DataTest> datas = new HashSet<DataTest>();
        public string Name { get; internal set; }
        public int Count => datas.Count;

        internal void AddData(DataTest dataTest)
        {
            datas.Add(dataTest);
        }

        internal void Remove(DataTest data)
        {
            datas.Remove(data);
        }

        internal List<DataTest> GetDataAll()
        {
            return new List<DataTest>(datas);
        }
    }

    public class ConsistentHashingTest
    {
        public static void Test()
        {
            var consistentHashing = new ConsistentHashing<object>(10);
            //模拟mysql数据库节点
            var mysqlNodes = new Dictionary<string, MySqlDataBase>();
            int affectedCount = 0;
            string nodeName = "";
            int state = 0;
            while (true)
            {
                Console.WriteLine("1.输入:n NewNode新增1个数据库节点");
                Console.WriteLine("2.输入:d 10000新增1万个数据");
                Console.WriteLine("3.输入:n- NodeName移除1个数据库节点");
                var command = Console.ReadLine();
                if (command.StartsWith("d"))
                {
                    var countText = command.Remove(0, 2);
                    var count = int.Parse(countText);
                    var random = new Random(977579129);
                    for (int i = 0; i < count; i++)
                    {
                        var data = new DataTest();
                        data.key = Guid.NewGuid().ToString() + random.Next(int.MinValue, int.MaxValue).ToString();
                        var virtualNode = consistentHashing.GetNode(data.key);
                        mysqlNodes[virtualNode.PhysicalNodeName].AddData(data);
                    }
                    affectedCount = 0;
                    foreach (var node in mysqlNodes.Values)
                    {
                        Console.WriteLine($"节点:{node.Name} 数据量:{node.Count}");
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
                    nodeName = command.Remove(0, 2);
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
                    consistentHashing.AddNode(nodeName);
                    mysqlNodes.Add(nodeName, new MySqlDataBase() { Name = nodeName });
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
                    var nodeDatas = mysqlNodes[node].GetDataAll();
                    for (int i = nodeDatas.Count - 1; i >= 0; i--)
                    {
                        var data = nodeDatas[i];
                        var virtualNode = consistentHashing.GetNode(data.key); //重新计算要迁移到哪个节点
                        if (virtualNode.PhysicalNodeName == node) //重新计算得出, 这个数据不需要迁移
                            continue;
                        mysqlNodes[node].Remove(data); //从旧节点移除这个数据
                        mysqlNodes[virtualNode.PhysicalNodeName].AddData(data); //迁移数据到新的节点去
                        affectedCount++;
                    }
                }
                if (state == 2)
                    mysqlNodes.Remove(nodeName);
                Console.WriteLine("迁移数据量:" + affectedCount);
                affectedCount = 0;
                foreach (var node in mysqlNodes.Values)
                {
                    //检查数据是否冗余
                    var nodeDatas = node.GetDataAll();
                    foreach (var data in nodeDatas)
                    {
                        var virtualNode = consistentHashing.GetNode(data.key);
                        if (virtualNode.PhysicalNodeName == node.Name)
                            continue;
                        affectedCount++;
                    }
                    Console.WriteLine($"节点:{node.Name} 数据量:{nodeDatas.Count}");
                }
                Console.WriteLine("冗余数据量:" + affectedCount);
            }
        }
    }
}
