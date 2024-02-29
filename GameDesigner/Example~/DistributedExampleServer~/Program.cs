using Binding;
using Cysharp.Threading.Tasks;
using Distributed;
using Net.Client;
using Net.Distributed;
using Net.Event;
using System.Diagnostics;

namespace DistributedExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NDebug.BindConsoleLog(false);
            Fast2BuildMethod.DynamicBuild(SerializeMode.Compress, 1, typeof(ItemConfig), typeof(LoadBalanceConfig), typeof(UserData));
            if (args.Length == 0)
            {
                Start();
                goto J;
            }
            StartService(args);
        J: while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private static async void Start()
        {
            Console.WriteLine("选择服务器模式:");
            Console.WriteLine("1.单进程");
            Console.WriteLine("2.多进程");
            var command = Console.ReadLine();
            //先启动配置服务器
            StartNewProcess("ConfigService", command == "2");
            await Task.Delay(1000);
            //启动数据库服务器, 启动两个节点数据库服务器 参数1是服务器类型, 用于注册到配置服务器, 参数2是DB服名称(唯一), 参数3是机器号(唯一)
            StartNewProcess("DBService DBGame01 100", command == "2");
            StartNewProcess("DBService DBGame02 200", command == "2");
            //启动登录服务器
            StartNewProcess("LoginService LoginService01", command == "2");
            StartNewProcess("LoginService LoginService02", command == "2");
            //启动网关服务器
            StartNewProcess("GatewayService GatewayService01", command == "2");
            StartNewProcess("GatewayService GatewayService02", command == "2");
            //启动客户端
            StartNewProcess("Client", command == "2");
        }

        private static void StartNewProcess(string args, bool multiProcess)
        {
            if (multiProcess)
            {
                var appPath = Environment.ProcessPath;
                var startInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = args,
                    CreateNoWindow = false,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            else
            {
                StartService(args.Split(' '));
            }
        }

        static void StartService(string[] args)
        {
            switch (args[0])
            {
                case "ConfigService":
                    var configService = new ConfigService
                    {
                        MaxThread = 1
                    };
                    configService.AddAdapter(new Net.Adapter.SerializeAdapter3());
                    configService.Init();
                    break;
                case "DBService":
                    var dBService = new DBService();
                    dBService.AddAdapter(new Net.Adapter.SerializeAdapter3());
                    dBService.Init(args[1], args[2]);
                    break;
                case "LoginService":
                    var loginService = new LoginService();
                    loginService.AddAdapter(new Net.Adapter.SerializeAdapter3());
                    loginService.Init(args[1]);
                    break;
                case "GatewayService":
                    var gatewayService = new GatewayService();
                    gatewayService.AddAdapter(new Net.Adapter.SerializeAdapter3());
                    gatewayService.Init(args[1]);
                    break;
                case "Client":
                    var clientTest = new ClientTest();
                    clientTest.Init();
                    break;
            }
        }
    }
}
