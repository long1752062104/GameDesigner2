using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Net.Share;
using Net.Event;
#if SERVER
using Net.System;
using Cysharp.Threading.Tasks;
using MySql.Data.MySqlClient;
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
using TimeSpanObs = Net.Common.PropertyObserverAuto<System.TimeSpan>;
using BytesObs = Net.Common.PropertyObserverAuto<byte[]>;
using StringObs = Net.Common.PropertyObserverAuto<string>;

namespace Distributed
{
    /// <summary>
    /// 此类由MySqlDataBuild工具生成, 请不要在此类编辑代码! 请新建一个类文件进行分写
    /// <para>MySqlDataBuild工具提供Rpc自动同步到mysql数据库的功能, 提供数据库注释功能</para>
    /// <para><see href="此脚本支持防内存修改器, 需要在uniyt的预编译处添加:ANTICHEAT关键字即可"/> </para>
    /// MySqlDataBuild工具gitee地址:https://gitee.com/leng_yue/my-sql-data-build
    /// </summary>
    public partial class UserData : IDataEntity
    {
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public DataRowState RowState { get; set; }
    #if SERVER
        internal Net.Server.NetPlayer client;
        private DistributedDB context;
        internal DistributedDB Context
        {
            get
            {
                if (context == null) //兼容.NET Framework写法
                    context = DistributedDB.I; 
                return context;
            }
            set => context = value;
        }
        public void SetContext(object context)
        {
            this.context = context as DistributedDB;
        }
    #else
        public void SetContext(object context) { }
    #endif
        private Int64 id;
        /// <summary></summary>
        public Int64 Id { get { return id; } set { this.id = value; } }

     //1
        private readonly StringObs account = new StringObs("UserData_account", false, null);

        /// <summary> --获得属性观察对象</summary>
        internal StringObs AccountObserver => account;

        /// <summary></summary>
        public String Account { get => GetAccountValue(); set => CheckAccountValue(value, 0); }

        /// <summary> --同步到数据库</summary>
        internal String SyncAccount { get => GetAccountValue(); set => CheckAccountValue(value, 1); }

        /// <summary> --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal String SyncIDAccount { get => GetAccountValue(); set => CheckAccountValue(value, 2); }

        private String GetAccountValue() => this.account.Value;

        private void CheckAccountValue(String value, int type) 
        {
            if (this.account.Value == value)
                return;
            this.account.Value = value;
            if (type == 0)
                CheckUpdate(1);
            else if (type == 1)
                AccountCall(false);
            else if (type == 2)
                AccountCall(true);
            if (RowState != 0) //初始化完成才能通知
                DistributedDBEvent.OnValueChanged?.Invoke(this, DistributedHashProto.USER_ACCOUNT, id, value);
        }

        /// <summary> --同步当前值到服务器Player对象上，需要处理</summary>
        public void AccountCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[DistributedDBEvent.UserData_SyncID], account.Value };
            else objects = new object[] { account.Value };
#if SERVER
            CheckUpdate(1);
            DistributedDBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_ACCOUNT, objects);
#else
            DistributedDBEvent.Client.Call(NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_ACCOUNT, objects);
#endif
        }

        [DataRowField("account", 1)]
        [Rpc(hash = (ushort)DistributedHashProto.USER_ACCOUNT)]
        private void AccountRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserData));收集Rpc
        {
            Account = value;
        }
     //1
        private readonly StringObs password = new StringObs("UserData_password", false, null);

        /// <summary> --获得属性观察对象</summary>
        internal StringObs PasswordObserver => password;

        /// <summary></summary>
        public String Password { get => GetPasswordValue(); set => CheckPasswordValue(value, 0); }

        /// <summary> --同步到数据库</summary>
        internal String SyncPassword { get => GetPasswordValue(); set => CheckPasswordValue(value, 1); }

        /// <summary> --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal String SyncIDPassword { get => GetPasswordValue(); set => CheckPasswordValue(value, 2); }

        private String GetPasswordValue() => this.password.Value;

        private void CheckPasswordValue(String value, int type) 
        {
            if (this.password.Value == value)
                return;
            this.password.Value = value;
            if (type == 0)
                CheckUpdate(2);
            else if (type == 1)
                PasswordCall(false);
            else if (type == 2)
                PasswordCall(true);
            if (RowState != 0) //初始化完成才能通知
                DistributedDBEvent.OnValueChanged?.Invoke(this, DistributedHashProto.USER_PASSWORD, id, value);
        }

        /// <summary> --同步当前值到服务器Player对象上，需要处理</summary>
        public void PasswordCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[DistributedDBEvent.UserData_SyncID], password.Value };
            else objects = new object[] { password.Value };
