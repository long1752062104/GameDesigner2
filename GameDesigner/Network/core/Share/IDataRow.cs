using System.Data;
using System.Text;

namespace Net.Share
{
    /// <summary>
    /// 数据行接口
    /// </summary>
    public interface IDataRow
    {
        /// <summary>
        /// 当前数据行状态
        /// </summary>
        DataRowState RowState { get; set; }
        /// <summary>
        /// 初始化数据行
        /// </summary>
        /// <param name="row"></param>
        void Init(DataRow row);
        /// <summary>
        /// sql的插入语句处理
        /// </summary>
        /// <param name="sb"></param>
        void AddedSql(StringBuilder sb);
        /// <summary>
        /// sql的修改语句处理
        /// </summary>
        /// <param name="sb"></param>
        void ModifiedSql(StringBuilder sb);
        /// <summary>
        /// sql的删除语句处理
        /// </summary>
        /// <param name="sb"></param>
        void DeletedSql(StringBuilder sb);
        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object this[string name] { get; set; }
        /// <summary>
        /// 根据索引获得字段值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object this[int index] { get; set; }
        /// <summary>
        /// 设置数据库实例
        /// </summary>
        /// <param name="context"></param>
        void SetContext(object context);
    }
}