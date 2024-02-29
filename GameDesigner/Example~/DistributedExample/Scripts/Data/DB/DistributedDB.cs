#if SERVER
using System;
using System.IO;
using System.Data;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
using Net.Event;
using Net.System;
using Net.Share;
using Net.Helper;
using Net.Distributed;
using Cysharp.Threading.Tasks;

namespace Distributed
{
    /// <summary>
    /// DistributedDB数据库管理类
    /// 此类由MySqlDataBuild工具生成, 请不要在此类编辑代码! 请新建一个类文件进行分写
    /// <para>MySqlDataBuild工具提供Rpc自动同步到mysql数据库的功能, 提供数据库注释功能</para>
    /// MySqlDataBuild工具gitee地址:https://gitee.com/leng_yue/my-sql-data-build
    /// </summary>
    public partial class DistributedDB
    {
        public static DistributedDB I { get; private set; } = new DistributedDB();
        private readonly MyDictionary<Type, HashSetSafe<IDataRow>> DataRowHandler = new MyDictionary<Type, HashSetSafe<IDataRow>>();
        private readonly ConcurrentStack<MySqlConnection> conns = new ConcurrentStack<MySqlConnection>();
        public string ConnectionText { get => ConnectionBuilder.ToString(); set => ConnectionBuilder = new MySqlConnectionStringBuilder(value); }
        public MySqlConnectionStringBuilder ConnectionBuilder = new MySqlConnectionStringBuilder() 
        {
            Database = "distributed",
            Server = "127.0.0.1",
            Port = 3306,
            UserID = "root",
            Password = "root",
            CharacterSet = "utf8mb4",
            Pooling = true,
            UseCompression = true,
            ConnectionTimeout = 60,
            AllowLoadLocalInfile = true
        };
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
        private readonly MyDictionary<short, UniqueIdGenerator> uniqueIdMap = new MyDictionary<short, UniqueIdGenerator>();
        private readonly MyDictionary<Type, QueueSafe<QueryTask>> queryTypes = new MyDictionary<Type, QueueSafe<QueryTask>>();
        private readonly MyDictionary<string, StringBuilder> queryCell = new MyDictionary<string, StringBuilder>();
        private readonly Queue<QueryTask> queryQueue = new Queue<QueryTask>();

        private MySqlConnection CheckConn(MySqlConnection conn)
        {
            if (conn == null)
            {
                conn = new MySqlConnection(ConnectionText); //数据库连接
                conn.Open();
            }
            conn.Ping();//长时间没有连接后断开连接检查状态
            if (conn.State != ConnectionState.Open)
            {
                conn.Close();
                conn = new MySqlConnection(ConnectionText); //数据库连接
                conn.Open();
            }
            return conn;
        }

        /// <summary>
        /// 初始化, 加载数据库数据到内存
        /// </summary>
        /// <param name="onInit">当加载数据库完成调用委托</param>
        /// <param name="connLen">连接数据库管线数量</param>
        public void Init(Action<List<object>> onInit, int connLen = 5)
        {
            InitTablesId(connLen);
            List<object> list = new List<object>();
     // -- 1
            var userTable = ExecuteReader($"SELECT * FROM `user`");
            foreach (DataRow row in userTable.Rows)
            {
                var data = new UserData();
                data.Init(row);
                list.Add(data);
            }
            userTable.Dispose();
     // -- 2
            onInit?.Invoke(list);
        }

        /// <summary>
        /// 初始化数据表id
        /// </summary>
        /// <param name="connLen">连接数据库管线数量</param>
        /// <param name="useMachineId">使用分布式机器ID</param>
        /// <param name="machineId">分布式机器ID</param>
        /// <param name="machineIdBits">分布式机器占用64位中的多少位数</param>
        public void InitTablesId(int connLen = 5, bool useMachineId = false, int machineId = 0, int machineIdBits = 10)
        {
            InitConnection(connLen);
            var uniqueId = ExecuteScalar<long>(@"SELECT MAX(id) FROM `user`;");
            uniqueIdMap[DistributedUniqueIdType.User] = new UniqueIdGenerator(useMachineId, machineId, machineIdBits, uniqueId);
        }

        /// <summary>
        /// 获取表的自增ID, 每调用一次这个方法ID就会++
        /// </summary>
        /// <param name="type">传入DistributedUniqueIdType类的常量字段</param>
        /// <returns></returns>
        public long GetUniqueId(short type)
        {
            return (long)uniqueIdMap[type].NewUniqueId();
        }

        public void InitConnection(int connLen = 1) //初学者避免发生死锁, 默认只创建一条连接
        {
            while (conns.TryPop(out var conn))
                conn.Close();
            for (int i = 0; i < connLen; i++)
                conns.Push(CheckConn(null));
        }

        public MySqlConnection PopConnect()
        {
            MySqlConnection conn1;
            while (!conns.TryPop(out conn1))
                Thread.Sleep(1);
            return CheckConn(conn1);
        }

