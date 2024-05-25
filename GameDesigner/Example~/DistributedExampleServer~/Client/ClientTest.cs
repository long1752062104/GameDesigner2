using Cysharp.Threading.Tasks;
using Net.Distributed;
using System.Diagnostics;
using Net.Client;

namespace DistributedExample
{
    public class ClientTest
    {
        public async void Init()
        {
            Console.Title = "Client";
            var loadBalance = new LoadBalance<TcpClient>();
            var config = new Net.Config.ClientConfig()
            {
                SendBufferSize = 1024 * 1024 * 20,
                ReceiveBufferSize = 1024 * 1024 * 20,
                LimitQueueCount = 1024 * 1024 * 5,
                ReconnectCount = int.MaxValue,
            };
            loadBalance.Config = config;
            var lbConfig = await loadBalance.RemoteConfig<LoadBalanceConfig>("127.0.0.1", 10240, config, (int)ProtoType.LoadBalanceConfig, "GatewayService");
            loadBalance.LBConfig = lbConfig;
            await loadBalance.Init();
            while (true)
            {
                Console.WriteLine("1000w账号测试");
                Console.WriteLine("1.注册账号:reg");
                Console.WriteLine("2.登录账号:log");
                var command = Console.ReadLine();
                ProtoType protoType = 0;
                if (command.StartsWith("reg"))
                    protoType = ProtoType.Register;
                if (command.StartsWith("log"))
                    protoType = ProtoType.Login;
                if (protoType != 0)
                {
                    var stopwatch = Stopwatch.StartNew();
                    int result = 0;
                    int unResult = 0;
                    int start = 0;
                    int count = 10000000;
                    while (start < count)
                    {
                        var stopwatch1 = Stopwatch.StartNew();
                        int len;
                        if (count - start >= 100000)
                            len = 100000;
                        else
                            len = count - start;
                        var tasks = new UniTask<int>[len];
                        for (int i = start; i < start + len; i++)
                        {
                            var client = loadBalance.GetRoundRobin().Token;
                            tasks[i - start] = client.Request<int>((uint)protoType, 1000 * 30, i.ToString(), i.ToString());
                        }
                        var results = await UniTask.WhenAll(tasks);
                        int result1 = 0;
                        int unResult1 = 0;
                        foreach (var item in results)
                        {
                            if (item == 1)
                            {
                                result++;
                                result1++;
                            }
                            else
                            {
                                unResult++;
                                unResult1++;
                            }
                        }
                        stopwatch1.Stop();
                        Console.WriteLine($"{start}-{start + len} 完成数量:{result1} 未完成:{unResult1} {stopwatch1.Elapsed}");
                        start += len;
                    }
                    stopwatch.Stop();
                    Console.WriteLine(stopwatch.Elapsed);
                    Console.WriteLine($"总体完成数量:{result} 未完成:{unResult}");
                }
            }
        }
    }
}
