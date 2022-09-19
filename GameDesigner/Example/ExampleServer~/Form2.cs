using Example2;
using Net.Share;
using Net.System;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExampleServer
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }

        private Service server;
        private bool run;

        private void button1_Click(object sender, EventArgs e)
        {
            if (run) 
            {
                button1.Text = "启动";
                server?.Close();
                run = false;
                return;
            }
            int port = int.Parse(textBox2.Text);//设置端口
            server = new Service();//创建服务器对象
            server.Log += str=> {//监听log
                if (listBox1.Items.Count > 2000)
                    listBox1.Items.Clear();
                listBox1.Items.Add(str);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            };
            server.OnlineLimit = 24000;//服务器最大运行2500人连接
            server.LineUp = 24000;
            server.MaxThread = 10; //增加并发线程
            server.RTO = 50;
            server.MTU = 1300;
            server.MTPS = 2048;
            server.SetHeartTime(10,200);
            server.OnNetworkDataTraffic += (df) => {//当统计网络性能,数据传输量
                toolStripStatusLabel1.Text = $"流出:{df.sendNumber}次/{ByteHelper.ToString(df.sendCount)} " +
                $"流入:{df.receiveNumber}次/{ByteHelper.ToString(df.receiveCount)} " +
                $"发送fps:{df.sendLoopNum} 接收fps:{df.revdLoopNum} 解析:{df.resolveNumber}次 " +
                $"总流入:{ByteHelper.ToString(df.inflowTotal)} 总流出:{ByteHelper.ToString(df.outflowTotal)}";
                label2.Text = "登录:" + server.OnlinePlayers + " 未登录:" + server.UnClientNumber;
            };
            server.AddAdapter(new Net.Adapter.SerializeAdapter3());
            server.AddAdapter(new Net.Adapter.CallSiteRpcAdapter<Player>(server));
            server.Run((ushort)port);//启动
            run = true;
            button1.Text = "关闭";
            Example2DB.connStr = $"Data Source='{AppDomain.CurrentDomain.BaseDirectory}/Data/example2.db';";
            Example2DB.I.Init(Example2DB.I.OnInit, 1);
            ThreadManager.Invoke(1f, Example2DB.I.Executed, true);//每秒检查有没有数据需要往mysql数据库更新
            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(100);
            //        byte num = 0;
            //        foreach (var user in Example2DB.I.UserinfoDatas.Values)
            //        {
            //            //user.Position = RandomHelper.Range(10000, 9999999).ToString();
            //            //user.Health = RandomHelper.Range(10000, 9999999);
            //            //user.MoveSpeed = RandomHelper.Range(10000, 9999999);
            //            //user.Rotation = RandomHelper.Range(10000, 9999999).ToString();
            //            //if (num++ >= 5)
            //            //    break;
            //            //user.BufferBytes = new byte[] { 1,2,3,4,5,6,7, num++, 9 };
            //        }
            //    }
            //});
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            server?.Close();
            Process.GetCurrentProcess().Kill();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            var item = listBox1.SelectedItem;
            if (item == null)
                return;
            MessageBox.Show(item.ToString());
        }
    }
}
