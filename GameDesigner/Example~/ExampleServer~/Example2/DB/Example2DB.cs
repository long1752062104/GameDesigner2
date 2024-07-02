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
using System.Data.SQLite;
using Net.Event;
using Net.System;
using Net.Share;
using Net.Helper;
using Net.Distributed;
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
        internal class DataEntityQueue
        {
            public HashSetSafe<IDataEntity> hashSet = new HashSetSafe<IDataEntity>();
            public string primaryKey;
            public int Count => hashSet.Count;

            public DataEntityQueue(string primaryKey)
            {
                this.primaryKey = primaryKey;
            }

            public void Add(IDataEntity item)
            {
                hashSet.Add(item);
            }

            public void Remove(IDataEntity item)
            {
                hashSet.Remove(item);
            }
        }

        public static Example2DB I { get; private set; } = new Example2DB();
        private readonly MyDictionary<Type, DataEntityQueue> DataRowHandler = new MyDictionary<Type, DataEntityQueue>();
        private readonly ConcurrentStack<SQLiteConnection> conns = new ConcurrentStack<SQLiteConnection>();
        public string ConnectionText { get => ConnectionBuilder.ToString(); set => ConnectionBuilder = new SQLiteConnectionStringBuilder(value); }
        public SQLiteConnectionStringBuilder ConnectionBuilder = new SQLiteConnectionStringBuilder() 
        {
            DataSource = @"D:\Demo\Assets\Samples\GameDesigner\Example\ExampleServer~\bin\Debug\Data\example2.db"
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
        private readonly MyDictionary<Type, QueueSafe<IQueryTask>> queryTypes = new MyDictionary<Type, QueueSafe<IQueryTask>>();
        private readonly MyDictionary<string, StringBuilder> queryCell = new MyDictionary<string, StringBuilder>();
        private readonly Queue<IQueryTask> queryQueue = new Queue<IQueryTask>();
        private int workerId;

        private SQLiteConnection CheckConn(SQLiteConnection conn)
        {
            if (conn == null)
            {
                conn = new SQLiteConnection(ConnectionText); //数据库连接
                conn.Open();
            }
            
            if (conn.State != ConnectionState.Open)
            {
                conn.Close();
                conn = new SQLiteConnection(ConnectionText); //数据库连接
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
            var configTable = ExecuteReader($"SELECT * FROM `config`");
            foreach (DataRow row in configTable.Rows)
            {
                var data = new ConfigData();
                data.SetContext(this);
                data.Init(row);
                list.Add(data);
            }
            configTable.Dispose();
     // -- 1
            var userinfoTable = ExecuteReader($"SELECT * FROM `userinfo`");
            foreach (DataRow row in userinfoTable.Rows)
            {
                var data = new UserinfoData();
                data.SetContext(this);
                data.Init(row);
                list.Add(data);
            }
            userinfoTable.Dispose();
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
            var configUniqueId = ExecuteScalar<int>(@"SELECT MAX(id) FROM `config`;");
            uniqueIdMap[Example2UniqueIdType.Config] = new UniqueIdGenerator(useMachineId, machineId, machineIdBits, (long)configUniqueId);
            var userinfoUniqueId = ExecuteScalar<int>(@"SELECT MAX(id) FROM `userinfo`;");
            uniqueIdMap[Example2UniqueIdType.Userinfo] = new UniqueIdGenerator(useMachineId, machineId, machineIdBits, (long)userinfoUniqueId);

        }

        /// <summary>
        /// 获取表的自增ID, 每调用一次这个方法ID就会++
        /// </summary>
        /// <param name="type">传入Example2UniqueIdType类的常量字段</param>
        /// <returns></returns>
        public int GetUniqueId(short type)
        {
            return (int)uniqueIdMap[type].NewUniqueId();
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
                                if (value is byte[] bytes)
                                {
                                    var hex = Encoding.ASCII.GetString(bytes);
                                    data[name] = Convert.FromBase64String(hex);
                                }
                                else data[name] = value;
                            }
                            data.RowState = DataRowState.Unchanged;
                            data.SetContext(this);
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

        public void Update(IDataEntity entity)//更新的行,列
        {
            var type = entity.GetType();
            if (!DataRowHandler.TryGetValue(type, out var hash))
                DataRowHandler[type] = hash = new DataEntityQueue(entity.GetCellNameAndTextLength(0, out _));
            hash.Add(entity);
        }

        [Obsolete("此方法已经丢弃，请使用BatchWorker方法代替，并且是每帧执行，不是每秒执行了", true)]
        public bool Executed() { return false; }

        public bool BatchWorker() //每帧调用一次, 需要自己调用此方法
        {
            lock (this) //当WaitBatchWorker执行的时候，这里就需要等待
            {
                BatchQueryWorkers();
                BatchSubmitWorkers();
                return true;
            }
        }

        /// <summary>
        /// 有时候需要强制执行完成所有批操作后再进行查询时用到，可以立即调用WaitBatchWorker方法执行所有的批处理数据
        /// </summary>
        public void WaitBatchWorker()
        {
            lock (this) //当BatchWorker执行的时候，这里就需要等待
            {
                bool isComplete1, isComplete2;
                do
                {
                    isComplete1 = BatchQueryWorkers();
                    isComplete2 = BatchSubmitWorkers();
                }
                while (!isComplete1 || !isComplete2);
            }
        }

        private bool BatchSubmitWorkers()
        {
            var isComplete = true;
            try
            {
                foreach (var item in DataRowHandler)
                {
                    var count = item.Value.Count;
                    if (count <= 0)
                        continue;
                    if (count > BatchSize)
                    {
                        count = BatchSize;
                        isComplete = false;
                    }
                    updateCmdText.Clear();
                    deleteCmdText.Clear();
                    var tableName = item.Key.Name;
                    tableName = tableName.Remove(tableName.Length - 4, 4);
                    foreach (var row in item.Value.hashSet)
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
                            ExecuteNonQuery(tableName, item.Value.primaryKey, updateCmdText, deleteCmdText);
                            updateCmdText.Clear();
                            deleteCmdText.Clear();
                        }
                        if (count-- <= 0)
                            break;
                    }
                    ExecuteNonQuery(tableName, item.Value.primaryKey, updateCmdText, deleteCmdText);
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError("SQL异常: " + ex);
            }
            return isComplete;
        }

        private void ExecuteNonQuery(string tableName, string primaryKey, StringBuilder updateCmdText, StringBuilder deleteCmdText)
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
                deleteCmdText.Insert(0, $"DELETE FROM `{tableName}` WHERE `{primaryKey}` IN (");
                deleteCmdText.Append(");");
                var stopwatch = Stopwatch.StartNew();
                var rowCount = ExecuteNonQuery(deleteCmdText.ToString());
                stopwatch.Stop();
                if (rowCount > 2000) NDebug.Log($"SQL批处理完成:{rowCount} 用时:{stopwatch.Elapsed}");
            }
        }

        private interface IQueryTask
        {
            string CommandText { get; set; }
            bool IsDone { get; set; }
            void SetRows(Example2DB context, DataRow[] rows);
        }

        private class QueryTask<T> : IQueryTask where T : IDataRow, new()
        {
            public string CommandText { get; set; }
            public bool IsDone { get; set; }
            internal T Data;

            public void SetRows(Example2DB context, DataRow[] rows)
            {
                if (rows.Length > 0)
                {
                    Data = new T();
                    Data.SetContext(context);
                    Data.Init(rows[0]);
                }
            }
        }

        private class QueryTaskList<T> : IQueryTask where T : IDataRow, new()
        {
            public string CommandText { get; set; }
            public bool IsDone { get; set; }
            internal T[] Datas;

            public void SetRows(Example2DB context, DataRow[] rows)
            {
                Datas = new T[rows.Length];
                for (int i = 0; i < rows.Length; i++)
                {
                    Datas[i] = new T();
                    Datas[i].SetContext(context);
                    Datas[i].Init(rows[i]);
                }
            }
        }

        /// <summary>
        /// 异步查询, 查询案例: `id`=1 或者 `name` = 'hello'
        /// <para></para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public async UniTask<T> QueryAsync<T>(string filterExpression) where T : IDataRow, new()
        {
            var type = typeof(T);
            if (!queryTypes.TryGetValue(type, out var queue))
                queryTypes[type] = queue = new QueueSafe<IQueryTask>();
            var queryTask = new QueryTask<T>() { CommandText = filterExpression };
            queue.Enqueue(queryTask);
            await UniTaskNetExtensions.Wait(CommandTimeout * 1000, (state) => ((IQueryTask)state).IsDone, queryTask);
            return queryTask.Data;
        }

        /// <summary>
        /// 异步查询, 查询案例: `id`=1 或者 `name` = 'hello'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public async UniTask<T[]> QueryListAsync<T>(string filterExpression) where T : IDataRow, new()
        {
            var type = typeof(T);
            if (!queryTypes.TryGetValue(type, out var queue))
                queryTypes[type] = queue = new QueueSafe<IQueryTask>();
            var queryTask = new QueryTaskList<T>() { CommandText = filterExpression };
            queue.Enqueue(queryTask);
            await UniTaskNetExtensions.Wait(CommandTimeout * 1000, (state) => ((IQueryTask)state).IsDone, queryTask);
            return queryTask.Datas;
        }

        private bool BatchQueryWorkers()
        {
            var isComplete = true;
            try
            {
                foreach (var item in queryTypes)
                {
                    var count = item.Value.Count;
                    if (count <= 0)
                        continue;
                    if (count > BatchSize)
                    {
                        count = BatchSize;
                        isComplete = false;
                    }
                    queryCell.Clear();
                    queryQueue.Clear();
                    var tableName = item.Key.Name;
                    tableName = tableName.Remove(tableName.Length - 4, 4);
                    for (int i = 0; i < count; i++)
                    {
                        if (item.Value.TryDequeue(out var queryTask))
                        {
                            var commandTexts = queryTask.CommandText.Split('=');
                            if (commandTexts.Length == 2)
                            {
                                var cellName = commandTexts[0].Trim();
                                if (!cellName.StartsWith("`")) //兼容.Net Framework写法
                                    cellName = $"`{cellName}`";
                                if (!queryCell.TryGetValue(cellName, out var queryCommandText))
                                    queryCell.Add(cellName, queryCommandText = new StringBuilder($"SELECT * FROM `{tableName}` WHERE {cellName} IN("));
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
                            if (dataTable != null)
                            {
                                var rows = dataTable.Select(queryTask.CommandText);
                                queryTask.SetRows(this, rows);
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
            return isComplete;
        }

        public DataTable ExecuteQueryAsDataTable(string cmdText)
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
                    var dataTable = new DataTable();
                    using (var adapter = new SQLiteDataAdapter(cmd))
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
        public void CreateTables(string password, string dbName = "Example2")
        {
            SetDatabaseName("");
            ConnectionBuilder.Password = password;
            InitConnection();
            int count = (int)ExecuteScalar<long>($"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '{dbName}'");
            if (count <= 0) 
            {
                count = ExecuteNonQuery($@"");
                NDebug.Log($"创建数据库:{dbName}{(count >= 0 ? "成功" : "失败")}!");
                if (count <= 0)
                    return;
            }
            CloseConnect();
            SetDatabaseName(dbName);
            InitConnection();
            //新增表时判断
 // -- 5
            count = (int)ExecuteScalar<long>($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{dbName}' AND table_name = 'config'");
            if (count <= 0)
            {
                count = ExecuteNonQuery(@"CREATE TABLE 'config' (
  'id' INTEGER NOT NULL,
  'name' TEXT,
  'number' INTEGER,
  PRIMARY KEY ('id')
)");
                NDebug.Log($"创建数据表:config{(count >= 0 ? "成功" : "失败")}!");
            }
            else
            {
                count = ExecuteScalar<int>($@"SELECT COUNT(*) AS column_count FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{dbName}' AND TABLE_NAME = 'config';");
                if (count != 3) NDebug.LogWarning("config表有改动,请进行处理! 1.如果你新增了列或删除列, 请用MySqlBuild工具重新生成! 2.如果是多人协作请根据提交信息新增或删除列!");
            }
 // -- 5
            count = (int)ExecuteScalar<long>($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{dbName}' AND table_name = 'userinfo'");
            if (count <= 0)
            {
                count = ExecuteNonQuery(@"CREATE TABLE 'userinfo' (
  'id' integer NOT NULL,
  'account' text,
  'password' text,
  'moveSpeed' real,
  'position' text,
  'rotation' text,
  'health' integer,
  'healthMax' integer,
  'buffer' blob(1024),
  PRIMARY KEY ('id')
)");
                NDebug.Log($"创建数据表:userinfo{(count >= 0 ? "成功" : "失败")}!");
            }
            else
            {
                count = ExecuteScalar<int>($@"SELECT COUNT(*) AS column_count FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{dbName}' AND TABLE_NAME = 'userinfo';");
                if (count != 9) NDebug.LogWarning("userinfo表有改动,请进行处理! 1.如果你新增了列或删除列, 请用MySqlBuild工具重新生成! 2.如果是多人协作请根据提交信息新增或删除列!");
            }
 // -- 6
        }

        private void SetDatabaseName(string name)
        {
            
        }

        private string GetDatabaseName()
        {
            return Path.GetFileNameWithoutExtension(ConnectionBuilder.DataSource);
        }

        /// <summary>
        /// 开始运行数据库中间件处理
        /// </summary>
        public void Start()
        {
            workerId = ThreadManager.Invoke(GetDatabaseName() + "Process", BatchWorker, true);
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        public void Stop() 
        {
            ThreadManager.Event.RemoveEvent(workerId);
            workerId = 0;
        }
    }
}
#endif