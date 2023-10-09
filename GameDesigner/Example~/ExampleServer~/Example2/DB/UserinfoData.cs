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
    public partial class UserinfoData : IDataRow
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
        /// <summary>id</summary>
        public Int64 Id { get { return id; } set { this.id = value; } }

     //1
        private readonly StringObs account = new StringObs("UserinfoData_account", false, null);

        /// <summary>account --获得属性观察对象</summary>
        internal StringObs AccountObserver => account;

        /// <summary>account</summary>
        public String Account { get => GetAccountValue(); set => CheckAccountValue(value, 0); }

        /// <summary>account --同步到数据库</summary>
        internal String SyncAccount { get => GetAccountValue(); set => CheckAccountValue(value, 1); }

        /// <summary>account --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
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
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_ACCOUNT, value);
        }

        /// <summary>account --同步当前值到服务器Player对象上，需要处理</summary>
        public void AccountCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], account.Value };
            else objects = new object[] { account.Value };
#if SERVER
            CheckUpdate(1);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_ACCOUNT, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_ACCOUNT, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_ACCOUNT)]
        private void AccountRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Account = value;
        }
     //1
        private readonly StringObs password = new StringObs("UserinfoData_password", false, null);

        /// <summary>password --获得属性观察对象</summary>
        internal StringObs PasswordObserver => password;

        /// <summary>password</summary>
        public String Password { get => GetPasswordValue(); set => CheckPasswordValue(value, 0); }

        /// <summary>password --同步到数据库</summary>
        internal String SyncPassword { get => GetPasswordValue(); set => CheckPasswordValue(value, 1); }

        /// <summary>password --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
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
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_PASSWORD, value);
        }

        /// <summary>password --同步当前值到服务器Player对象上，需要处理</summary>
        public void PasswordCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], password.Value };
            else objects = new object[] { password.Value };
#if SERVER
            CheckUpdate(2);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_PASSWORD, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_PASSWORD, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_PASSWORD)]
        private void PasswordRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Password = value;
        }
     //1
        private readonly DoubleObs moveSpeed = new DoubleObs("UserinfoData_moveSpeed", true, null);

        /// <summary>moveSpeed --获得属性观察对象</summary>
        internal DoubleObs MoveSpeedObserver => moveSpeed;

        /// <summary>moveSpeed</summary>
        public Double MoveSpeed { get => GetMoveSpeedValue(); set => CheckMoveSpeedValue(value, 0); }

        /// <summary>moveSpeed --同步到数据库</summary>
        internal Double SyncMoveSpeed { get => GetMoveSpeedValue(); set => CheckMoveSpeedValue(value, 1); }

        /// <summary>moveSpeed --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal Double SyncIDMoveSpeed { get => GetMoveSpeedValue(); set => CheckMoveSpeedValue(value, 2); }

        private Double GetMoveSpeedValue() => this.moveSpeed.Value;

        private void CheckMoveSpeedValue(Double value, int type) 
        {
            if (this.moveSpeed.Value == value)
                return;
            this.moveSpeed.Value = value;
            if (type == 0)
                CheckUpdate(3);
            else if (type == 1)
                MoveSpeedCall(false);
            else if (type == 2)
                MoveSpeedCall(true);
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_MOVESPEED, value);
        }

        /// <summary>moveSpeed --同步当前值到服务器Player对象上，需要处理</summary>
        public void MoveSpeedCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], moveSpeed.Value };
            else objects = new object[] { moveSpeed.Value };
#if SERVER
            CheckUpdate(3);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_MOVESPEED, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_MOVESPEED, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_MOVESPEED)]
        private void MoveSpeedRpc(Double value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            MoveSpeed = value;
        }
     //1
        private readonly StringObs position = new StringObs("UserinfoData_position", false, null);

        /// <summary>position --获得属性观察对象</summary>
        internal StringObs PositionObserver => position;

        /// <summary>position</summary>
        public String Position { get => GetPositionValue(); set => CheckPositionValue(value, 0); }

        /// <summary>position --同步到数据库</summary>
        internal String SyncPosition { get => GetPositionValue(); set => CheckPositionValue(value, 1); }

        /// <summary>position --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal String SyncIDPosition { get => GetPositionValue(); set => CheckPositionValue(value, 2); }

        private String GetPositionValue() => this.position.Value;

        private void CheckPositionValue(String value, int type) 
        {
            if (this.position.Value == value)
                return;
            this.position.Value = value;
            if (type == 0)
                CheckUpdate(4);
            else if (type == 1)
                PositionCall(false);
            else if (type == 2)
                PositionCall(true);
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_POSITION, value);
        }

        /// <summary>position --同步当前值到服务器Player对象上，需要处理</summary>
        public void PositionCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], position.Value };
            else objects = new object[] { position.Value };
