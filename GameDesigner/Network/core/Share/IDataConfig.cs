using System.Data;

namespace Net.Share
{
    /// <summary>
    /// 游戏数据配置接口
    /// </summary>
    public interface IDataConfig
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// 初始化表格行数据转实体对象
        /// </summary>
        /// <param name="row"></param>
        void Init(DataRow row);
    }

    /// <summary>
    /// 游戏实体数据配置接口
    /// </summary>
    public interface IEntityDataConfig : IDataConfig
    {
        /// <summary>
        /// 获取配置表列的索引值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object this[int index] { get; set; }

        /// <summary>
        /// 获取配置表列的值
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        object this[string columnName] { get; set; }
    }
}