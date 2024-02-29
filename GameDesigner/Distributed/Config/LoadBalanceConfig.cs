using System.Collections.Generic;

namespace Net.Distributed
{
    /// <summary>
    /// 负载均衡配置项
    /// </summary>
    public class ItemConfig
    {
        /// <summary>
        /// 服务器节点名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 服务器主机IP
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; }

        public override string ToString()
        {
            return $"{Name} {Host} {Port}";
        }
    }

    /// <summary>
    /// 负载均衡配置
    /// </summary>
    public class LoadBalanceConfig
    {
        /// <summary>
        /// 服务器节点配置项
        /// </summary>
        public List<ItemConfig> Items { get; set; } = new List<ItemConfig>();
        /// <summary>
        /// 虚拟节点数量, 也是真实节点的客户端连接数量
        /// </summary>
        public int Count { get; set; } = 5;

        public LoadBalanceConfig() { }

        public LoadBalanceConfig(IList<ItemConfig> items)
        {
            Items.AddRange(items);
        }

        public void Add(ItemConfig item)
        {
            Items.Add(item);
        }

        public void Remove(ItemConfig item)
        {
            Items.Remove(item);
        }

        public bool TryGetConfig(string name, out ItemConfig config)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Name == name)
                {
                    config = Items[i];
                    return true;
                }
            }
            config = null;
            return false;
        }
    }
}