#if SERVER
            CheckUpdate(4);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_POSITION, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_POSITION, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_POSITION)]
        private void PositionRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Position = value;
        }
     //1
        private readonly StringObs rotation = new StringObs("UserinfoData_rotation", false, null);

        /// <summary>rotation --获得属性观察对象</summary>
        internal StringObs RotationObserver => rotation;

        /// <summary>rotation</summary>
        public String Rotation { get => GetRotationValue(); set => CheckRotationValue(value, 0); }

        /// <summary>rotation --同步到数据库</summary>
        internal String SyncRotation { get => GetRotationValue(); set => CheckRotationValue(value, 1); }

        /// <summary>rotation --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal String SyncIDRotation { get => GetRotationValue(); set => CheckRotationValue(value, 2); }

        private String GetRotationValue() => this.rotation.Value;

        private void CheckRotationValue(String value, int type) 
        {
            if (this.rotation.Value == value)
                return;
            this.rotation.Value = value;
            if (type == 0)
                CheckUpdate(5);
            else if (type == 1)
                RotationCall(false);
            else if (type == 2)
                RotationCall(true);
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_ROTATION, value);
        }

        /// <summary>rotation --同步当前值到服务器Player对象上，需要处理</summary>
        public void RotationCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], rotation.Value };
            else objects = new object[] { rotation.Value };
#if SERVER
            CheckUpdate(5);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_ROTATION, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_ROTATION, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_ROTATION)]
        private void RotationRpc(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Rotation = value;
        }
     //1
        private readonly Int64Obs health = new Int64Obs("UserinfoData_health", true, null);

        /// <summary>health --获得属性观察对象</summary>
        internal Int64Obs HealthObserver => health;

        /// <summary>health</summary>
        public Int64 Health { get => GetHealthValue(); set => CheckHealthValue(value, 0); }

        /// <summary>health --同步到数据库</summary>
        internal Int64 SyncHealth { get => GetHealthValue(); set => CheckHealthValue(value, 1); }

        /// <summary>health --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal Int64 SyncIDHealth { get => GetHealthValue(); set => CheckHealthValue(value, 2); }

        private Int64 GetHealthValue() => this.health.Value;

        private void CheckHealthValue(Int64 value, int type) 
        {
            if (this.health.Value == value)
                return;
            this.health.Value = value;
            if (type == 0)
                CheckUpdate(6);
            else if (type == 1)
                HealthCall(false);
            else if (type == 2)
                HealthCall(true);
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_HEALTH, value);
        }

        /// <summary>health --同步当前值到服务器Player对象上，需要处理</summary>
        public void HealthCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], health.Value };
            else objects = new object[] { health.Value };
#if SERVER
            CheckUpdate(6);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_HEALTH, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_HEALTH, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_HEALTH)]
        private void HealthRpc(Int64 value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Health = value;
        }
     //1
        private readonly Int64Obs healthMax = new Int64Obs("UserinfoData_healthMax", true, null);

        /// <summary>healthMax --获得属性观察对象</summary>
        internal Int64Obs HealthMaxObserver => healthMax;

        /// <summary>healthMax</summary>
        public Int64 HealthMax { get => GetHealthMaxValue(); set => CheckHealthMaxValue(value, 0); }

        /// <summary>healthMax --同步到数据库</summary>
        internal Int64 SyncHealthMax { get => GetHealthMaxValue(); set => CheckHealthMaxValue(value, 1); }

        /// <summary>healthMax --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal Int64 SyncIDHealthMax { get => GetHealthMaxValue(); set => CheckHealthMaxValue(value, 2); }

        private Int64 GetHealthMaxValue() => this.healthMax.Value;

        private void CheckHealthMaxValue(Int64 value, int type) 
        {
            if (this.healthMax.Value == value)
                return;
            this.healthMax.Value = value;
            if (type == 0)
                CheckUpdate(7);
            else if (type == 1)
                HealthMaxCall(false);
            else if (type == 2)
                HealthMaxCall(true);
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_HEALTHMAX, value);
        }

        /// <summary>healthMax --同步当前值到服务器Player对象上，需要处理</summary>
        public void HealthMaxCall(bool syncId = false)
        {
            
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], healthMax.Value };
            else objects = new object[] { healthMax.Value };
