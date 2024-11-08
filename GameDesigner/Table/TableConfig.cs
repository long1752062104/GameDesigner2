using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using Net.Event;
using Net.Share;

namespace Net.Table
{
    /// <summary>
    /// 表配置类, 可双端使用
    /// </summary>
    public class TableConfig
    {
        private DataSet dataSet;
        private readonly Dictionary<Type, Dictionary<string, object>> tableDict = new(); //表缓存字典

        /// <summary>
        /// 加载表文件
        /// </summary>
        /// <param name="path"></param>
        public void LoadTableFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            LoadTable(bytes);
        }

        /// <summary>
        /// 加载表数据
        /// </summary>
        /// <param name="jsonStr"></param>
        public void LoadTable(string jsonStr)
        {
            dataSet = Newtonsoft_X.Json.JsonConvert.DeserializeObject<DataSet>(jsonStr);
            TypeSolver.InitTypeSolverCollectors();
        }

        /// <summary>
        /// 加载二进制表数据
        /// </summary>
        /// <param name="bytes"></param>
        public void LoadTable(byte[] bytes)
        {
            dataSet = new DataSet();
            var dataSetInfo = TableBinarySerialize.Deserialize(bytes);
            for (int i = 0; i < dataSetInfo.Tables.Count; i++)
            {
                var tableInfo = dataSetInfo.Tables[i];
                var dataTable = new DataTable(tableInfo.TableName);
                for (int x = 0; x < tableInfo.Columns.Count; x++)
                {
                    var colInfo = tableInfo.Columns[x];
                    dataTable.Columns.Add(new DataColumn(colInfo.ColumnName, colInfo.DataType));
                }
                for (int x = 0; x < tableInfo.Rows.Count; x++)
                {
                    var rowInfo = tableInfo.Rows[x];
                    var dataRow = dataTable.NewRow();
                    dataTable.Rows.Add(dataRow);
                    for (int y = 0; y < tableInfo.Columns.Count; y++)
                        dataRow[y] = rowInfo[y];
                }
                dataTable.AcceptChanges();
                dataSet.Tables.Add(dataTable);
            }
            dataSet.AcceptChanges();
            TypeSolver.InitTypeSolverCollectors();
        }

        /// <summary>
        /// 获取某个表
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
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
            if (datas.Length == 0)
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
                if (!tableDict.TryGetValue(type, out var dict))
                    tableDict.Add(type, dict = new Dictionary<string, object>());
                if (dict.TryGetValue(filterExpression, out var datas))
                    return datas as T[];
                var sheetName = type.Name.Replace("DataConfig", string.Empty);
                var table = GetTable(sheetName);
                var items = new List<T>();
                if (string.IsNullOrEmpty(filterExpression))
                {
                    var rows = table.Rows;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var obj = rows[i]["ID"];
                        if (obj == null)
                            continue;
                        var str = obj.ToString();
                        if (string.IsNullOrEmpty(str))
                            continue;
                        var t = new T();
                        t.Init(rows[i]);
                        items.Add(t);
                    }
                }
                else
                {
                    var rows = table.Select(filterExpression);
                    for (int i = 0; i < rows.Length; i++)
                    {
                        var t = new T();
                        t.Init(rows[i]);
                        items.Add(t);
                    }
                }
                foreach (T[] items1 in dict.Values)
                    for (int i = 0; i < items1.Length; i++)
                        for (int x = 0; x < items.Count; x++)
                            if (items[x].ID == items1[i].ID) //相同的行, 不同的查询语句必须保证只需要一个对象
                                items[x] = items1[i];
                datas = items.ToArray();
                dict.Add(filterExpression, datas);
                return datas as T[];
            }
            catch (Exception ex)
            {
                NDebug.LogError("获取Excel表数据异常: " + ex);
            }
            return null;
        }
    }
}