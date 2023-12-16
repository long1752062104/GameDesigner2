using System.Data;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Net.Config;
using Net.Share;

namespace Framework
{
    public class TableManager : MonoBehaviour
    {
        protected readonly TableConfig tableConfig = new TableConfig();

        public virtual async UniTask Init()
        {
            var path = GlobalSetting.Instance.tablePath + "/GameConfig.bytes";
            using (var request = UnityWebRequest.Get("file:///" + path)) 
            {
                var oper = request.SendWebRequest();
                while (!oper.isDone)
                {
                    await UniTask.Yield();
                }
                if (!string.IsNullOrEmpty(request.error))
                {
                    Global.Logger.LogError("点击菜单GameDesigner/Framework/GenerateExcelData生成execl表数据! " + request.error);
                    return;
                }
                var jsonStr = request.downloadHandler.text;
                tableConfig.LoadTable(jsonStr);
            }
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
    }
}