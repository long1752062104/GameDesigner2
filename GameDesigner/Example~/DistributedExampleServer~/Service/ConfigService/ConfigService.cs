using Net.Distributed;
using Net.Helper;
using Net.Server;
using Net.Share;
using System.Net;

namespace DistributedExample
{
    public class ConfigService : TcpServer
    {
        private Dictionary<string, LoadBalanceConfig> Configs;
        private int dbServerPort = 9544;

        public void Init()
        {
            Configs = PersistHelper.Deserialize<Dictionary<string, LoadBalanceConfig>>("lbConfig.json");
            Start(10240);
            Console.Title = $"ConfigService 10240";
        }

        protected override bool OnUnClientRequest(NetPlayer unClient, RPCModel model)
        {
            switch ((ProtoType)model.protocol)
            {
                case ProtoType.RegisterConfig:
                    RegisterConfig(unClient, model.AsString, model.AsString);
                    break;
                case ProtoType.LoadBalanceConfig:
                    LoadBalanceConfig(unClient, model.AsString);
                    break;
            }
            return false;
        }

        private void RegisterConfig(NetPlayer client, string serverType, string serverName)
        {
            lock (this)
            {
                if (!Configs.TryGetValue(serverType, out var lbConfig))
                    Configs[serverType] = lbConfig = new LoadBalanceConfig();
                if (!lbConfig.TryGetConfig(serverName, out var config))
                {
                    config = new ItemConfig()
                    {
                        Name = serverName,
                        Host = ((IPEndPoint)client.RemotePoint).Address.ToString(),
                        Port = dbServerPort,
                    };
                    lbConfig.Add(config);
                    dbServerPort++;
                    PersistHelper.Serialize(Configs, "lbConfig.json");
                }
                Response(client, (int)ProtoType.RegisterConfig, client.Token, config);
            }
        }

        private void LoadBalanceConfig(NetPlayer client, string serverType)
        {
            lock (this)
            {
                if (!Configs.TryGetValue(serverType, out var lbConfig))
                    Configs[serverType] = lbConfig = new LoadBalanceConfig();
                Response(client, (int)ProtoType.LoadBalanceConfig, client.Token, lbConfig);
            }
        }
    }
}
