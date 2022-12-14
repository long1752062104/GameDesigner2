using System;
using System.Data;
using System.Threading.Tasks;
using Net.System;
using System.Collections.Generic;
using System.Text;
#if SERVER
using System.Data.SQLite;
#endif
using Net.Share;

#if ANTICHEAT
using Boolean = CodeStage.AntiCheat.ObscuredTypes.ObscuredBool;
#else
using Boolean = System.Boolean;
#endif
#if ANTICHEAT
using Char = CodeStage.AntiCheat.ObscuredTypes.ObscuredChar;
#else
using Char = System.Char;
#endif
#if ANTICHEAT
using SByte = CodeStage.AntiCheat.ObscuredTypes.ObscuredSByte;
#else
using SByte = System.SByte;
#endif
#if ANTICHEAT
using Byte = CodeStage.AntiCheat.ObscuredTypes.ObscuredByte;
#else
using Byte = System.Byte;
#endif
#if ANTICHEAT
using Int16 = CodeStage.AntiCheat.ObscuredTypes.ObscuredShort;
#else
using Int16 = System.Int16;
#endif
#if ANTICHEAT
using UInt16 = CodeStage.AntiCheat.ObscuredTypes.ObscuredUShort;
#else
using UInt16 = System.UInt16;
#endif
#if ANTICHEAT
using Int32 = CodeStage.AntiCheat.ObscuredTypes.ObscuredInt;
#else
using Int32 = System.Int32;
#endif
#if ANTICHEAT
using UInt32 = CodeStage.AntiCheat.ObscuredTypes.ObscuredUInt;
#else
using UInt32 = System.UInt32;
#endif
#if ANTICHEAT
using Int64 = CodeStage.AntiCheat.ObscuredTypes.ObscuredLong;
#else
using Int64 = System.Int64;
#endif
#if ANTICHEAT
using UInt64 = CodeStage.AntiCheat.ObscuredTypes.ObscuredULong;
#else
using UInt64 = System.UInt64;
#endif
#if ANTICHEAT
using Single = CodeStage.AntiCheat.ObscuredTypes.ObscuredFloat;
#else
using Single = System.Single;
#endif
#if ANTICHEAT
using Double = CodeStage.AntiCheat.ObscuredTypes.ObscuredDouble;
#else
using Double = System.Double;
#endif
#if ANTICHEAT
using Decimal = CodeStage.AntiCheat.ObscuredTypes.ObscuredDecimal;
#else
using Decimal = System.Decimal;
#endif
#if ANTICHEAT
using String = CodeStage.AntiCheat.ObscuredTypes.ObscuredString;
#else
using String = System.String;
#endif


    /// <summary>
    /// 此类由MySqlDataBuild工具生成, 请不要在此类编辑代码! 请新建一个类文件进行分写
    /// <para>MySqlDataBuild工具提供Rpc自动同步到mysql数据库的功能, 提供数据库注释功能</para>
    /// <para><see href="此脚本支持unity的CodeStage.AntiCheat防修改数值插件, 需要在uniyt的预编译处添加:ANTICHEAT关键字即可"/> </para>
    /// MySqlDataBuild工具gitee地址:https://gitee.com/leng_yue/my-sql-data-build
    /// </summary>
    public partial class ConfigData : IDataRow
    {
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public DataRowState RowState { get; set; }
    #if SERVER
        private readonly HashSetSafe<int> columns = new HashSetSafe<int>();
    #endif

        private Int64 id;
        /// <summary>{KEYNOTE}</summary>
        public Int64 Id
        {
            get { return id; }
            set { this.id = value; }
        }

    
        private String name;
        /// <summary>{NOTE}</summary>
        public String Name
        {
            get { return name; }
            set
            {
                if (this.name == value)
                    return;
                if(value==null) value = string.Empty;
                this.name = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(1);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                NameCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncName
        {
            get { return name; }
            set
            {
                if (this.name == value)
                    return;
                if(value==null) value = string.Empty;
                this.name = value;
                NameCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncIDName
        {
            get { return name; }
            set
            {
                if (this.name == value)
                    return;
                if(value==null) value = string.Empty;
                this.name = value;
                SyncNameCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void NameCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.NAME, (System.String)name);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncNameCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.NAME, (System.Int64)id, (System.String)name);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.NAME)]
        private void NameCall(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(ConfigData));收集Rpc
        {
            Name = value;
            OnName?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnName;
    
        private Int64 number;
        /// <summary>{NOTE}</summary>
        public Int64 Number
        {
            get { return number; }
            set
            {
                if (this.number == value)
                    return;
                
                this.number = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(2);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                NumberCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Int64 SyncNumber
        {
            get { return number; }
            set
            {
                if (this.number == value)
                    return;
                
                this.number = value;
                NumberCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Int64 SyncIDNumber
        {
            get { return number; }
            set
            {
                if (this.number == value)
                    return;
                
                this.number = value;
                SyncNumberCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void NumberCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.NUMBER, (System.Int64)number);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncNumberCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.NUMBER, (System.Int64)id, (System.Int64)number);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.NUMBER)]
        private void NumberCall(Int64 value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(ConfigData));收集Rpc
        {
            Number = value;
            OnNumber?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnNumber;
    

        public ConfigData() { }

    #if SERVER
        public ConfigData(params object[] parms)
        {
            NewTableRow(parms);
        }
        public void NewTableRow()
        {
            for (int i = 0; i < 3; i++)
            {
                var obj = this[i];
                if (obj == null)
                    continue;
                var defaultVal = GetDefaultValue(obj.GetType());
                if (obj.Equals(defaultVal))
                    continue;
                columns.Add(i);
            }
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
                this[i] = parms[i];
                columns.Add(i);
            }
            RowState = DataRowState.Added;
            Example2DB.I.Update(this);
        }
        public string GetCellName(int index)
        {
            switch (index)
            {
    
                case 0:
                    return "id";
    
                case 1:
                    return "name";
    
                case 2:
                    return "number";
    
            }
            throw new Exception("错误");
        }
        public object this[int index]
        {
            get
            {
                switch (index)
                {
    
                    case 0:
                        return this.id;
    
                    case 1:
                        return this.name;
    
                    case 2:
                        return this.number;
    
                }
                throw new Exception("错误");
            }
            set
            {
                switch (index)
                {
    
                    case 0:
                        this.id = (Int64)value;
                        break;
    
                    case 1:
                        this.name = (String)value;
                        break;
    
                    case 2:
                        this.number = (Int64)value;
                        break;
    
                }
            }
        }

        public void Delete()
        {
            RowState = DataRowState.Detached;
            Example2DB.I.Update(this);
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
            var data = Example2DB.ExecuteQuery<ConfigData>(cmdText);
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
        public static async Task<ConfigData> QueryAsync(string filterExpression)
        {
            var cmdText = $"select * from config where {filterExpression}; ";
            var data = await Example2DB.ExecuteQueryAsync<ConfigData>(cmdText);
            return data;
        }
        public static ConfigData[] QueryList(string filterExpression)
        {
            var cmdText = $"select * from config where {filterExpression}; ";
            var datas = Example2DB.ExecuteQueryList<ConfigData>(cmdText);
            return datas;
        }
        public static async Task<ConfigData[]> QueryListAsync(string filterExpression)
        {
            var cmdText = $"select * from config where {filterExpression}; ";
            var datas = await Example2DB.ExecuteQueryListAsync<ConfigData>(cmdText);
            return datas;
        }
        public void Update() => Update(0, 3);
        public void Update(int start, int end)
        {
            if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached | RowState == DataRowState.Added | RowState == 0) return;
    
            for (int i = start; i < end; i++)
                columns.Add(i);
            RowState = DataRowState.Modified;
            Example2DB.I.Update(this);
    
        }
    #endif

        public void Init(DataRow row)
        {
            RowState = DataRowState.Unchanged;
    
            if (row[0] is Int64 id)
                this.id = id;
    
            if (row[1] is String name)
                this.name = name;
    
            if (row[2] is Int64 number)
                this.number = number;
    
        }

        public void AddedSql(StringBuilder sb, List<IDbDataParameter> parms, ref int parmsLen)
        {
    #if SERVER
            if(columns.Count == 0)//如果没有修改一个字段是不允许提交的
                return;
    
            string cmdText = "REPLACE INTO config(";
            string cmdText1 = "VALUES(";
            foreach (var column in columns)
            {
                var name = GetCellName(column);
                cmdText += $"`{name}`,";
                var value = this[column];
                if (value is string | value is DateTime)
                    cmdText1 += $"'{value}',";
                else if (value is byte[] buffer)
                {
                    var count = parms.Count;
                    cmdText1 += $"@buffer{count},";
                    parms.Add(new SQLiteParameter($"@buffer{count}", buffer));
                    parmsLen += buffer.Length;
                }
                else cmdText1 += $"{value},";
                columns.Remove(column);
            }
            cmdText = cmdText.TrimEnd(',');
            cmdText1 = cmdText1.TrimEnd(',');
            cmdText += ")" + cmdText1 + "); ";
    
            sb.Append(cmdText);
            RowState = DataRowState.Unchanged;
    #endif
        }

        public void ModifiedSql(StringBuilder sb, List<IDbDataParameter> parms, ref int parmsLen)
        {
    #if SERVER
            if (RowState == DataRowState.Detached | RowState == DataRowState.Deleted | RowState == DataRowState.Added | RowState == 0)
                return;
            if(columns.Count == 0)//如果没有修改一个字段是不允许提交的
                return;
            string cmdText = $"UPDATE config SET ";
            foreach (var column in columns)
            {
                var name = GetCellName(column);
                var value = this[column];
                if (value is string | value is DateTime)
                    cmdText += $"`{name}`='{value}',";
                else if (value is byte[] buffer)
                {
                    var count = parms.Count;
                    cmdText += $"`{name}`=@buffer{count},";
                    parms.Add(new SQLiteParameter($"@buffer{count}", buffer));
                    parmsLen += buffer.Length;
                }
                else cmdText += $"`{name}`={value},";
                columns.Remove(column);
            }
            cmdText = cmdText.TrimEnd(',');
            cmdText += $" WHERE `id`={id}; ";
            sb.Append(cmdText);
            RowState = DataRowState.Unchanged;
    #endif
        }

        public void DeletedSql(StringBuilder sb)
        {
    #if SERVER
            if (RowState == DataRowState.Deleted)
                return;
            string cmdText = $"DELETE FROM config WHERE `id` = {id}; ";
            sb.Append(cmdText);
            RowState = DataRowState.Deleted;
    #endif
        }
    }
