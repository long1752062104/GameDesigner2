using LockStep.Server;
using Net.Event;
using Net.Share;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ExampleServer
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form3_Load(object sender, EventArgs e)
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
                NDebug.RemoveFormLog();
                return;
            }
            NDebug.BindDebug(new FormDebug(listBox1));
            int port = int.Parse(textBox2.Text);//设置端口
            server = new Service();//创建服务器对象
            server.OnlineLimit = 24000;//服务器最大运行2500人连接
            server.LineUp = 24000;
            server.MaxThread = 10; //增加并发线程
            server.RTO = 50;
            server.MTU = 1300;
            server.MTPS = 2048;
            server.SetHeartTime(5,200);
            server.OnNetworkDataTraffic += (df) => {//当统计网络性能,数据传输量
                toolStripStatusLabel1.Text = $"流出:{df.sendNumber}次/{ByteHelper.ToString(df.sendCount)} " +
                $"流入:{df.receiveNumber}次/{ByteHelper.ToString(df.receiveCount)} " +
                $"FPS:{df.FPS} 解析:{df.resolveNumber}次 " +
                $"总流入:{ByteHelper.ToString(df.inflowTotal)} 总流出:{ByteHelper.ToString(df.outflowTotal)} " +
                $"登录:{server.OnlinePlayers} 未登录:{server.UnClientNumber}";
            };
            server.AddAdapter(new Net.Adapter.SerializeAdapter2());
            server.AddAdapter(new Net.Adapter.CallSiteRpcAdapter<Player>(server));
            server.Run((ushort)port);//启动
            run = true;
            button1.Text = "关闭";
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
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

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                dataGridView1.Columns.Add("PlayerID", "玩家标识");
                dataGridView1.Columns.Add("Name", "玩家名称");
                dataGridView1.Columns.Add("RemotePoint", "玩家IP");
                dataGridView1.Columns.Add("SceneName", "当前场景");
                dataGridView1.Columns.Add("UserID", "玩家UID");
                dataGridView1.Columns.Add("Login", "是否登录");
                dataGridView1.Columns.Add("isDispose", "是否释放");
                dataGridView1.Columns.Add("Connected", "是否连接");
                dataGridView1.Columns.Add("Redundant", "冗余连接");
                dataGridView1.Columns.Add("QueueUpNo", "玩家排队");
                dataGridView1.Columns.Add("ConnectTime", "连接时间");
                foreach (var client in server.AllClients.Values)
                {
                    dataGridView1.Rows.Add(client.PlayerID, client.Name, client.RemotePoint.ToString(),
                        client.SceneName, client.UserID.ToString(), client.Login.ToString(), client.isDispose.ToString(),
                        client.Connected.ToString(), client.Redundant.ToString(),
                        client.QueueUpNo.ToString(), client.ConnectTime.ToString("f"));
                }
            }
        }
    }
}
