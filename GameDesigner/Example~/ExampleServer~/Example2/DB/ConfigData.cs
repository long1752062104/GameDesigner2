using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Net.Share;
using Net.Event;
#if SERVER
using Net.System;
using Cysharp.Threading.Tasks;
using System.Data.SQLite;
#endif
using BooleanObs = Net.Common.PropertyObserverAuto<bool>;
using ByteObs = Net.Common.PropertyObserverAuto<byte>;
using SByteObs = Net.Common.PropertyObserverAuto<sbyte>;
using Int16Obs = Net.Common.PropertyObserverAuto<short>;
using UInt16Obs = Net.Common.PropertyObserverAuto<ushort>;
using CharObs = Net.Common.PropertyObserverAuto<char>;
using Int32Obs = Net.Common.PropertyObserverAuto<int>;
using UInt32Obs = Net.Common.PropertyObserverAuto<uint>;
using SingleObs = Net.Common.PropertyObserverAuto<float>;
using Int64Obs = Net.Common.PropertyObserverAuto<long>;
using UInt64Obs = Net.Common.PropertyObserverAuto<ulong>;
using DoubleObs = Net.Common.PropertyObserverAuto<double>;
using DateTimeObs = Net.Common.PropertyObserverAuto<System.DateTime>;
using BytesObs = Net.Common.PropertyObserverAuto<byte[]>;
using StringObs = Net.Common.PropertyObserverAuto<string>;

namespace Example2
{
    /// <summary>
    /// 此类由MySqlDataBuild工具生成, 请不要在此类编辑代码! 请新建一个类文件进行分写
    /// <para>MySqlDataBuild工具提供Rpc自动同步到mysql数据库的功能, 提供数据库注释功能</para>
    /// <para><see href="此脚本支持防内存修改器, 需要在uniyt的预编译处添加:ANTICHEAT关键字即可"/> </para>
    /// MySqlDataBuild工具gitee地址:https://gitee.com/leng_yue/my-sql-data-build
    /// </summary>
    public partial class ConfigData : IDataRow
    {
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public DataRowState RowState { get; set; }
    #if SERVER
        internal Net.Server.NetPlayer client;
    #endif
        /// <summary>当属性被修改时调用, 参数1: 哪个字段被修改(表名_字段名), 参数2:被修改的值</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action<Example2HashProto, object> OnValueChanged;

        private Int64 id;
        /// <summary></summary>
        public Int64 Id { get { return id; } set { this.id = value; } }

     // -- 1
        private readonly StringObs name = new StringObs("ConfigData_name", false, null);

        /// <summary> --获得属性观察对象</summary>
        internal StringObs NameObserver => name;

        /// <summary></summary>
        public String Name { get => GetNameValue(); set => CheckNameValue(value, 0); }

        /// <summary> --同步到数据库</summary>
        internal String SyncName { get => GetNameValue(); set => CheckNameValue(value, 1); }

        /// <summary> --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal String SyncIDName { get => GetNameValue(); set => CheckNameValue(value, 2); }

        private String GetNameValue() => this.name.Value;

        private void CheckNameValue(String value, int type) 
        {
            if (this.name.Value == value)
                return;
            this.name.Value = value;
            if (type == 0)
                CheckUpdate(1);
            else if (type == 1)
                NameCall(false);
            else if (type == 2)
                NameCall(true);
            OnValueChanged?.Invoke(Example2HashProto.CONFIG_NAME, value);
        }

        /// <summary> --同步当前值到服务器Player对象上，需要处理</summary>
        public void NameCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.ConfigData_SyncID], name.Value };
            else objects = new object[] { name.Value };