#if SERVER
            CheckUpdate(2);
            DistributedDBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_PASSWORD, objects);
#else
            DistributedDBEvent.Client.Call(NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_PASSWORD, objects);
#endif
        }

        [DataRowField("password", 2)]
        [Rpc(hash = (ushort)DistributedHashProto.USER_PASSWORD)]
        private void PasswordRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserData));收集Rpc
        {
            Password = value;
        }
     //1
        private readonly StringObs name = new StringObs("UserData_name", false, null);

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
                CheckUpdate(3);
            else if (type == 1)
                NameCall(false);
            else if (type == 2)
                NameCall(true);
            if (RowState != 0) //初始化完成才能通知
                DistributedDBEvent.OnValueChanged?.Invoke(this, DistributedHashProto.USER_NAME, id, value);
        }

        /// <summary> --同步当前值到服务器Player对象上，需要处理</summary>
        public void NameCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[DistributedDBEvent.UserData_SyncID], name.Value };
            else objects = new object[] { name.Value };
#if SERVER
            CheckUpdate(3);
            DistributedDBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_NAME, objects);
#else
            DistributedDBEvent.Client.Call(NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_NAME, objects);
#endif
        }

        [DataRowField("name", 3)]
        [Rpc(hash = (ushort)DistributedHashProto.USER_NAME)]
        private void NameRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserData));收集Rpc
        {
            Name = value;
        }
     //1
        private readonly Int32Obs level = new Int32Obs("UserData_level", true, null);

        /// <summary> --获得属性观察对象</summary>
        internal Int32Obs LevelObserver => level;

        /// <summary></summary>
        public Int32 Level { get => GetLevelValue(); set => CheckLevelValue(value, 0); }

        /// <summary> --同步到数据库</summary>
        internal Int32 SyncLevel { get => GetLevelValue(); set => CheckLevelValue(value, 1); }

        /// <summary> --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal Int32 SyncIDLevel { get => GetLevelValue(); set => CheckLevelValue(value, 2); }

        private Int32 GetLevelValue() => this.level.Value;

        private void CheckLevelValue(Int32 value, int type) 
        {
            if (this.level.Value == value)
                return;
            this.level.Value = value;
            if (type == 0)
                CheckUpdate(4);
            else if (type == 1)
                LevelCall(false);
            else if (type == 2)
                LevelCall(true);
            if (RowState != 0) //初始化完成才能通知
                DistributedDBEvent.OnValueChanged?.Invoke(this, DistributedHashProto.USER_LEVEL, id, value);
        }

        /// <summary> --同步当前值到服务器Player对象上，需要处理</summary>
        public void LevelCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[DistributedDBEvent.UserData_SyncID], level.Value };
            else objects = new object[] { level.Value };
#if SERVER
            CheckUpdate(4);
            DistributedDBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_LEVEL, objects);
#else
            DistributedDBEvent.Client.Call(NetCmd.SyncPropertyData, (ushort)DistributedHashProto.USER_LEVEL, objects);
