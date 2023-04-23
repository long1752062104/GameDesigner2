using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework
{
    public class TableManager : MonoBehaviour
    {
        private DataSet dataSet;
        private readonly Dictionary<Type, Dictionary<string, IDataConfig[]>> directory = new Dictionary<Type, Dictionary<string, IDataConfig[]>>();

        internal async UniTask Init()
        {
            string path;
            if (Global.I.Mode == AssetBundleMode.LocalPath)
                path = Directory.GetCurrentDirectory() + $"/AssetBundles/Table/GameConfig.json";
            else if (Global.I.Mode == AssetBundleMode.StreamingAssetsPath)
                path = Application.streamingAssetsPath + $"/AssetBundles/Table/GameConfig.json";
            else
                path = Application.persistentDataPath + $"/AssetBundles/Table/GameConfig.json";
            using (var request = UnityWebRequest.Get(path)) 
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
                dataSet = Newtonsoft_X.Json.JsonConvert.DeserializeObject<DataSet>(jsonStr);
                foreach (DataTable table in dataSet.Tables)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        table.Columns[i].ColumnName = table.Rows[0][i].ToString();
                    }
                }
            }
        }

        public DataTable GetTable(string sheetName)
        {
            return dataSet.Tables[sheetName];
        }

        /// <summary>
        /// 获取excel表格数据，filterExpression参数例子: "Name = 'UI_Message'"
        /// </summary>
        /// <typeparam name="T">要获取的类型</typeparam>
        /// <param name="filterExpression">过滤表达式</param>
        /// <returns></returns>
        public T GetDataConfig<T>(string filterExpression) where T : IDataConfig, new()
        {
            var datas = GetDataConfigs<T>(filterExpression);
            if (datas == null)
                return default;
            return datas[0];
        }

        /// <summary>
        /// 获取excel表格数据，filterExpression参数例子: "Name = 'UI_Message'"
        /// </summary>
        /// <typeparam name="T">要获取的类型</typeparam>
        /// <param name="filterExpression">过滤表达式</param>
        /// <returns></returns>
        public T[] GetDataConfigs<T>(string filterExpression) where T : IDataConfig, new()
        {
            try 
            {
                var type = typeof(T);
                if (!directory.TryGetValue(type, out var dict))
                    directory.Add(type, dict = new Dictionary<string, IDataConfig[]>());
                if (dict.TryGetValue(filterExpression, out var datas))
                    return datas as T[];
                var sheetName = type.Name.Replace("DataConfig", "");
                var table = GetTable(sheetName);
                var rows = table.Select(filterExpression);
                var items = new T[rows.Length];
                for (int i = 0; i < rows.Length; i++)
                {
                    var t = new T();
                    t.Init(rows[i]);
                    items[i] = t;
                }
                dict.Add(filterExpression, items as IDataConfig[]);
                return items;
            }
            catch (Exception ex) 
            {
                Global.Logger.LogError("获取Excel表数据异常: " + ex);
            }
            return null;
        }
    }
}