#if SERVER
            CheckUpdate(7);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_HEALTHMAX, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_HEALTHMAX, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_HEALTHMAX)]
        private void HealthMaxRpc(Int64 value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            HealthMax = value;
        }
     //1
        private readonly BytesObs buffer = new BytesObs("UserinfoData_buffer", false, null);

        /// <summary>buffer --获得属性观察对象</summary>
        internal BytesObs BufferBytesObserver => buffer;

        /// <summary>buffer</summary>
        internal System.Byte[] BufferBytes { get => GetBufferBytesValue(); set => CheckBufferBytesValue(value, 0); }

        /// <summary>buffer --同步到数据库</summary>
        internal System.Byte[] SyncBufferBytes { get => GetBufferBytesValue(); set => CheckBufferBytesValue(value, 1); }

        /// <summary>buffer --同步带有Key字段的值到服务器Player对象上，需要处理</summary>
        internal System.Byte[] SyncIDBufferBytes { get => GetBufferBytesValue(); set => CheckBufferBytesValue(value, 2); }

        private System.Byte[] GetBufferBytesValue() => this.buffer.Value;

        private void CheckBufferBytesValue(System.Byte[] value, int type) 
        {
            
            this.buffer.Value = value;
            if (type == 0)
                CheckUpdate(8);
            else if (type == 1)
                BufferBytesCall(false);
            else if (type == 2)
                BufferBytesCall(true);
            OnValueChanged?.Invoke(Example2HashProto.USERINFO_BUFFER, value);
        }

        /// <summary>buffer --同步当前值到服务器Player对象上，需要处理</summary>
        public void BufferBytesCall(bool syncId = false)
        {
            object bytes = buffer.Value;
            object[] objects;
            if (syncId) objects = new object[] { this[Example2DBEvent.UserinfoData_SyncID], bytes };
            else objects = new object[] { bytes };
#if SERVER
            CheckUpdate(8);
            Example2DBEvent.OnSyncProperty?.Invoke(client, NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_BUFFER, objects);
#else
            Example2DBEvent.Client.SendRT(NetCmd.SyncPropertyData, (ushort)Example2HashProto.USERINFO_BUFFER, objects);
#endif
        }

        [Rpc(hash = (ushort)Example2HashProto.USERINFO_BUFFER)]
        private void BufferBytesRpc(System.Byte[] value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            BufferBytes = value;
        }
     //2

        public UserinfoData() { }

    #if SERVER
        public UserinfoData(params object[] parms) : base()
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
        public void NewTableRowSync()
        {
            var sb = new StringBuilder();
            BulkLoaderBuilder(sb, false);
            Example2DB.I.ExecuteNonQuery(sb.ToString());
            RowState = DataRowState.Unchanged;
        }
        public string GetCellNameAndTextLength(int index, out uint length)
        {
            switch (index)
            {
     //3
                case 0: length = 65535; return "id";
     //3
                case 1: length = 65535; return "account";
     //3
                case 2: length = 65535; return "password";
     //3
                case 3: length = 65535; return "moveSpeed";
     //3
                case 4: length = 65535; return "position";
     //3
                case 5: length = 65535; return "rotation";
     //3
                case 6: length = 65535; return "health";
     //3
                case 7: length = 65535; return "healthMax";
     //3
                case 8: length = 1024; return "buffer";
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
                    case 3: return this.moveSpeed.Value;
     //5
                    case 4: return this.position.Value;
     //5
                    case 5: return this.rotation.Value;
     //5
                    case 6: return this.health.Value;
     //5
                    case 7: return this.healthMax.Value;
     //5
                    case 8: return this.buffer.Value;
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
                        CheckAccountValue((String)value, -1);
                        break;
     //7
                    case 2:
                        CheckPasswordValue((String)value, -1);
                        break;
     //7
                    case 3:
                        CheckMoveSpeedValue((Double)value, -1);
                        break;
     //7
                    case 4:
                        CheckPositionValue((String)value, -1);
                        break;
     //7
                    case 5:
                        CheckRotationValue((String)value, -1);
                        break;
     //7
                    case 6:
                        CheckHealthValue((Int64)value, -1);
                        break;
     //7
                    case 7:
                        CheckHealthMaxValue((Int64)value, -1);
                        break;
     //7
                    case 8:
                        CheckBufferBytesValue((System.Byte[])value, -1);
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
                    case "moveSpeed": return this.moveSpeed.Value;
     //9
                    case "position": return this.position.Value;
     //9
                    case "rotation": return this.rotation.Value;
     //9
                    case "health": return this.health.Value;
     //9
                    case "healthMax": return this.healthMax.Value;
     //9
                    case "buffer": return this.buffer.Value;
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
                        CheckAccountValue((String)value, -1);
                        break;
     //11
                    case "password":
                        CheckPasswordValue((String)value, -1);
                        break;
     //11
                    case "moveSpeed":
                        CheckMoveSpeedValue((Double)value, -1);
                        break;
     //11
                    case "position":
                        CheckPositionValue((String)value, -1);
                        break;
     //11
                    case "rotation":
                        CheckRotationValue((String)value, -1);
                        break;
     //11
                    case "health":
                        CheckHealthValue((Int64)value, -1);
                        break;
     //11
                    case "healthMax":
                        CheckHealthMaxValue((Int64)value, -1);
                        break;
     //11
                    case "buffer":
                        CheckBufferBytesValue((System.Byte[])value, -1);
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
        public static UserinfoData Query(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var data = Example2DB.I.ExecuteQuery<UserinfoData>(cmdText);
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
        public static async UniTask<UserinfoData> QueryAsync(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var data = await Example2DB.I.ExecuteQueryAsync<UserinfoData>(cmdText);
            return data;
        }
        public static UserinfoData[] QueryList(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var datas = Example2DB.I.ExecuteQueryList<UserinfoData>(cmdText);
            return datas;
        }
        public static async UniTask<UserinfoData[]> QueryListAsync(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var datas = await Example2DB.I.ExecuteQueryListAsync<UserinfoData>(cmdText);
            return datas;
        }
        public void Update()
        {
            if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached | RowState == DataRowState.Added | RowState == 0) return;
            RowState = DataRowState.Modified;
            Example2DB.I.Update(this);
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
            if (row[3] is Double moveSpeed)
                CheckMoveSpeedValue(moveSpeed, -1);
     //13
            if (row[4] is String position)
                CheckPositionValue(position, -1);
     //13
            if (row[5] is String rotation)
                CheckRotationValue(rotation, -1);
     //13
            if (row[6] is Int64 health)
                CheckHealthValue(health, -1);
     //13
            if (row[7] is Int64 healthMax)
                CheckHealthMaxValue(healthMax, -1);
     //13
            if (row[8] is System.Byte[] buffer)
                CheckBufferBytesValue(Convert.FromBase64String(Encoding.ASCII.GetString(buffer)), -1);
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
            string cmdText = $"DELETE FROM userinfo {CheckSqlKey(0, id)}";
            sb.Append(cmdText);
            RowState = DataRowState.Deleted;
    #endif
        }

    #if SERVER
        public void BulkLoaderBuilder(StringBuilder sb, bool isBulk = false)
        {
 //15
            if (!isBulk)
                sb.Append("REPLACE INTO userinfo VALUES (");
            for (int i = 0; i < 9; i++)
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
                    Example2DB.I.CheckStringValue(ref text, length);
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
                        NDebug.LogError($"userinfo表{name}列长度溢出!");
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

        public override string ToString()
        {
            return $"Id:{Id} Account:{Account} Password:{Password} MoveSpeed:{MoveSpeed} Position:{Position} Rotation:{Rotation} Health:{Health} HealthMax:{HealthMax} Buffer:{BufferBytes} ";
        }
    }
}