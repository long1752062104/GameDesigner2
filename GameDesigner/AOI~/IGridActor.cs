namespace Net.AOI
{
    /// <summary>
    /// 格子演员接口, 怪物, 角色都属于演员
    /// </summary>
    public interface IGridActor
    {
        /// <summary>
        /// 演员ID 可用于实例化客户端哪个id的预制体
        /// </summary>
        public int ActorID { get; set; }
        /// <summary>
        /// 头发
        /// </summary>
        public int Hair { get; set; }
        /// <summary>
        /// 头部
        /// </summary>
        public int Head { get; set; }
        /// <summary>
        /// 上衣
        /// </summary>
        public int Jacket { get; set; }
        /// <summary>
        /// 腰带
        /// </summary>
        public int Belt { get; set; }
        /// <summary>
        /// 裤子
        /// </summary>
        public int Pants { get; set; }
        /// <summary>
        /// 鞋子
        /// </summary>
        public int Shoe { get; set; }
        /// <summary>
        /// 武器
        /// </summary>
        public int Weapon { get; set;}
    }
}
