using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Example2
{
    /// <summary>
    /// Example2DB数据库管理类
    /// 此类由MySqlDataBuild工具生成, 请不要在此类编辑代码! 请新建一个类文件进行分写
    /// <para>MySqlDataBuild工具提供Rpc自动同步到mysql数据库的功能, 提供数据库注释功能</para>
    /// MySqlDataBuild工具gitee地址:https://gitee.com/leng_yue/my-sql-data-build
    /// </summary>
    public partial class Example2DB
    {
        public ConcurrentDictionary<string, UserinfoData> UserinfoDatas = new ConcurrentDictionary<string, UserinfoData>();
        public ConcurrentDictionary<int, ConfigData> Configs = new ConcurrentDictionary<int, ConfigData>();

        public void OnInit(List<object> data)
        {
            foreach (var item in data)
            {
                if (item is UserinfoData data1)
                {
                    UserinfoDatas.TryAdd(data1.Account, data1);
                }
                if (item is ConfigData data2)
                {
                    Configs.TryAdd((int)data2.Id, data2);
                }
            }
        }
    }
}