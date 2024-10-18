using System.Linq;
using System.Collections.Generic;
using Net.Helper;

namespace Net.Distributed
{
    /// <summary>
    /// 一致性哈希虚拟节点
    /// </summary>
    public class VirtualNode<T>
    {
        /// <summary>
        /// 虚拟节点名称
        /// </summary>
        public string VirtualNodeName { get; set; }
        /// <summary>
        /// 物理节点名称, 即数据库服务器的真实节点
        /// </summary>
        public string PhysicalNodeName { get; set; }
        /// <summary>
        /// 哈希开始范围
        /// </summary>
        public uint StartHash { get; set; }
        /// <summary>
        /// 哈希结束范围
        /// </summary>
        public uint EndHash { get; set; }
        /// <summary>
        /// 对象参数
        /// </summary>
        public T Token { get; set; }

        /// <summary>
        /// 虚拟节点
        /// </summary>
        /// <param name="virtualNode"></param>
        /// <param name="physicalNodeName"></param>
        public VirtualNode(string virtualNode, string physicalNodeName)
        {
            VirtualNodeName = virtualNode;
            PhysicalNodeName = physicalNodeName;
        }

        /// <summary>
        /// 克隆对象，防止字段被修改
        /// </summary>
        /// <returns></returns>
        public VirtualNode<T> Clone()
        {
            var node = new VirtualNode<T>(VirtualNodeName, PhysicalNodeName)
            {
                StartHash = StartHash,
                EndHash = EndHash,
                Token = Token
            };
            return node;
        }

        public override string ToString()
        {
            return $"虚拟节点:{VirtualNodeName} 物理节点:{PhysicalNodeName} 哈希范围:({StartHash}, {EndHash})";
        }
    }

    /// <summary>
    /// 一致性哈希, 节点的顺序是0-uint.maxValue, 当只有一个节点时哈希值从0-uint.maxValue, 当有两个节点时,节点1的哈希计算是从节点1的hash值开始到节点2的hash值结束, 
    /// 节点2计算从节点2的hash值开始到尾部(uint.maxValue)和节点1的开始hash值为止, 以此类推
    /// |__________node_a____________->__________________node_b_______________->_________________|uint.maxValue|_________->________node_a____|
    /// </summary>
    public class ConsistentHashing<T>
    {
        private readonly SortedDictionary<uint, VirtualNode<T>> hashRing;  // 哈希环，按照哈希值排序
        private readonly HashSet<string> nodes;  // 节点集合
        private readonly int virtualNodeReplicas;  // 每个节点的虚拟节点数量

        /// <summary>
        /// 一致性哈希构造
        /// </summary>
        /// <param name="virtualNodeReplicas">虚拟节点因子</param>
        public ConsistentHashing(int virtualNodeReplicas = 5)
        {
            hashRing = new SortedDictionary<uint, VirtualNode<T>>();
            nodes = new HashSet<string>();
            this.virtualNodeReplicas = virtualNodeReplicas;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="token"></param>
        public void AddNode(string node, T token = default)
        {
            AddNodeGet(node, token);
        }

        /// <summary>
        /// 添加节点并获得虚拟节点列表
        /// </summary>
        /// <param name="node"></param>
        /// <param name="token"></param>
        public List<VirtualNode<T>> AddNodeGet(string node, T token = default)
        {
            var virtualNodes = new List<VirtualNode<T>>();
            if (nodes.Add(node))
            {
                // 根据节点名称和索引构造虚拟节点，并添加到哈希环中
                for (int i = 0; i < virtualNodeReplicas; i++) // 假设每个节点有5个虚拟节点
                {
                    var virtualNode = $"{node}_V{i}";
                    var hash = GetHash(virtualNode);
                    var Node = new VirtualNode<T>(virtualNode, node) { Token = token };
                    hashRing[hash] = Node;
                    virtualNodes.Add(Node);
                }
                RecalculateNode();
            }
            return virtualNodes;
        }

        /// <summary>
        /// 移除节点
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(string node)
        {
            if (nodes.Remove(node))
            {
                // 移除对应的虚拟节点
                for (int i = 0; i < virtualNodeReplicas; i++)
                {
                    var virtualNode = $"{node}_V{i}";
                    var hash = GetHash(virtualNode);
                    hashRing.Remove(hash);
                }
                RecalculateNode();
            }
        }

        /// <summary>
        /// 移除节点并且获取移除的虚拟节点列表
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<VirtualNode<T>> RemoveNodeGet(string node)
        {
            var virtualNodes = new List<VirtualNode<T>>();
            if (nodes.Remove(node))
            {
                // 移除对应的虚拟节点
                for (int i = 0; i < virtualNodeReplicas; i++)
                {
                    var virtualNode = $"{node}_V{i}";
                    var hash = GetHash(virtualNode);
                    virtualNodes.Add(hashRing[hash]);
                    hashRing.Remove(hash);
                }
                RecalculateNode();
            }
            return virtualNodes;
        }

        /// <summary>
        /// 根据键获取对应的数据库节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public VirtualNode<T> GetNode(string key)
        {
            if (nodes.Count == 0)
                return null;
            var hashValue = GetHash(key);
            foreach (var virtualNode in hashRing.Values)
            {
                if (hashValue >= virtualNode.StartHash && hashValue <= virtualNode.EndHash)
                {
                    return virtualNode;
                }
            }
            // 如果循环结束仍未找到节点，则返回环形空间上的最后一个节点
            return hashRing.Last().Value;
        }

        /// <summary>
        /// 获取受影响的节点列表
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public List<VirtualNode<T>> GetAffectedNodes(string nodeName)
        {
            var affectedNodes = new HashSet<VirtualNode<T>>();
            if (nodes.Count == 0)
                return affectedNodes.ToList();
            for (int i = 0; i < virtualNodeReplicas; i++) // 假设每个节点有5个虚拟节点
            {
                var virtualNode = $"{nodeName}_V{i}";
                var newHash = GetHash(virtualNode);
                var keys = hashRing.Keys.ToArray();
                foreach (var hash in keys)
                {
                    if (newHash > hash)
                        continue;
                    //之前是谁管的
                    for (int x = 0; x < keys.Length; x++)
                    {
                        if (newHash > keys[x])
                            continue;
                        if (x == 0) //环形过来, 最后一个会环绕到前面的位置 (尾首相连)
                            goto J;
                        //如果在中间
                        affectedNodes.Add(hashRing[keys[x - 1]]);
                        goto J1;
                    }
                    break;
                }
            J:
                //如果大于所有元素或者小于所有元素, 都是在尾部添加
                affectedNodes.Add(hashRing.Last().Value);
            J1:;
            }
            return affectedNodes.ToList();
        }

        // 哈希函数
        private uint GetHash(string node)
        {
            return node.CRCU32();
        }

        /// <summary>
        /// 计算节点的范围
        /// </summary>
        public void RecalculateNode()
        {
            if (hashRing.Count == 0)
                return;
            foreach (var item in hashRing)
            {
                foreach (var nodeHash in hashRing.Keys)
                {
                    if (item.Key < nodeHash)
                    {
                        item.Value.StartHash = item.Key;
                        item.Value.EndHash = nodeHash;
                        break;
                    }
                }
            }
            var last = hashRing.Last();
            last.Value.StartHash = last.Key;
            last.Value.EndHash = uint.MaxValue;
        }
    }
}