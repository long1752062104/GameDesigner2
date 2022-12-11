using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

namespace Framework
{
    public partial class TableManager : MonoBehaviour
    {
        private DataSet dataSet;
        private readonly Dictionary<Type, Dictionary<string, IDataConfig[]>> directory = new Dictionary<Type, Dictionary<string, IDataConfig[]>>();

        internal void Init()
        {
            string path;
            if (Global.Resources.Mode == AssetBundleMode.LocalPath)
                path = Application.streamingAssetsPath + $"/AssetBundles/Table/GameConfig.json";
            else
                path = Application.persistentDataPath + $"/AssetBundles/Table/GameConfig.json";
            if (File.Exists(path)) 
            {
                var jsonStr = File.ReadAllText(path);
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
            return GetDataConfigs<T>(filterExpression)[0];
        }

        /// <summary>
        /// ��ȡexcel������ݣ�filterExpression��������: "Name = 'UI_Message'"
        /// </summary>
        /// <typeparam name="T">Ҫ��ȡ������</typeparam>
        /// <param name="filterExpression">���˱��ʽ</param>
        /// <returns></returns>
        public T[] GetDataConfigs<T>(string filterExpression) where T : IDataConfig, new()
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
    }
}