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
                    Global.Logger.LogError("����˵�GameDesigner/Framework/GenerateExcelData����execl������! " + request.error);
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
        /// ��ȡexcel������ݣ�filterExpression��������: "Name = 'UI_Message'"
        /// </summary>
        /// <typeparam name="T">Ҫ��ȡ������</typeparam>
        /// <param name="filterExpression">���˱��ʽ</param>
        /// <returns></returns>
        public T GetDataConfig<T>(string filterExpression) where T : IDataConfig, new()
        {
            var datas = GetDataConfigs<T>(filterExpression);
            if (datas == null)
                return default;
            return datas[0];
        }

        /// <summary>
        /// ��ȡexcel������ݣ�filterExpression��������: "Name = 'UI_Message'"
        /// </summary>
        /// <typeparam name="T">Ҫ��ȡ������</typeparam>
        /// <param name="filterExpression">���˱��ʽ</param>
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
                Global.Logger.LogError("��ȡExcel�������쳣: " + ex);
            }
            return null;
        }
    }
}