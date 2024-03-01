using Binding;
using Distributed;
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
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("运行此分布式案例前需要安装Mysql (推荐8.0.28版本，群文件有)\r\n" +
                "如果mysql的连接用户是root并且密码是root则直接运行！\r\n" +
                "如果不是，你需要修改DBService.cs文件第37行的root密码后再运行！\r\n");
            Console.ForegroundColor = color;
            Console.WriteLine("选择服务器模式:");
            Console.WriteLine("1.单进程");
            Console.WriteLine("2.多进程");
            Console.WriteLine("3.模拟服务器崩溃重启，你先使用2启动多个进程，然后再关闭几个服务器，再使用以下命令确定单个服务器");
            Console.WriteLine("  输入:ConfigService重启配置服务器");
            Console.WriteLine("  输入:DBService DBGame01 100重启1号DB服务器");
            Console.WriteLine("  输入:DBService DBGame02 200重启2号DB服务器");
            Console.WriteLine("  输入:LoginService LoginService01重启1号登录服务器");
            Console.WriteLine("  输入:LoginService LoginService02重启2号登录服务器");
            Console.WriteLine("  输入:GatewayService GatewayService01重启1号网关服务器");
            Console.WriteLine("  输入:GatewayService GatewayService02重启2号网关服务器");
            while (true) 
            {
                var command = Console.ReadLine();
                if (command.StartsWith("1") | command.StartsWith("2"))
                {
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
                else if (command.StartsWith("ConfigService") | command.StartsWith("DBService") | command.StartsWith("LoginService") | command.StartsWith("GatewayService"))
                {
                    StartNewProcess(command, true);
                }
            }
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
                    var configService = new ConfigService();
                    configService.MaxThread = 1;
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