        public void CloseConnect()
        {
            while (conns.TryPop(out MySqlConnection conn))
                conn.Close();
        }

        public DataTable ExecuteReader(string cmdText)
        {
            var conn = PopConnect();
            var dt = new DataTable();
            try
            {
                using (var cmd = new MySqlCommand())
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
        /// 查询1: select * from Distributed where id=1;
        /// <para></para>
        /// 查询2: select * from Distributed where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Distributed where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Distributed where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Distributed where id not in(1,2,3,4,5);
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
        /// 查询1: select * from Distributed where id=1;
        /// <para></para>
        /// 查询2: select * from Distributed where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Distributed where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Distributed where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Distributed where id not in(1,2,3,4,5);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public T[] ExecuteQueryList<T>(string cmdText) where T : IDataRow, new()
        {
            var conn = PopConnect();
            try
            {
                using (var cmd = new MySqlCommand())
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
        /// 查询1: select * from Distributed where id=1;
        /// <para></para>
        /// 查询2: select * from Distributed where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Distributed where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Distributed where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Distributed where id not in(1,2,3,4,5);
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
        /// 查询1: select * from Distributed where id=1;
        /// <para></para>
        /// 查询2: select * from Distributed where id=1 and `index`=1;
        /// <para></para>
        /// 查询3: select * from Distributed where id=1 or `index`=1;
        /// <para></para>
        /// 查询4: select * from Distributed where id in(1,2,3,4,5);
        /// <para></para>
        /// 查询5: select * from Distributed where id not in(1,2,3,4,5);
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

        private int ExecuteNonQuery(MySqlConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand())
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

        private T ExecuteScalar<T>(MySqlConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            try
            {
                using (var cmd = new MySqlCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = CommandTimeout;//避免死锁一直无畏的等待, 在30秒内必须完成
                    cmd.Parameters.AddRange(parameters);
                    var obj = cmd.ExecuteScalar();
                    if (obj is DBNull)
                        return default;
                    if (obj is string)
                    {
                        if (typeof(T) == typeof(string))
                            return (T)obj;
                        else
                            return default;
                    }
                    var count = (T)Convert.ChangeType(obj, typeof(T));
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

        /// <summary>
        /// 设置表执行顺序, 用到mysql外键时需要设置
        /// </summary>
        /// <param name="types">生成的xxData类</param>
        public void SetExecutionOrder(params Type[] types)
        {
            foreach (var type in types)
                DataRowHandler[type] = new HashSetSafe<IDataRow>();
        }

        public void Update(IDataRow entity)//更新的行,列
        {
            var type = entity.GetType();
            if (!DataRowHandler.TryGetValue(type, out var hash))
                DataRowHandler[type] = hash = new HashSetSafe<IDataRow>();
            hash.Add(entity);
        }

        public bool BatchWorker() //每帧调用一次, 需要自己调用此方法
        {
            BatchQueryWorkers();
            BatchSubmitWorkers();
            return true;
        }

        private bool BatchSubmitWorkers()
        {
            try
            {
                foreach (var item in DataRowHandler)
                {
                    var count = item.Value.Count;
                    if (count <= 0)
                        continue;
                    count = count > BatchSize ? BatchSize : count;
                    updateCmdText.Clear();
                    deleteCmdText.Clear();
                    var tableName = item.Key.Name;
                    tableName = tableName.Remove(tableName.Length - 4, 4);
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
                        if (count-- <= 0)
                            break;
                    }
                    ExecuteNonQuery(tableName, updateCmdText, deleteCmdText);
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
                var conn = PopConnect();
                try
                {
                    var path = PathHelper.Combine(AppDomain.CurrentDomain.BaseDirectory, $"MySqlBulkLoader/{ConnectionBuilder.Database}/");
                    var fileName = path + $"{tableName}.txt";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        stream.SetLength(0);
                        var text = updateCmdText.ToString();
                        var bytes = Encoding.UTF8.GetBytes(text);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }
                    var bulkLoader = new MySqlBulkLoader(conn)
                    {
                        TableName = $"`{tableName}`",
                        FieldTerminator = "|",
                        LineTerminator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n",
                        NumberOfLinesToSkip = 0,
                        FileName = fileName,
                        EscapeCharacter = '\\',
                        Local = true,
                        CharacterSet = "utf8mb4",
                        ConflictOption = MySqlBulkLoaderConflictOption.Replace,
                    };
                    var stopwatch = Stopwatch.StartNew();
                    var rowCount = bulkLoader.Load();
                    QueryCount += rowCount;
                    stopwatch.Stop();
                    if (rowCount > 2000) NDebug.Log($"SQL批处理完成:{rowCount} 用时:{stopwatch.Elapsed}");
                }
                catch (Exception ex)
                {
                    NDebug.LogError("批量错误: 1.如果表字段更改则需要重新生成! 2.打开Navicat菜单工具->命令列界面,输入SHOW VARIABLES LIKE 'local_infile';后回车,看是否开启批量加载!如果没有则输入SET GLOBAL local_infile = ON;回车设置批量加载! 详细信息:" + ex);
                }
                finally
                {
                    conns.Push(conn);
                }
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

        private class QueryTask
        {
            internal string CommandText;
            internal bool IsDone;
            internal IDataRow Data;
        }

        /// <summary>
        /// 异步查询, 查询案例: `id`=1 或者 `name` = 'hello'
        /// <para></para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public async UniTask<T> QueryAsync<T>(string filterExpression) where T : IDataRow
        {
            var type = typeof(T);
            if (!queryTypes.TryGetValue(type, out var queue))
                queryTypes[type] = queue = new QueueSafe<QueryTask>();
            var queryTask = new QueryTask() { CommandText = filterExpression };
            queue.Enqueue(queryTask);
            await UniTaskNetExtensions.Wait(CommandTimeout * 1000, (state) => ((QueryTask)state).IsDone, queryTask);
            return (T)queryTask.Data;
        }

        private bool BatchQueryWorkers()
        {
            try
            {
                foreach (var item in queryTypes)
                {
                    var count = item.Value.Count;
                    if (count <= 0)
                        continue;
                    count = count > BatchSize ? BatchSize : count;
                    queryCell.Clear();
                    queryQueue.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        if (item.Value.TryDequeue(out var queryTask))
                        {
                            var commandTexts = queryTask.CommandText.Split('=');
                            if (commandTexts.Length == 2)
                            {
                                var cellName = commandTexts[0].Trim();
                                if (!queryCell.TryGetValue(cellName, out var queryCommandText))
                                    queryCell.Add(cellName, queryCommandText = new StringBuilder($"SELECT * FROM `user` WHERE `{cellName}` IN("));
                                queryCommandText.Append($"{commandTexts[1]},");
                            }
                            else throw new NotImplementedException("还未实现多查询, 请联系作者增加功能!");
                            queryQueue.Enqueue(queryTask);
                        }
                    }
                    foreach (var item1 in queryCell)
                    {
                        var queryCommandText = item1.Value;
                        queryCommandText[queryCommandText.Length - 1] = ' ';
                        queryCommandText.Append(");");
                        var commandText = queryCommandText.ToString();
                        var dataTable = ExecuteQueryAsDataTable(commandText);
                        while (queryQueue.Count > 0)
                        {
                            var queryTask = queryQueue.Dequeue();
                            var rows = dataTable.Select(queryTask.CommandText);
                            if (rows.Length > 0)
                            {
                                queryTask.Data = (IDataRow)Activator.CreateInstance(item.Key);
                                queryTask.Data.Init(rows[0]);
                            }
                            queryTask.IsDone = true;
                        }
                    }
                }
            } 
            catch (Exception ex)
            {
                NDebug.LogError("批量查询异常: " + ex);
            }
            return true;
        }

        public DataTable ExecuteQueryAsDataTable(string cmdText)
        {
            var conn = PopConnect();
            try
            {
                using (var cmd = new MySqlCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Parameters.Clear();
                    var dataTable = new DataTable();
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                    conns.Push(conn);
                    return dataTable;
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
        public void CreateTables(string password, string dbName = "Distributed")
        {
            ConnectionBuilder.Database = "";
            ConnectionBuilder.Password = password;
            InitConnection();
            int count = (int)ExecuteScalar<long>($"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '{dbName}'");
            if (count <= 0) 
            {
                count = ExecuteNonQuery($@"CREATE DATABASE `{dbName}` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */");
                NDebug.Log($"创建数据库:{dbName}{(count >= 0 ? "成功" : "失败")}!");
                if (count <= 0)
                    return;
            }
            CloseConnect();
            ConnectionBuilder.Database = dbName;
            InitConnection();
            //新增表时判断
 // -- 5
            count = (int)ExecuteScalar<long>($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{dbName}' AND table_name = 'user'");
            if (count <= 0)
            {
                count = ExecuteNonQuery(@"CREATE TABLE `user` (
  `id` bigint NOT NULL,
  `account` varchar(255) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `level` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `account` (`account`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci");
                NDebug.Log($"创建数据表:user{(count >= 0 ? "成功" : "失败")}!");
            }
            else
            {
                count = ExecuteScalar<int>($@"SELECT COUNT(*) AS column_count FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{dbName}' AND TABLE_NAME = 'user';");
                if (count != 5) NDebug.LogWarning("user表有改动,请进行处理! 1.如果你新增了列或删除列, 请用MySqlBuild工具重新生成! 2.如果是多人协作请根据提交信息新增或删除列!");
            }
 // -- 6
        }
    }
}
#endif