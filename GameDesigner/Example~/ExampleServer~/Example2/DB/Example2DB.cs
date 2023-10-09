#if SERVER
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;
using System.Data.SQLite;
using Net.Event;
using Net.System;
using Net.Share;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;

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
        public static Example2DB I { get; private set; } = new Example2DB();
        private readonly MyDictionary<Type, HashSetSafe<IDataRow>> DataRowHandler = new MyDictionary<Type, HashSetSafe<IDataRow>>();
        private readonly ConcurrentStack<SQLiteConnection> conns = new ConcurrentStack<SQLiteConnection>();
        public static string connStr = @"Data Source='D:\Demo\Assets\Samples\GameDesigner\Example\ExampleServer~\bin\Debug\Data\example2.db';";
        /// <summary>
        /// 从运行到现在的所有Sql执行次数
        /// </summary>
        public long QueryCount { get; set; }
        /// <summary>
        /// Sql批处理sql语句字符串大小, 默认是128k的字符串长度
        /// </summary>
        public int SqlBatchSize { get; set; } = ushort.MaxValue * 2;
        /// <summary>
        /// 命令超时, 默认为30秒内必须完成
        /// </summary>
        public int CommandTimeout { get; set; } = 30;
        /// <summary>
        /// 每次执行最多可批量的次数 默认是10万
        /// </summary>
        public int BatchSize { get; set; } = 100000;
        private readonly StringBuilder updateCmdText = new StringBuilder();
        private readonly StringBuilder deleteCmdText = new StringBuilder();

        private SQLiteConnection CheckConn(SQLiteConnection conn)
        {
            if (conn == null)
            {
                conn = new SQLiteConnection(connStr); //数据库连接
                conn.Open();
            }
            
            if (conn.State != ConnectionState.Open)
            {
                conn.Close();
                conn = new SQLiteConnection(connStr); //数据库连接
                conn.Open();
            }
            return conn;
        }

        public void Init(Action<List<object>> onInit, int connLen = 5)
        {
            InitConnection(connLen);
            List<object> list = new List<object>();
     // -- 1
            var configTable = ExecuteReader($"SELECT * FROM config");
            foreach (DataRow row in configTable.Rows)
            {
                var data = new ConfigData();
                data.Init(row);
                list.Add(data);
            }
            configTable.Dispose();
     // -- 1
            var userinfoTable = ExecuteReader($"SELECT * FROM userinfo");
            foreach (DataRow row in userinfoTable.Rows)
            {
                var data = new UserinfoData();
                data.Init(row);
                list.Add(data);
            }
            userinfoTable.Dispose();
     // -- 2
            onInit?.Invoke(list);
        }

        public void InitConnection(int connLen = 1) //初学者避免发生死锁, 默认只创建一条连接
        {
            while (conns.TryPop(out var conn))
                conn.Close();
            for (int i = 0; i < connLen; i++)
                conns.Push(CheckConn(null));
        }

        public SQLiteConnection PopConnect()
        {
            SQLiteConnection conn1;
            while (!conns.TryPop(out conn1))
                Thread.Sleep(1);
            return CheckConn(conn1);
        }

        public void CloseConnect()
        {
            while (conns.TryPop(out SQLiteConnection conn))
                conn.Close();
        }

        public DataTable ExecuteReader(string cmdText)
        {
            var conn = PopConnect();
            var dt = new DataTable();
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Parameters.Clear();
                    using (var sdr = cmd.ExecuteReader())
                    {
                        dt.Load(sdr);
                        QueryCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError(cmdText + " 错误: " + ex);
            }
            finally
            {
                conns.Push(conn);
            }
            return dt;
        }

        public async UniTask<DataTable> ExecuteReaderAsync(string cmdText)
        {
            await UniTask.SwitchToThreadPool();
            return ExecuteReader(cmdText);
        }

        /// <summary>
        /// 查询1: select * from Example2 where id=1;
        /// <para></para>
        /// 查询2: select * from Example2 where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Example2 where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Example2 where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Example2 where id not in(1,2,3,4,5);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public T ExecuteQuery<T>(string cmdText) where T : IDataRow, new()
        {
            var array = ExecuteQueryList<T>(cmdText);
            if (array == null)
                return default;
            if (array.Length == 0)
                return default;
            return array[0];
        }

        /// <summary>
        /// 查询1: select * from Example2 where id=1;
        /// <para></para>
        /// 查询2: select * from Example2 where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Example2 where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Example2 where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Example2 where id not in(1,2,3,4,5);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public T[] ExecuteQueryList<T>(string cmdText) where T : IDataRow, new()
        {
            var conn = PopConnect();
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Parameters.Clear();
                    using (var sdr = cmd.ExecuteReader())
                    {
                        var datas = new List<T>();
                        while (sdr.Read())
                        {
                            var data = new T();
                            for (int i = 0; i < sdr.FieldCount; i++)
                            {
                                var name = sdr.GetName(i);
                                var value = sdr.GetValue(i);
                                if (value == DBNull.Value) //空值不能进行赋值,会报错
                                    continue;
                                data[name] = value;
                            }
                            data.RowState = DataRowState.Unchanged;
                            datas.Add(data);
                        }
                        QueryCount++;
                        return datas.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError(cmdText + " 错误: " + ex);
            }
            finally
            {
                conns.Push(conn);
            }
            return default;
        }

        /// <summary>
        /// 查询1: select * from Example2 where id=1;
        /// <para></para>
        /// 查询2: select * from Example2 where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Example2 where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Example2 where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Example2 where id not in(1,2,3,4,5);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public async UniTask<T> ExecuteQueryAsync<T>(string cmdText) where T : IDataRow, new()
        {
            var array = await ExecuteQueryListAsync<T>(cmdText);
            if (array == null)
                return default;
            if (array.Length == 0)
                return default;
            return array[0];
        }

        /// <summary>
        /// 查询1: select * from Example2 where id=1;
        /// <para></para>
        /// 查询2: select * from Example2 where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Example2 where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Example2 where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Example2 where id not in(1,2,3,4,5);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public async UniTask<T[]> ExecuteQueryListAsync<T>(string cmdText) where T : IDataRow, new()
        {
            await UniTask.SwitchToThreadPool();
            var datas = ExecuteQueryList<T>(cmdText);
            return datas;
        }

        public async UniTaskVoid ExecuteNonQuery(string cmdText, List<IDbDataParameter> parameters, Action<int, Stopwatch> onComplete)
        {
            var stopwatch = Stopwatch.StartNew();
            var count = await ExecuteNonQuery(cmdText, parameters);
            stopwatch.Stop();
            onComplete(count, stopwatch);
        }

        public async UniTask<int> ExecuteNonQuery(string cmdText, List<IDbDataParameter> parameters)
        {
            await UniTask.SwitchToThreadPool();
            var conn = PopConnect();
            var pars = parameters != null ? parameters.ToArray() : new IDbDataParameter[0];
            var count = ExecuteNonQuery(conn, cmdText, pars);
            conns.Push(conn);
            return count;
        }

        public int ExecuteNonQuery(string cmdText)
        {
            var conn = PopConnect();
            var pars = new IDbDataParameter[0];
            var count = ExecuteNonQuery(conn, cmdText, pars);
            conns.Push(conn);
            return count;
        }

        private int ExecuteNonQuery(SQLiteConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = CommandTimeout;//避免死锁一直无畏的等待, 在30秒内必须完成
                    cmd.Parameters.AddRange(parameters);
                    var count = cmd.ExecuteNonQuery();
                    QueryCount += count;
                    return count;
                }
            }
            catch (Exception ex)
            {
                cmdText = GetCommandText(cmdText, parameters);
                NDebug.LogError(cmdText + " 发生错误,如果有必要,请将sql语句复制到Navicat的查询窗口执行: " + ex);
            }
            return -1;
        }

        public T ExecuteScalar<T>(string cmdText)
        {
            var conn = PopConnect();
            var pars = new IDbDataParameter[0];
            var count = ExecuteScalar<T>(conn, cmdText, pars);
            conns.Push(conn);
            return count;
        }

        private T ExecuteScalar<T>(SQLiteConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = CommandTimeout;//避免死锁一直无畏的等待, 在30秒内必须完成
                    cmd.Parameters.AddRange(parameters);
                    var count = (T)cmd.ExecuteScalar();
                    QueryCount++;
                    return count;
                }
            }
            catch (Exception ex)
            {
                cmdText = GetCommandText(cmdText, parameters);
                NDebug.LogError(cmdText + " 发生错误,如果有必要,请将sql语句复制到Navicat的查询窗口执行: " + ex);
            }
            return default;
        }

        private static string GetCommandText(string cmdText, IDbDataParameter[] parameters) 
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Value is byte[] buffer)
                {
                    var sb = new StringBuilder();
                    for (int n = 0; n < buffer.Length; n++)
                    {
                        var x = buffer[n].ToString("x").PadLeft(2, '0');
                        sb.Append(x);
                    }
                    var hex = sb.ToString();
                    cmdText = cmdText.Replace($"@buffer{i} ", $"UNHEX('{hex}') "); //必须留空格, 否则buffer1和buffer10都一样被替换, buffer10会留个0的问题
                }
            }
            return cmdText;
        }

        public void Update(IDataRow entity)//更新的行,列
        {
            var type = entity.GetType();
            if (!DataRowHandler.TryGetValue(type, out var hash))
                DataRowHandler[type] = hash = new HashSetSafe<IDataRow>();
            hash.Add(entity);
        }

        public bool Executed()//每秒调用一次, 需要自己调用此方法
        {
            try
            {
                updateCmdText.Clear();
                deleteCmdText.Clear();
                foreach (var item in DataRowHandler)
                {
                    var tableName = item.Key.Name;
                    tableName = tableName.Remove(tableName.Length - 4, 4);
                    var index = 0;
                    var count = item.Value.Count;
                    count = count > BatchSize ? BatchSize : count;
                    foreach (var row in item.Value)
                    {
                        switch (row.RowState)
                        {
                            case DataRowState.Added:
                                row.AddedSql(updateCmdText);
                                break;
                            case DataRowState.Modified:
                                row.ModifiedSql(updateCmdText);
                                break;
                            case DataRowState.Detached:
                                row.DeletedSql(deleteCmdText);
                                break;
                        }
                        item.Value.Remove(row);
                        if (updateCmdText.Length + deleteCmdText.Length >= SqlBatchSize) 
                        {
                            ExecuteNonQuery(tableName, updateCmdText, deleteCmdText);
                            updateCmdText.Clear();
                            deleteCmdText.Clear();
                        }
                        if (index++ >= count)
                            break;
                    }
                    ExecuteNonQuery(tableName, updateCmdText, deleteCmdText);
                    updateCmdText.Clear();
                    deleteCmdText.Clear();
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError("SQL异常: " + ex);
            }
            return true;
        }

        private void ExecuteNonQuery(string tableName, StringBuilder updateCmdText, StringBuilder deleteCmdText)
        {
            if (updateCmdText.Length > 0)
            {
 // -- 3
                var stopwatch = Stopwatch.StartNew();
                var rowCount = ExecuteNonQuery(updateCmdText.ToString());
                stopwatch.Stop();
                if (rowCount > 2000) NDebug.Log($"SQL批处理完成:{rowCount} 用时:{stopwatch.Elapsed}");
 // -- 4
            }
            if (deleteCmdText.Length > 0)
            {
                var stopwatch = Stopwatch.StartNew();
                var rowCount = ExecuteNonQuery(deleteCmdText.ToString());
                stopwatch.Stop();
                if (rowCount > 2000) NDebug.Log($"SQL批处理完成:{rowCount} 用时:{stopwatch.Elapsed}");
            }
        }

        public string CheckStringValue(string value, uint length)
        {
            CheckStringValue(ref value, length);
            return value;
        }

        public void CheckStringValue(ref string value, uint length)
        {
            if (value == null)
                value = string.Empty;
            value = value.Replace("\\", "\\\\"); //如果包含\必须转义, 否则出现 \'时就会出错
            value = value.Replace("'", "\\\'"); //出现'时必须转转义成\'
            value = value.Replace("|", "\\|"); //批量分隔符|
            if (value.Length >= length - 3) //必须保留三个字符做最后的判断, 如最后一个字符出现了\或'时出错问题
                value = value.Substring(0, (int)length);
        }

        /// <summary>
        /// 当项目在其他电脑上使用时可快速还原数据库信息和所有数据表
        /// </summary>
        public void CreateTables()
        {
            connStr = @"";
            InitConnection();
            int count = (int)ExecuteScalar<long>($"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = 'Example2'");
            if (count <= 0) 
            {
                count = ExecuteNonQuery(@"");
                NDebug.Log($"创建数据库:Example2{(count >= 0 ? "成功" : "失败")}!");
                if (count <= 0)
                    return;
                CloseConnect();
                connStr = @"Data Source='D:\Demo\Assets\Samples\GameDesigner\Example\ExampleServer~\bin\Debug\Data\example2.db';";
                InitConnection();
 // -- 6
            }
            else NDebug.Log($"数据库:Example2已存在!");
        }
    }
}
#endif