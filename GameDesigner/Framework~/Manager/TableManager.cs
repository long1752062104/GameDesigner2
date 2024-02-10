using System.Data;
using UnityEngine;
using Net.Config;
using Net.Share;

namespace Framework
{
    public class TableManager : MonoBehaviour
    {
        public string tablePath = "Assets/Arts/Table/GameConfig.bytes";
        protected TableConfig tableConfig = new TableConfig();

        public virtual void Init()
        {
            var textAsset = Global.Resources.LoadAsset<TextAsset>(tablePath);
            if (textAsset == null)
            {
                Debug.LogError("获取游戏配置失败!");
                return;
            }
            var jsonStr = textAsset.text;
            tableConfig.LoadTable(jsonStr);
        }

        /// <summary>
        /// 获取某个表
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public DataTable GetTable(string sheetName)
        {
            return tableConfig.GetTable(sheetName);
        }

        /// <summary>
        /// 获取excel表格数据，filterExpression参数例子: "Name = 'UI_Message'"
        /// </summary>
        /// <typeparam name="T">要获取的类型</typeparam>
        /// <param name="filterExpression">过滤表达式</param>
        /// <returns></returns>
        public T GetDataConfig<T>(string filterExpression) where T : IDataConfig, new()
        {
            return tableConfig.GetDataConfig<T>(filterExpression);
        }

        /// <summary>
        /// 获取excel表格数据，filterExpression参数例子: "Name = 'UI_Message'"
        /// </summary>
        /// <typeparam name="T">要获取的类型</typeparam>
        /// <param name="filterExpression">过滤表达式</param>
        /// <returns></returns>
        public T[] GetDataConfigs<T>(string filterExpression) where T : IDataConfig, new()
        {
            return tableConfig.GetDataConfigs<T>(filterExpression);
        }

        /// <summary>
        /// 转移对象信息, 保留对象数据, 可能旧的TableManager被销毁, 转移数据给新的TableManager组件
        /// </summary>
        /// <param name="tableManager"></param>
        public void Change(TableManager tableManager)
        {
            tableConfig = tableManager.tableConfig;
            tableManager.tableConfig = null;
        }
    }
}