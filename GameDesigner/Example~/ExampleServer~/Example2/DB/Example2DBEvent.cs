namespace Example2
{
    public class Example2DBEvent
    {
        private static Net.Client.ClientBase client;
        /// <summary>
        /// 设置同步到服务器的客户端对象, 如果不设置, 则默认是ClientBase.Instance对象
        /// </summary>
        public static Net.Client.ClientBase Client
        {
            get
            {
                if (client == null)
                    client = Net.Client.ClientBase.Instance;
                return client;
            }
            set => client = value;
        }

        /// <summary>
        /// 当服务器属性同步给客户端, 如果需要同步属性到客户端, 需要监听此事件, 并且调用发送给客户端
        /// 参数1: 要发送给哪个客户端
        /// 参数2: cmd
        /// 参数3: protocol
        /// 参数4: pars
        /// </summary>
        public static System.Action<Net.Server.NetPlayer, byte, uint, object[]> OnSyncProperty;

        /// <summary>
        /// 当实体行属性更改时触发
        /// 参数1: 实体行对象接口
        /// 参数1: 哪个属性被修改
        /// 参数2: 数据的唯一id
        /// 参数3: 数据的更改值
        /// </summary>
        public static System.Action<Net.Share.IDataRow, Example2HashProto, object, object> OnValueChanged;

        /// <summary>ConfigData类对象属性同步id索引</summary>
		public static int ConfigData_SyncID = 0;
		/// <summary>UserinfoData类对象属性同步id索引</summary>
		public static int UserinfoData_SyncID = 0;
		
    }
}