#endif
        }

        [DataRowField("level", 4)]
        [Rpc(hash = (ushort)DistributedHashProto.USER_LEVEL)]
        private void LevelRpc(Int32 value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserData));收集Rpc
        {
            Level = value;
        }
     //2

        public UserData() { }

    #if SERVER
        public UserData(params object[] parms) : base()
        {
            NewTableRow(parms);
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
            NewTableRow();
        }
        public void NewTableRow()
        {
            if (id == default)
                id = (Int64)Context.GetUniqueId(DistributedUniqueIdType.User);
            RowState = DataRowState.Added;
            Context.Update(this);
        }
        public void NewTableRowSync()
        {
            if (id == default)
                id = (Int64)Context.GetUniqueId(DistributedUniqueIdType.User);
            var sb = new StringBuilder();
            BulkLoaderBuilder(sb, false);
            Context.ExecuteNonQuery(sb.ToString());
            RowState = DataRowState.Unchanged;
        }
        public string GetCellNameAndTextLength(int index, out uint length)
        {
            switch (index)
            {
     //3
                case 0: length = 0; return "id";
     //3
                case 1: length = 255; return "account";
     //3
                case 2: length = 255; return "password";
     //3
                case 3: length = 255; return "name";
     //3
                case 4: length = 0; return "level";
     //4
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
     //5
                    case 0: return this.id;
     //5
                    case 1: return this.account.Value;
     //5
                    case 2: return this.password.Value;
     //5
                    case 3: return this.name.Value;
     //5
                    case 4: return this.level.Value;
     //6
                }
                throw new Exception("错误");
            }
            set
            {
                switch (index)
                {
     //7
                    case 0:
                        this.id = (Int64)value;
                        break;
     //7
                    case 1:
                        CheckAccountValue((String)Convert.ChangeType(value, typeof(String)), -1);
                        break;
     //7
                    case 2:
                        CheckPasswordValue((String)Convert.ChangeType(value, typeof(String)), -1);
                        break;
     //7
                    case 3:
                        CheckNameValue((String)Convert.ChangeType(value, typeof(String)), -1);
                        break;
     //7
                    case 4:
                        CheckLevelValue((Int32)Convert.ChangeType(value, typeof(Int32)), -1);
                        break;
     //8
                }
            }
        }

        public object this[string name]
        {
            get
            {
                switch (name)
                {
     //9
                    case "id": return this.id;
     //9
                    case "account": return this.account.Value;
     //9
                    case "password": return this.password.Value;
     //9
                    case "name": return this.name.Value;
     //9
                    case "level": return this.level.Value;
     //10
                }
                throw new Exception("错误");
            }
            set
            {
                switch (name)
                {
     //11
                    case "id":
                        this.id = (Int64)value;
                        break;
     //11
                    case "account":
                        CheckAccountValue((String)Convert.ChangeType(value, typeof(String)), -1);
                        break;
     //11
                    case "password":
                        CheckPasswordValue((String)Convert.ChangeType(value, typeof(String)), -1);
                        break;
     //11
                    case "name":
                        CheckNameValue((String)Convert.ChangeType(value, typeof(String)), -1);
                        break;
     //11
                    case "level":
                        CheckLevelValue((Int32)Convert.ChangeType(value, typeof(Int32)), -1);
                        break;
     //12
                }
            }
        }

    #if SERVER
        public void Delete(bool immediately = false)
        {
            if (immediately)
            {
                var sb = new StringBuilder();
                BulkDeleteBuilder(sb, false);
                Context.ExecuteNonQuery(sb.ToString());
            }
            else
            {
                RowState = DataRowState.Detached;
                Context.Update(this);
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
        public static UserData Query(string filterExpression)
        {
            var cmdText = $"select * from `user` where {filterExpression}; ";
            var data = DistributedDB.I.ExecuteQuery<UserData>(cmdText);
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
        public static async UniTask<UserData> QueryAsync(string filterExpression)
        {
            var cmdText = $"select * from `user` where {filterExpression}; ";
            var data = await DistributedDB.I.ExecuteQueryAsync<UserData>(cmdText);
            return data;
        }
        public static UserData[] QueryList(string filterExpression)
        {
            var cmdText = $"select * from `user` where {filterExpression}; ";
            var datas = DistributedDB.I.ExecuteQueryList<UserData>(cmdText);
            return datas;
        }
        public static async UniTask<UserData[]> QueryListAsync(string filterExpression)
        {
            var cmdText = $"select * from `user` where {filterExpression}; ";
            var datas = await DistributedDB.I.ExecuteQueryListAsync<UserData>(cmdText);
            return datas;
        }
        public void Update(bool immediately = false)
        {
            if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached | RowState == DataRowState.Added | RowState == 0) return;
            if (immediately)
            {
                var sb = new StringBuilder();
                BulkLoaderBuilder(sb, false);
                Context.ExecuteNonQuery(sb.ToString());
                RowState = DataRowState.Unchanged;
            }
            else
            {
                RowState = DataRowState.Modified;
                Context.Update(this);
            }
        }
    #endif

        public void Init(DataRow row)
        {
            RowState = DataRowState.Unchanged;
     //13
            if (row[0] is Int64 id)
                this.id = id;
     //13
            if (row[1] is String account)
                CheckAccountValue(account, -1);
     //13
            if (row[2] is String password)
                CheckPasswordValue(password, -1);
     //13
            if (row[3] is String name)
                CheckNameValue(name, -1);
     //13
            if (row[4] is Int32 level)
                CheckLevelValue(level, -1);
     //14
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
            BulkDeleteBuilder(sb, true);
            RowState = DataRowState.Deleted;
    #endif
        }

    #if SERVER
        public void BulkDeleteBuilder(StringBuilder sb, bool isBulk = true)
        {
            if (!isBulk)
                sb.Append($"DELETE FROM `user` {CheckSqlKey(0, id)}");
            else
                sb.Append($"{(sb.Length == 0 ? "" : ", ")}{id}");
        }

        public void BulkLoaderBuilder(StringBuilder sb, bool isBulk = true)
        {
 //15
            if (!isBulk)
                sb.Append("REPLACE INTO `user` VALUES (");
            for (int i = 0; i < 5; i++)
            {
                var name = GetCellNameAndTextLength(i, out var length);
                var value = this[i];
                if (value == null) //空的值会导致sql语句错误
                {
                    if (isBulk) sb.Append(@"\N|");
                    else sb.Append($"NULL,");
                    continue;
                }
                if (value is string text)
                {
                    Context.CheckStringValue(ref text, length);
                    if (isBulk) sb.Append(text + "|");
                    else sb.Append($"'{text}',");
                }
                else if (value is DateTime dateTime)
                {
                    if (isBulk) sb.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss") + "|");
                    else sb.Append($"'{dateTime.ToString("yyyy-MM-dd HH:mm:ss")}',");
                }
                else if (value is bool boolVal)
                {
                    if (isBulk) sb.Append(boolVal ? "1|" : "0|");
                    else sb.Append($"{(boolVal ? "1" : "0")},");
                }
                else if (value is byte[] buffer)
                {
                    var base64Str = Convert.ToBase64String(buffer, Base64FormattingOptions.None);
                    if (buffer.Length >= length)
                    {
                        NDebug.LogError($"user表{name}列长度溢出!");
                        if (isBulk) sb.Append(@"\N|");
                        else sb.Append($"NULL,");
                        continue;
                    }
                    if (isBulk) sb.Append(base64Str + "|");
                    else sb.Append($"'{base64Str}',");
                }
                else 
                {
                    if (isBulk) sb.Append(value + "|");
                    else sb.Append($"{value},");
                }
            }
            if (isBulk)
                sb.AppendLine();
            else
            {
                sb[sb.Length - 1] = ' ';
                sb.Append(");");
            }
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
                Context.CheckStringValue(ref text, length);
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
            if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached | id <= 0) return; //如果id=-1或者0表示这个对象还没有完成赋值
            if (RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                RowState = DataRowState.Modified;
            Context.Update(this);
#endif
        }

        public override int GetHashCode()
        {
            return (int)id; //解决hash碰撞不判断对象是否同一地址问题
        }

        public override string ToString()
        {
            return $"Id:{Id} Account:{Account} Password:{Password} Name:{Name} Level:{Level} ";
        }
    }
}