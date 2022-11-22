using System;
using System.Data;
using System.Threading.Tasks;
using Net.System;
using System.Collections.Generic;
using System.Text;
#if SERVER
using System.Data.SQLite;
#endif

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
    public partial class UserinfoData : IDataRow
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

    
        private String account;
        /// <summary>{NOTE}</summary>
        public String Account
        {
            get { return account; }
            set
            {
                if (this.account == value)
                    return;
                if(value==null) value = string.Empty;
                this.account = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(1);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                AccountCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncAccount
        {
            get { return account; }
            set
            {
                if (this.account == value)
                    return;
                if(value==null) value = string.Empty;
                this.account = value;
                AccountCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncIDAccount
        {
            get { return account; }
            set
            {
                if (this.account == value)
                    return;
                if(value==null) value = string.Empty;
                this.account = value;
                SyncAccountCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void AccountCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.ACCOUNT, (System.String)account);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncAccountCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.ACCOUNT, (System.Int64)id, (System.String)account);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.ACCOUNT)]
        private void AccountCall(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Account = value;
            OnAccount?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnAccount;
    
        private String password;
        /// <summary>{NOTE}</summary>
        public String Password
        {
            get { return password; }
            set
            {
                if (this.password == value)
                    return;
                if(value==null) value = string.Empty;
                this.password = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(2);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                PasswordCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncPassword
        {
            get { return password; }
            set
            {
                if (this.password == value)
                    return;
                if(value==null) value = string.Empty;
                this.password = value;
                PasswordCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncIDPassword
        {
            get { return password; }
            set
            {
                if (this.password == value)
                    return;
                if(value==null) value = string.Empty;
                this.password = value;
                SyncPasswordCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void PasswordCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.PASSWORD, (System.String)password);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncPasswordCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.PASSWORD, (System.Int64)id, (System.String)password);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.PASSWORD)]
        private void PasswordCall(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Password = value;
            OnPassword?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnPassword;
    
        private Double moveSpeed;
        /// <summary>{NOTE}</summary>
        public Double MoveSpeed
        {
            get { return moveSpeed; }
            set
            {
                if (this.moveSpeed == value)
                    return;
                
                this.moveSpeed = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(3);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                MoveSpeedCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Double SyncMoveSpeed
        {
            get { return moveSpeed; }
            set
            {
                if (this.moveSpeed == value)
                    return;
                
                this.moveSpeed = value;
                MoveSpeedCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Double SyncIDMoveSpeed
        {
            get { return moveSpeed; }
            set
            {
                if (this.moveSpeed == value)
                    return;
                
                this.moveSpeed = value;
                SyncMoveSpeedCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void MoveSpeedCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.MOVESPEED, (System.Double)moveSpeed);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncMoveSpeedCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.MOVESPEED, (System.Int64)id, (System.Double)moveSpeed);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.MOVESPEED)]
        private void MoveSpeedCall(Double value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            MoveSpeed = value;
            OnMoveSpeed?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnMoveSpeed;
    
        private String position;
        /// <summary>{NOTE}</summary>
        public String Position
        {
            get { return position; }
            set
            {
                if (this.position == value)
                    return;
                if(value==null) value = string.Empty;
                this.position = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(4);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                PositionCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncPosition
        {
            get { return position; }
            set
            {
                if (this.position == value)
                    return;
                if(value==null) value = string.Empty;
                this.position = value;
                PositionCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncIDPosition
        {
            get { return position; }
            set
            {
                if (this.position == value)
                    return;
                if(value==null) value = string.Empty;
                this.position = value;
                SyncPositionCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void PositionCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.POSITION, (System.String)position);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncPositionCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.POSITION, (System.Int64)id, (System.String)position);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.POSITION)]
        private void PositionCall(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Position = value;
            OnPosition?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnPosition;
    
        private String rotation;
        /// <summary>{NOTE}</summary>
        public String Rotation
        {
            get { return rotation; }
            set
            {
                if (this.rotation == value)
                    return;
                if(value==null) value = string.Empty;
                this.rotation = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(5);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                RotationCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncRotation
        {
            get { return rotation; }
            set
            {
                if (this.rotation == value)
                    return;
                if(value==null) value = string.Empty;
                this.rotation = value;
                RotationCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public String SyncIDRotation
        {
            get { return rotation; }
            set
            {
                if (this.rotation == value)
                    return;
                if(value==null) value = string.Empty;
                this.rotation = value;
                SyncRotationCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void RotationCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.ROTATION, (System.String)rotation);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncRotationCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.ROTATION, (System.Int64)id, (System.String)rotation);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.ROTATION)]
        private void RotationCall(String value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Rotation = value;
            OnRotation?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnRotation;
    
        private Int64 health;
        /// <summary>{NOTE}</summary>
        public Int64 Health
        {
            get { return health; }
            set
            {
                if (this.health == value)
                    return;
                
                this.health = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(6);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                HealthCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Int64 SyncHealth
        {
            get { return health; }
            set
            {
                if (this.health == value)
                    return;
                
                this.health = value;
                HealthCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Int64 SyncIDHealth
        {
            get { return health; }
            set
            {
                if (this.health == value)
                    return;
                
                this.health = value;
                SyncHealthCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void HealthCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.HEALTH, (System.Int64)health);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncHealthCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.HEALTH, (System.Int64)id, (System.Int64)health);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.HEALTH)]
        private void HealthCall(Int64 value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            Health = value;
            OnHealth?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnHealth;
    
        private Int64 healthMax;
        /// <summary>{NOTE}</summary>
        public Int64 HealthMax
        {
            get { return healthMax; }
            set
            {
                if (this.healthMax == value)
                    return;
                
                this.healthMax = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(7);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                HealthMaxCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Int64 SyncHealthMax
        {
            get { return healthMax; }
            set
            {
                if (this.healthMax == value)
                    return;
                
                this.healthMax = value;
                HealthMaxCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Int64 SyncIDHealthMax
        {
            get { return healthMax; }
            set
            {
                if (this.healthMax == value)
                    return;
                
                this.healthMax = value;
                SyncHealthMaxCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void HealthMaxCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.HEALTHMAX, (System.Int64)healthMax);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncHealthMaxCall()
        {
            
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.HEALTHMAX, (System.Int64)id, (System.Int64)healthMax);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.HEALTHMAX)]
        private void HealthMaxCall(Int64 value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            HealthMax = value;
            OnHealthMax?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnHealthMax;
    
        private System.Byte[] buffer;
        /// <summary>{NOTE}</summary>
        internal System.Byte[] BufferBytes
        {
            get { return buffer; }
            set
            {
                if (this.buffer == value)
                    return;
                if(value==null) value = new byte[0];
                this.buffer = value;
    #if SERVER
                if (RowState == DataRowState.Deleted | RowState == DataRowState.Detached) return;
                columns.Add(8);
                if(RowState != DataRowState.Added & RowState != 0)//如果还没初始化或者创建新行,只能修改值不能更新状态
                    RowState = DataRowState.Modified;
                Example2DB.I.Update(this);
    #elif CLIENT
                BufferBytesCall();
    #endif
            }
        }

        /// <summary>{NOTE1}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public System.Byte[] SyncBufferBytes
        {
            get { return buffer; }
            set
            {
                if (this.buffer == value)
                    return;
                if(value==null) value = new byte[0];
                this.buffer = value;
                BufferBytesCall();
            }
        }

        /// <summary>{NOTE2}</summary>
        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public System.Byte[] SyncIDBufferBytes
        {
            get { return buffer; }
            set
            {
                if (this.buffer == value)
                    return;
                if(value==null) value = new byte[0];
                this.buffer = value;
                SyncBufferBytesCall();
            }
        }

        /// <summary>{NOTE3}</summary>
        public void BufferBytesCall()
        {
            object bytes = buffer;
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.BUFFER, (System.Byte[])bytes);
        }

	    /// <summary>{NOTE4}</summary>
        public void SyncBufferBytesCall()
        {
            object bytes = buffer;
            Net.Client.ClientBase.Instance.SendRT(Net.Share.NetCmd.EntityRpc, (ushort)Example2HashProto.BUFFER, (System.Int64)id, (System.Byte[])bytes);
        }

        [Net.Share.Rpc(hash = (ushort)Example2HashProto.BUFFER)]
        private void BufferBytesCall(System.Byte[] value)//重写NetPlayer的OnStart方法来处理客户端自动同步到服务器数据库, 方法内部添加AddRpc(data(UserinfoData));收集Rpc
        {
            BufferBytes = value;
            OnBufferBytes?.Invoke();
        }

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action OnBufferBytes;
    

        public UserinfoData() { }

    #if SERVER
        public UserinfoData(params object[] parms)
        {
            NewTableRow(parms);
        }
        public void NewTableRow()
        {
            for (int i = 0; i < 9; i++)
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
                    return "account";
    
                case 2:
                    return "password";
    
                case 3:
                    return "moveSpeed";
    
                case 4:
                    return "position";
    
                case 5:
                    return "rotation";
    
                case 6:
                    return "health";
    
                case 7:
                    return "healthMax";
    
                case 8:
                    return "buffer";
    
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
                        return this.account;
    
                    case 2:
                        return this.password;
    
                    case 3:
                        return this.moveSpeed;
    
                    case 4:
                        return this.position;
    
                    case 5:
                        return this.rotation;
    
                    case 6:
                        return this.health;
    
                    case 7:
                        return this.healthMax;
    
                    case 8:
                        return this.buffer;
    
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
                        this.account = (String)value;
                        break;
    
                    case 2:
                        this.password = (String)value;
                        break;
    
                    case 3:
                        this.moveSpeed = (Double)value;
                        break;
    
                    case 4:
                        this.position = (String)value;
                        break;
    
                    case 5:
                        this.rotation = (String)value;
                        break;
    
                    case 6:
                        this.health = (Int64)value;
                        break;
    
                    case 7:
                        this.healthMax = (Int64)value;
                        break;
    
                    case 8:
                        this.buffer = (System.Byte[])value;
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
        public static UserinfoData Query(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var data = Example2DB.ExecuteQuery<UserinfoData>(cmdText);
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
        public static async Task<UserinfoData> QueryAsync(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var data = await Example2DB.ExecuteQueryAsync<UserinfoData>(cmdText);
            return data;
        }
        public static UserinfoData[] QueryList(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var datas = Example2DB.ExecuteQueryList<UserinfoData>(cmdText);
            return datas;
        }
        public static async Task<UserinfoData[]> QueryListAsync(string filterExpression)
        {
            var cmdText = $"select * from userinfo where {filterExpression}; ";
            var datas = await Example2DB.ExecuteQueryListAsync<UserinfoData>(cmdText);
            return datas;
        }
        public void Update() => Update(0, 9);
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
    
            if (row[1] is String account)
                this.account = account;
    
            if (row[2] is String password)
                this.password = password;
    
            if (row[3] is Double moveSpeed)
                this.moveSpeed = moveSpeed;
    
            if (row[4] is String position)
                this.position = position;
    
            if (row[5] is String rotation)
                this.rotation = rotation;
    
            if (row[6] is Int64 health)
                this.health = health;
    
            if (row[7] is Int64 healthMax)
                this.healthMax = healthMax;
    
            if (row[8] is System.Byte[] buffer)
                this.buffer = buffer;
    
        }

        public void AddedSql(StringBuilder sb, List<IDbDataParameter> parms, ref int parmsLen)
        {
    #if SERVER
            if(columns.Count == 0)//如果没有修改一个字段是不允许提交的
                return;
    
            string cmdText = "REPLACE INTO userinfo(";
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
            string cmdText = $"UPDATE userinfo SET ";
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
            string cmdText = $"DELETE FROM userinfo WHERE `id` = {id}; ";
            sb.Append(cmdText);
            RowState = DataRowState.Deleted;
    #endif
        }
    }
