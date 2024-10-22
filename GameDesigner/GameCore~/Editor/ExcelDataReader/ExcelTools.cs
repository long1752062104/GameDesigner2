using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Net.Helper;
using ExcelDataReader;
using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;
using UnityEditorInternal;

namespace GameCore
{
    [InitializeOnLoad]
    public class ExcelTools
    {
        private static string[] m_ExcelName;
        private static string[] m_ExcelPath;
        private static int excelSelected = 0;

        public static void GenerateExcelData()
        {
            if (AssetBundleBuilder.Instance == null)
            {
                Debug.LogError("请创建打包资源文件，在Project界面的Assets右键菜单GameCore/Create AssetBundleBuilder");
                return;
            }
            var tablePath = AssetBundleBuilder.Instance.tablePath;
            var path = tablePath + "/GameConfig.bytes";
            if (!Directory.Exists(tablePath))
                Directory.CreateDirectory(tablePath);
            var dataSetNew = new DataSet();
            foreach (var file in Directory.GetFiles("Tools/Excel/", "*.*"))
            {
                if (!file.EndsWith(".xls") && !file.EndsWith(".xlsx"))
                    continue;
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("~$"))
                    continue;
                var temp = file + ".temp";
                File.Copy(file, temp, true);
                FileStream stream = null;
                try
                {
                    stream = new FileStream(temp, FileMode.Open, FileAccess.Read);
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        foreach (DataTable table in dataSet.Tables)
                        {
                            if (table.Columns.Count == 0) //空表
                                continue;
                            if (table.Rows.Count < 3) //空行
                                continue;
                            var dataTable = new DataTable(table.TableName);
                            var columnTypes = new List<Type>();
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                var columnName = table.Rows[0][i].ToString();
                                var columnType = table.Rows[1][i].ToString();
                                if (string.IsNullOrEmpty(columnName) | string.IsNullOrEmpty(columnType))
                                    continue;
                                var dataType = GetDataType(columnType);
                                if (dataType.IsEnum)
                                    dataTable.Columns.Add(new DataColumn(columnName, typeof(int))); //由于枚举类型可能存在于热更新程序集，而表读取早于热更新程序集，所以会导致读取不到类型的问题
                                else
                                    dataTable.Columns.Add(new DataColumn(columnName, dataType));
                                columnTypes.Add(dataType);
                            }
                            for (int x = 3; x < table.Rows.Count; x++)
                            {
                                var row = table.Rows[x];
                                var rowId = row[0].ToString();
                                if (string.IsNullOrEmpty(rowId))
                                    continue;
                                var dataRow = dataTable.NewRow();
                                dataTable.Rows.Add(dataRow);
                                for (int y = 0; y < columnTypes.Count; y++)
                                {
                                    var dataType = columnTypes[y];
                                    if (dataType == typeof(bool))
                                        dataRow[y] = ObjectConverter.AsBool(row[y]);
                                    else if (dataType == typeof(byte))
                                        dataRow[y] = ObjectConverter.AsByte(row[y]);
                                    else if (dataType == typeof(sbyte))
                                        dataRow[y] = ObjectConverter.AsSbyte(row[y]);
                                    else if (dataType == typeof(char))
                                        dataRow[y] = ObjectConverter.AsChar(row[y]);
                                    else if (dataType == typeof(short))
                                        dataRow[y] = ObjectConverter.AsShort(row[y]);
                                    else if (dataType == typeof(ushort))
                                        dataRow[y] = ObjectConverter.AsUshort(row[y]);
                                    else if (dataType == typeof(int))
                                        dataRow[y] = ObjectConverter.AsInt(row[y]);
                                    else if (dataType == typeof(uint))
                                        dataRow[y] = ObjectConverter.AsUint(row[y]);
                                    else if (dataType == typeof(float))
                                        dataRow[y] = ObjectConverter.AsFloat(row[y]);
                                    else if (dataType == typeof(long))
                                        dataRow[y] = ObjectConverter.AsLong(row[y]);
                                    else if (dataType == typeof(ulong))
                                        dataRow[y] = ObjectConverter.AsUlong(row[y]);
                                    else if (dataType == typeof(double))
                                        dataRow[y] = ObjectConverter.AsDouble(row[y]);
                                    else if (dataType == typeof(decimal))
                                        dataRow[y] = ObjectConverter.AsDecimal(row[y]);
                                    else if (dataType == typeof(DateTime))
                                        dataRow[y] = ObjectConverter.AsDateTime(row[y]);
                                    else if (dataType == typeof(string))
                                        dataRow[y] = ObjectConverter.AsString(row[y]);
                                    else if (dataType.IsEnum)
                                        dataRow[y] = (int)ObjectConverter.AsEnum(row[y], dataType);
                                    else
                                        dataRow[y] = row[y];
                                }
                            }
                            dataTable.AcceptChanges();
                            dataSetNew.Tables.Add(dataTable);
                        }
                    }
                }
                finally
                {
                    stream?.Close();
                    File.Delete(temp);
                }
            }
            dataSetNew.AcceptChanges();
            var jsonData = Newtonsoft_X.Json.JsonConvert.SerializeObject(dataSetNew);
            File.WriteAllText(path, jsonData);
            Debug.Log("生成表格数据完成! " + path);
        }

        [MenuItem("GameDesigner/GameCore/GenerateExcelDataToCs", priority = 3)]
        public static void GenerateExcelDataToCs()
        {
            if (AssetBundleBuilder.Instance == null)
            {
                Debug.LogError("请创建打包资源文件，在Project界面的Assets右键菜单GameCore/Create AssetBundleBuilder");
                return;
            }
            foreach (var file in Directory.GetFiles("Tools/Excel/", "*.*"))
            {
                if (!file.EndsWith(".xls") && !file.EndsWith(".xlsx"))
                    continue;
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("~$"))
                    continue;
                var temp = file + ".temp";
                File.Copy(file, temp, true);
                FileStream stream = null;
                try
                {
                    stream = new FileStream(temp, FileMode.Open, FileAccess.Read);
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        var excelPath = Directory.GetFiles(Application.dataPath, "DataConfigScript.txt", SearchOption.AllDirectories)[0]; //必须取出来，否则就是缺少文件了
                        var textSource = File.ReadAllText(excelPath);
                        foreach (DataTable table in dataSet.Tables)
                        {
                            if (table.Columns.Count == 0) //空表
                                continue;
                            if (table.Rows.Count < 3) //   空行
                                continue;
                            var text = textSource.Replace("SHEETNAME", table.TableName);
                            var texts = text.Split(new string[] { "SPLIT" }, 0);
                            var fullSB = new StringBuilder();
                            var fieldSB = new StringBuilder();
                            var indexGetSB = new StringBuilder();
                            var indexSetSB = new StringBuilder();
                            var nameGetSB = new StringBuilder();
                            var nameSetSB = new StringBuilder();
                            var initSB = new StringBuilder();
                            for (int i = 1; i < table.Columns.Count; i++)
                            {
                                var name = table.Rows[0][i].ToString();
                                var columnType = table.Rows[1][i].ToString();
                                var des = table.Rows[2][i].ToString();
                                if (string.IsNullOrEmpty(name) | string.IsNullOrEmpty(columnType))
                                    continue;
                                var dataType = GetDataType(columnType);
                                indexGetSB.AppendLine($"                case {i - 1}: return {name};");
                                indexSetSB.AppendLine($"                case {i - 1}: {name} = ({columnType})value; break;");
                                nameGetSB.AppendLine($"                case \"{name}\": return {name};");
                                nameSetSB.AppendLine($"                case \"{name}\": {name} = ({columnType})value; break;");

                                var text3 = texts[1].Replace("NAME", name);
                                text3 = text3.Replace("TYPE", columnType);
                                text3 = text3.Replace("NOTE", des);
                                text3 = text3.TrimStart('\r', '\n');
                                fieldSB.AppendLine(text3);

                                text3 = texts[3].Replace("NAME", name);
                                columnType = columnType[0].ToString().ToUpper() + columnType.Substring(1, columnType.Length - 1);
                                if (dataType.IsEnum)
                                    text3 = text3.Replace("ObjectConverter.AsTYPE", $"({columnType})ObjectConverter.AsInt");
                                else
                                    text3 = text3.Replace("TYPE", columnType);
                                text3 = text3.TrimStart('\r', '\n');
                                text3 = text3.TrimEnd('\n', '\r');
                                initSB.AppendLine(text3);
                            }
                            texts[2] = texts[2].Replace("INDEX_GET", indexGetSB.ToString());
                            texts[2] = texts[2].Replace("INDEX_SET", indexSetSB.ToString());
                            texts[2] = texts[2].Replace("COLUMN_GET", nameGetSB.ToString());
                            texts[2] = texts[2].Replace("COLUMN_SET", nameSetSB.ToString());
                            fullSB.Append(texts[0]);
                            fullSB.Append(fieldSB.ToString());
                            fullSB.Append(texts[2]);
                            fullSB.Append(initSB.ToString());
                            fullSB.Append(texts[4]);
                            var text5 = fullSB.ToString();
                            File.WriteAllText($"{AssetBundleBuilder.Instance.tableScriptPath}/{table.TableName}DataConfig.cs", text5);
                            var path = $"{AssetBundleBuilder.Instance.tableScriptPathEx}/{table.TableName}DataConfigEx.cs";
                            if (!File.Exists(path))
                            {
                                var excelScriptEx = $"public partial class {table.TableName}DataConfig\r\n{{\r\n}}";
                                File.WriteAllText(path, excelScriptEx);
                            }
                            Debug.Log($"生成表:{table.TableName}完成!");
                        }
                        Debug.Log("全部表生成完毕!");
                        AssetDatabase.Refresh();
                    }
                }
                finally
                {
                    stream?.Close();
                    File.Delete(temp);
                }
            }
        }

        private static Type GetDataType(string columnType)
        {
            Type dataType;
            switch (columnType.ToLower())
            {
                case "boolean": dataType = typeof(bool); break;
                case "bool": dataType = typeof(bool); break;
                case "byte": dataType = typeof(byte); break;
                case "sbyte": dataType = typeof(sbyte); break;
                case "char": dataType = typeof(char); break;
                case "short": dataType = typeof(short); break;
                case "ushort": dataType = typeof(ushort); break;
                case "int": dataType = typeof(int); break;
                case "uint": dataType = typeof(uint); break;
                case "long": dataType = typeof(long); break;
                case "ulong": dataType = typeof(ulong); break;
                case "float": dataType = typeof(float); break;
                case "double": dataType = typeof(double); break;
                case "decimal": dataType = typeof(decimal); break;
                case "dateTime": dataType = typeof(DateTime); break;
                case "string": dataType = typeof(string); break;
                case "json": dataType = typeof(string); break;
                default:
                    dataType = AssemblyHelper.GetTypeNotOptimized(columnType);
                    dataType ??= typeof(object);
                    break;
            }
            return dataType;
        }

        [MenuItem("GameDesigner/GameCore/GenerateExcelDataAll", priority = 4)]
        public static void GenerateExcelDataAll()
        {
            GenerateExcelData();
            GenerateExcelDataToCs();
            AssetDatabase.Refresh();
        }

        static ExcelTools()
        {
            EditorApplication.projectChanged += UpdateCurrent;
            UpdateCurrent();
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void UpdateCurrent()
        {
            var files = Directory.GetFiles("Tools/Excel/", "*.*");
            var excelNames = new List<string>() { "None" };
            var excelFiles = new List<string>() { "None" };
            foreach (var file in files)
            {
                if (!file.EndsWith(".xls") && !file.EndsWith(".xlsx"))
                    continue;
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("~$"))
                    continue;
                excelNames.Add(Path.GetFileName(file));
                excelFiles.Add(file);
            }
            excelNames.Add("生成Excel表数据");
            excelFiles.Add("GenerateExcelDataAll");
            m_ExcelName = excelNames.ToArray();
            m_ExcelPath = excelFiles.ToArray();
        }

        static void OnToolbarGUI()
        {
            if (m_ExcelName.Length <= 0)
                return;
            var size = EditorStyles.popup.CalcSize(new GUIContent(m_ExcelName[excelSelected]));
            EditorGUILayout.LabelField("游戏配置:", GUILayout.Width(55));
            int excelSelectedNew = EditorGUILayout.Popup(excelSelected, m_ExcelName, GUILayout.Width(size.x + 5f), GUILayout.MinWidth(55));
            if (excelSelectedNew != excelSelected)
            {
                excelSelected = 0;
                var currSelect = m_ExcelPath[excelSelectedNew];
                if (currSelect == "GenerateExcelDataAll")
                    GenerateExcelDataAll();
                else if (currSelect != "None")
                    InternalEditorUtility.OpenFileAtLineExternal(m_ExcelPath[excelSelectedNew], 0);
            }
        }
    }
}
