#if SERVER
using System;
using System.IO;
using System.Data;
using System.Text;
using System.Threading;
using System.Reflection;
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
        private SQLiteConnection connection;
        private SQLiteConnection Connection
        {
            get
            {
                if (connection == null)
                    return null;
                
                if (connection.State != ConnectionState.Open)
                {
                    connection.Close();
                    connection = new SQLiteConnection(ConnectionText); //数据库连接
                    connection.Open();
                }
                return connection;
            }
            set => connection = value;
        }
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
        private readonly List<NonQueryTask> nonQueryList = new List<NonQueryTask>();
        private readonly StringBuilder commandTextBuilder = new StringBuilder();
        private readonly QueueSafe<NonQueryTask> nonQueryQueue = new QueueSafe<NonQueryTask>();
        private FieldInfo nonQueryHandlerField;
        private int workerId;

        private SQLiteConnection CreateConnection()
        {
            connection?.Close();
            connection = new SQLiteConnection(ConnectionText);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 初始化, 加载数据库数据到内存
        /// </summary>
        /// <param name="onInit">当加载数据库完成调用委托</param>
        public void Init(Action<List<object>> onInit)
        {
            InitTablesId();
            var list = new List<object>();
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
        /// <param name="useMachineId">使用分布式机器ID</param>
        /// <param name="machineId">分布式机器ID</param>
        /// <param name="machineIdBits">分布式机器占用64位中的多少位数</param>
        public void InitTablesId(bool useMachineId = false, int machineId = 0, int machineIdBits = 10)
        {
            InitConnection();
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
        public int GetUniqueId(short type) => (int)uniqueIdMap[type].NewUniqueId();

        /// <summary>
        /// 获取某表的当前自增ID最大值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetCurrentId(short type) => (int)uniqueIdMap[type].CurrentId();

        public UniqueIdGenerator GetUniqueIdMap(short type) => uniqueIdMap[type];

        public void InitConnection()
        {
            
            CreateConnection();
        }

        public DataTable ExecuteReader(string cmdText)
        {
            lock (this)
            {
                var dataTable = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = cmdText;
                        cmd.Connection = Connection;
                        cmd.CommandTimeout = CommandTimeout;
                        cmd.Parameters.Clear();
                        using (var sdr = cmd.ExecuteReader())
                        {
                            dataTable.Load(sdr);
                            QueryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    NDebug.LogError(cmdText + " 错误: " + ex);
                }
                return dataTable;
            }
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
        [Obsolete("这个方法查询会卡当前线程，尽量少用或使用QueryAsync代替", false)]
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
        [Obsolete("这个方法查询会卡当前线程，尽量少用或使用QueryListAsync代替", false)]
        public T[] ExecuteQueryList<T>(string cmdText) where T : IDataRow, new()
        {
            lock (this)
            {
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = cmdText;
                        cmd.Connection = Connection;
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
                return default;
            }
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
        [Obsolete("这个方法查询会卡线程池，如果大量调用，可能会导致线程池占满，尽量少用或使用QueryAsync代替", false)]
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
        [Obsolete("这个方法查询会卡线程池，如果大量调用，可能会导致线程池占满，尽量少用或使用QueryListAsync代替", false)]
        public async UniTask<T[]> ExecuteQueryListAsync<T>(string cmdText) where T : IDataRow, new()
        {
            await UniTask.SwitchToThreadPool();
            var datas = ExecuteQueryList<T>(cmdText);
            return datas;
        }

        [Obsolete("这个方法查询会卡线程池，如果大量调用，可能会导致线程池占满，尽量少用或使用NonQueryAsync代替", false)]
        public async UniTask<int> ExecuteNonQueryAsync(string cmdText, params IDbDataParameter[] parameters)
        {
            await UniTask.SwitchToThreadPool();
            var count = ExecuteNonQuery(cmdText, parameters);
            return count;
        }

        [Obsolete("这个方法查询会卡当前线程，尽量少用或使用NonQueryAsync代替", false)]
        public int ExecuteNonQuery(string cmdText, params IDbDataParameter[] parameters)
        {
            lock (this)
            {
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = cmdText;
                        cmd.Connection = Connection;
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
        }

        public T ExecuteScalar<T>(string cmdText, params IDbDataParameter[] parameters)
        {
            lock (this)
            {
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = cmdText;
                        cmd.Connection = Connection;
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
                ExecuteNonQueryWorkers();
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
                int affectedRows;
                do
                {
                    isComplete1 = BatchQueryWorkers();
                    isComplete2 = BatchSubmitWorkers();
                    affectedRows = ExecuteNonQueryWorkers();
                }
                while (!isComplete1 || !isComplete2 || affectedRows > 0);
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

        private class NonQueryTask
        {
            public string CommandText { get; set; }
            public bool IsDone { get; set; }
            public int AffectedRows { get; set; }
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
            await UniTaskNetExtensions.Wait(CommandTimeout * 1000, (state) => state.IsDone, queryTask);
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
            await UniTaskNetExtensions.Wait(CommandTimeout * 1000, (state) => state.IsDone, queryTask);
            return queryTask.Datas;
        }

        /// <summary>
        /// 非查询执行，异步
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns>影响行数</returns>
        public async UniTask<int> NonQueryAsync(string cmdText)
        {
            var query = new NonQueryTask()
            {
                CommandText = cmdText,
            };
            nonQueryQueue.Enqueue(query);
            await UniTaskNetExtensions.Wait(CommandTimeout * 1000, (state) => state.IsDone, query);
            return query.AffectedRows;
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
                    string commandText;
                    string[] commandTexts;
                    var commandBuilder = new StringBuilder($"SELECT * FROM `{tableName}` WHERE ");
                    for (int i = 0; i < count; i++)
                    {
                        if (item.Value.TryDequeue(out var queryTask))
                        {
                            commandText = queryTask.CommandText;
                            commandText = commandText.Replace("  ", " "); //如果有两个空格则合成一个空格
                            commandTexts = commandText.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int x = 0; x < commandTexts.Length; x += 4)
                            {
                                var cellName = commandTexts[x].Trim();
                                if (!cellName.StartsWith("`")) //兼容.Net Framework写法
                                    cellName = $"`{cellName}`";
                                if (!queryCell.TryGetValue(cellName, out var queryCommandText))
                                    queryCell.Add(cellName, queryCommandText = new StringBuilder($"{cellName} IN("));
                                queryCommandText.Append($"{commandTexts[x + 2]},");
                            }
                            queryQueue.Enqueue(queryTask);
                        }
                    }
                    foreach (var queryCommandText in queryCell.Values)
                    {
                        queryCommandText[queryCommandText.Length - 1] = ' ';
                        queryCommandText.Append(")");
                        commandText = queryCommandText.ToString();
                        commandBuilder.Append(commandText);
                        commandBuilder.Append(" or ");
                    }
                    commandBuilder.Remove(commandBuilder.Length - 4, 4);
                    commandText = commandBuilder.ToString();
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
            catch (Exception ex)
            {
                NDebug.LogError("批量查询异常: " + ex);
            }
            return isComplete;
        }

        public DataTable ExecuteQueryAsDataTable(string cmdText)
        {
            lock (this)
            {
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = cmdText;
                        cmd.Connection = Connection;
                        cmd.CommandTimeout = CommandTimeout;
                        cmd.Parameters.Clear();
                        var dataTable = new DataTable();
                        using (var adapter = new SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                        return dataTable;
                    }
                }
                catch (Exception ex)
                {
                    NDebug.LogError(cmdText + " 错误: " + ex);
                }
                return default;
            }
        }

        private int ExecuteNonQueryWorkers()
        {
            var count = nonQueryQueue.Count;
            if (count <= 0)
                return 0;
            if (count > BatchSize)
                count = BatchSize;
            nonQueryList.Clear();
            commandTextBuilder.Clear();
            for (int i = 0; i < count; i++)
            {
                if (nonQueryQueue.TryDequeue(out var nonQuery))
                {
                    nonQueryList.Add(nonQuery);
                    commandTextBuilder.AppendLine(nonQuery.CommandText);
                }
            }
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = commandTextBuilder.ToString();
                    cmd.Connection = Connection;
                    cmd.CommandTimeout = CommandTimeout;
                    nonQueryHandlerField?.SetValue(cmd, new Action<int, int>(OnNonQueryHandler));
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError("非查询错误: " + ex);
                for (int i = 0; i < nonQueryList.Count; i++)
                {
                    nonQueryList[i].IsDone = true;
                }
            }
            return 0;
        }

        private void OnNonQueryHandler(int index, int affectedRows)
        {
            var nonQuery = nonQueryList[index];
            nonQuery.AffectedRows = affectedRows;
            nonQuery.IsDone = true;
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
            connection?.Clone();
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