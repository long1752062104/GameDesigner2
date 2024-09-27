using UnityEditor;
using System.IO;
using ExcelDataReader;
using UnityEngine;
using System.Data;
using System.Text;
using GameCore;
using System;

public class ExcelTools
{
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
        var excelPath = "Tools/Excel/GameConfig.xls";
        var temp = excelPath + ".temp";
        File.Copy(excelPath, temp, true);
        try
        {
            using (var stream = new FileStream(temp, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet();
                    var dataSetNew = new DataSet();
                    foreach (DataTable table in dataSet.Tables)
                    {
                        var dataTable = new DataTable(table.TableName);
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            var columnName = table.Rows[0][i].ToString();
                            var columnType = table.Rows[1][i].ToString();
                            if (string.IsNullOrEmpty(columnName) | string.IsNullOrEmpty(columnType))
                                continue;
                            Type dataType;
                            switch (columnType)
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
                                case "DateTime": dataType = typeof(DateTime); break;
                                case "dateTime": dataType = typeof(DateTime); break;
                                case "string": dataType = typeof(string); break;
                                default: dataType = typeof(object); break;
                            }
                            dataTable.Columns.Add(new DataColumn(columnName, dataType));
                        }
                        for (int x = 3; x < table.Rows.Count; x++)
                        {
                            var row = table.Rows[x];
                            var dataRow = dataTable.NewRow();
                            dataTable.Rows.Add(dataRow);
                            for (int y = 0; y < dataTable.Columns.Count; y++)
                            {
                                var dataType = dataTable.Columns[y].DataType;
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
                                else
                                    dataRow[y] = row[y];
                            }
                        }
                        dataTable.AcceptChanges();
                        dataSetNew.Tables.Add(dataTable);
                    }
                    dataSetNew.AcceptChanges();
                    var jsonStr = Newtonsoft_X.Json.JsonConvert.SerializeObject(dataSetNew);
                    File.WriteAllText(path, jsonStr);
                    Debug.Log("生成表格数据完成! " + path);
                }
            }
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [MenuItem("GameDesigner/GameCore/GenerateExcelDataToCs", priority = 3)]
    public static void GenerateExcelDataToCs()
    {
        if (AssetBundleBuilder.Instance == null)
        {
            Debug.LogError("请创建打包资源文件，在Project界面的Assets右键菜单GameCore/Create AssetBundleBuilder");
            return;
        }
        string excelPath = "Tools/Excel/GameConfig.xls";
        var temp = excelPath + ".temp";
        File.Copy(excelPath, temp, true);
        try
        {
            using (var stream = new FileStream(temp, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet();
                    var files = Directory.GetFiles(Application.dataPath, "DataConfigScript.txt", SearchOption.AllDirectories);
                    var excelPath1 = "";
                    foreach (var file in files)
                    {
                        var info = new FileInfo(file);
                        if (info.Directory.Name == "Template")
                        {
                            excelPath1 = file;
                            break;
                        }
                    }
                    var textSource = File.ReadAllText(excelPath1);
                    foreach (DataTable table in dataSet.Tables)
                    {
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
                            var typeStr = table.Rows[1][i].ToString();
                            var des = table.Rows[2][i].ToString();

                            if (string.IsNullOrEmpty(name) | string.IsNullOrEmpty(typeStr))
                                continue;

                            indexGetSB.AppendLine($"                case {i - 1}: return {name};");
                            indexSetSB.AppendLine($"                case {i - 1}: {name} = ({typeStr})value; break;");
                            nameGetSB.AppendLine($"                case \"{name}\": return {name};");
                            nameSetSB.AppendLine($"                case \"{name}\": {name} = ({typeStr})value; break;");

                            var text3 = texts[1].Replace("NAME", name);
                            text3 = text3.Replace("TYPE", typeStr);
                            text3 = text3.Replace("NOTE", des);
                            text3 = text3.TrimStart('\r', '\n');
                            fieldSB.AppendLine(text3);

                            text3 = texts[3].Replace("NAME", name);
                            typeStr = typeStr[0].ToString().ToUpper() + typeStr.Substring(1, typeStr.Length - 1);
                            text3 = text3.Replace("TYPE", typeStr);
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
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [MenuItem("GameDesigner/GameCore/GenerateExcelDataAll", priority = 4)]
    public static void GenerateExcelDataAll()
    {
        GenerateExcelData();
        GenerateExcelDataToCs();
        AssetDatabase.Refresh();
    }
}