#if SERVER
            CheckUpdate(1);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.CONFIG_NAME, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.CONFIG_NAME, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.CONFIG_NAME)]
        private void NameRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(ConfigData));收集Rpc
        {
            Name = value;
        }
     // -- 1
        private readonly Int64Obs number = new Int64Obs("ConfigData_number", true, null);

        /// <summary> --获得属性观察对象</summary>
        internal Int64Obs NumberObserver => number;

        /// <summary></summary>
        public Int64 Number { get => GetNumberValue(); set => CheckNumberValue(value, 0); }

        /// <summary> --同步到数据库</summary>
        internal Int64 SyncNumber { get => GetNumberValue(); set => CheckNumberValue(value, 1); }

        /// <summary> --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal Int64 SyncIDNumber { get => GetNumberValue(); set => CheckNumberValue(value, 2); }

        private Int64 GetNumberValue() => this.number.Value;

        private void CheckNumberValue(Int64 value, int type) 
        {
            if (this.number.Value == value)
                return;
            this.number.Value = value;
            if (type == 0)
                CheckUpdate(2);
            else if (type == 1)
                NumberCall(false);
            else if (type == 2)
                NumberCall(true);
            OnValueChanged?.Invoke(Example2HashProto.CONFIG_NUMBER, value);
        }

        /// <summary> --同步当前值到服务器Player对象上，需要处理</summary>
        public void NumberCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.ConfigData_SyncID], number.Value };
            else objects = new object[] { number.Value };
#if SERVER
            CheckUpdate(2);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.CONFIG_NUMBER, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.CONFIG_NUMBER, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.CONFIG_NUMBER)]
        private void NumberRpc(Int64 value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(ConfigData));收集Rpc
        {
            Number = value;
        }
     // -- 2

        public ConfigData() { }

    #if SERVER
        public ConfigData(params object[] parms) : base()
        {
            NewTableRow(parms);
        }
        public void NewTableRow()
        {
            RowState = DataRowState.Added;
            Example2DB.I.Update(this);
        }
        public object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
        public void NewTableRow(params object[] parms)
        {
            if (parms == null)
                return;
            if (parms.Length == 0)
                return;
            for (int i = 0; i < parms.Length; i++)
            {
                if (parms[i] == null)
                    continue;
                this[i] = parms[i];
            }
            RowState = DataRowState.Added;
            Example2DB.I.Update(this);
        }
        public string GetCellNameAndTextLength(int index, out int length)
        {
            switch (index)
            {
     // -- 3
                case 0: length = 65536; return "id";
     // -- 3
                case 1: length = 65536; return "name";
     // -- 3
                case 2: length = 65536; return "number";
     // -- 4
            }
            throw new Exception("错误");
        }
    #endif

        public object this[int index]
        {
            get
            {
                switch (index)
                {
     // -- 5
                    case 0: return this.id;
     // -- 5
                    case 1: return this.name.Value;
     // -- 5
                    case 2: return this.number.Value;
     // -- 6
                }
                throw new Exception("错误");
            }
            set
            {
                switch (index)
                {
     // -- 7
                    case 0:
                        this.id = (Int64)value;
                        break;
     // -- 7
                    case 1:
                        CheckNameValue((String)value, -1);
                        break;
     // -- 7
                    case 2:
                        CheckNumberValue((Int64)value, -1);
                        break;
     // -- 8
                }
            }
        }

    #if SERVER
        public void Delete(bool immediately = false)
        {
            if (immediately)
            {
                var sb = new StringBuilder();
                DeletedSql(sb);
                _ = Example2DB.I.ExecuteNonQuery(sb.ToString(), null);
            }
            else
            {
                RowState = DataRowState.Detached;
                Example2DB.I.Update(this);
            }
        }

        /// <summary>
        /// 查询1: Query("`id`=1");
        /// <para></para>
        /// 查询2: Query("`id`=1 and `index`=1");
        /// <para></para>
        /// 查询3: Query("`id`=1 or `index`=1");
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public static ConfigData Query(string filterExpression)
        {
            var cmdText = $"select * from config where {filterExpression}; ";
            var data = Example2DB.I.ExecuteQuery<ConfigData>(cmdText);
            return data;
        }
        /// <summary>
        /// 查询1: Query("`id`=1");
        /// <para></para>
        /// 查询2: Query("`id`=1 and `index`=1");
        /// <para></para>
        /// 查询3: Query("`id`=1 or `index`=1");
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public static async UniTask<ConfigData> QueryAsync(string filterExpression)
        {
            var cmdText = $"select * from config where {filterExpression}; ";
            var data = await Example2DB.I.ExecuteQueryAsync<ConfigData>(cmdText);
            return data;
        }
        public static ConfigData[] QueryList(string filterExpression)
        {
            var cmdText = $"select * from config where {filterExpression}; ";
            var datas = Example2DB.I.ExecuteQueryList<ConfigData>(cmdText);
            return datas;
        }
        public static async UniTask<ConfigData[]> QueryListAsync(string filterExpression)
        {
            var cmdText = $"select * from config where {filterExpression}; ";
            var datas = await Example2DB.I.ExecuteQueryListAsync<ConfigData>(cmdText);
            return datas;
        }
        public void Update()
        {
            if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached | RowState == DataRowState.Added | RowState == 0) return;
     // -- 9
            RowState = DataRowState.Modified;
            Example2DB.I.Update(this);
     // -- 10
        }
    #endif

        public void Init(DataRow row)
        {
            RowState = DataRowState.Unchanged;
     // -- 11
            if (row[0] is Int64 id)
                this.id = id;
     // -- 11
            if (row[1] is String name)
                CheckNameValue(name, -1);
     // -- 11
            if (row[2] is Int64 number)
                CheckNumberValue(number, -1);
     // -- 12
        }

        public void AddedSql(StringBuilder sb)
        {
    #if SERVER
    
            BulkLoaderBuilder(sb);
    
            RowState = DataRowState.Unchanged;
    #endif
        }

        public void ModifiedSql(StringBuilder sb)
        {
    #if SERVER
            if (RowState == DataRowState.Detached | RowState == DataRowState.Deleted | RowState == DataRowState.Added | RowState == 0)
                return;
            BulkLoaderBuilder(sb);
            RowState = DataRowState.Unchanged;
    #endif
        }

        public void DeletedSql(StringBuilder sb)
        {
    #if SERVER
            if (RowState == DataRowState.Deleted)
                return;
            string cmdText = $"DELETE FROM config {CheckSqlKey(0, id)}";
            sb.Append(cmdText);
            RowState = DataRowState.Deleted;
    #endif
        }

    #if SERVER
        public void BulkLoaderBuilder(StringBuilder sb)
        {
 // -- 14
            string keyText = "";
            string valueText = "";
            for (int i = 0; i < 3; i++)
            {
                var name = GetCellNameAndTextLength(i, out var length);
                var value = this[i];
                if (value == null) //空的值会导致sql语句错误
                    continue;
                keyText += $"`{name}`,";
                if (value is string text)
                {
                    Example2DB.I.CheckStringValue(ref text, length);
                    valueText += $"'{text}',";
                }
                else if (value is DateTime dateTime)
                {
                    valueText += $"'{dateTime.ToString("G")}',";
                }
                else if (value is bool boolVal)
                {
                    valueText += $"'{boolVal}',";
                }
                else if (value is byte[] buffer)
                {
                    var base64Str = Convert.ToBase64String(buffer, Base64FormattingOptions.None);
                    if (buffer.Length >= length)
                    {
                        NDebug.LogError($"config表{name}列长度溢出!");
                        continue;
                    }
                    valueText += $"'{base64Str}',";
                }
                else 
                {
                    valueText += $"{value},";
                }
            }
            keyText = keyText.TrimEnd(',');
            valueText = valueText.TrimEnd(',');
            if (!string.IsNullOrEmpty(keyText) & !string.IsNullOrEmpty(valueText))
            {
                sb.Append($"REPLACE INTO config ({keyText}) VALUES ({valueText});");
                sb.AppendLine();
            }
 // -- 15
        }
    #endif

    #if SERVER
        private string CheckSqlKey(int column, object value)
        {
            var name = GetCellNameAndTextLength(column, out var length);
            if (value == null) //空的值会导致sql语句错误
                return "";
            if (value is string text)
            {
                Example2DB.I.CheckStringValue(ref text, length);
                return $" WHERE `{name}`='{text}'; ";
            }
            else if (value is DateTime)
                return $" WHERE `{name}`='{value}'; ";
            else if (value is byte[])
                return "";
            return $" WHERE `{name}`={value}; ";
        }
    #endif

        private void CheckUpdate(int cellIndex)
        {
#if SERVER
            if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
            if (RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                RowState = DataRowState.Modified;
            Example2DB.I.Update(this);
#endif
        }
    }
}