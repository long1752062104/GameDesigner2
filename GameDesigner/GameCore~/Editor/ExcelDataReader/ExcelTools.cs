using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using ExcelDataReader;
using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;
using UnityEditorInternal;
using Net.Table;
using Net.Helper;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
                            if (table.TableName == "Type") //类型定义
                                continue;
                            var dataTable = new DataTable(table.TableName);
                            var columnTypes = new List<ITypeSolver>();
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                var columnName = table.Rows[0][i].ToString();
                                var columnType = table.Rows[1][i].ToString();
                                if (string.IsNullOrEmpty(columnName) | string.IsNullOrEmpty(columnType))
                                    continue;
                                if (!TypeSolver.TryGetValue(columnType, out var typeSolver))
                                    continue;
                                var dataType = typeSolver.DataType;
                                if (dataType.IsEnum)
                                    dataTable.Columns.Add(new DataColumn(columnName, typeof(int))); //由于枚举类型可能存在于热更新程序集，而表读取早于热更新程序集，所以会导致读取不到类型的问题
                                else
                                    dataTable.Columns.Add(new DataColumn(columnName, dataType));
                                columnTypes.Add(typeSolver);
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
                                    dataRow[y] = columnTypes[y].As(row[y]);
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
                            if (table.TableName == "Type") //类型定义表
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
                                if (!TypeSolver.TryGetValue(columnType, out var typeSolver))
                                    continue;
                                var dataType = typeSolver.DataType;
                                var fieldType = AssemblyHelper.GetCodeTypeName(dataType.ToString());
                                indexGetSB.AppendLine($"                case {i - 1}: return {name};");
                                indexSetSB.AppendLine($"                case {i - 1}: {name} = ({fieldType})value; break;");
                                nameGetSB.AppendLine($"                case \"{name}\": return {name};");
                                nameSetSB.AppendLine($"                case \"{name}\": {name} = ({fieldType})value; break;");

                                var text3 = texts[1].Replace("NAME", name);
                                text3 = text3.Replace("TYPE", fieldType);
                                text3 = text3.Replace("NOTE", des);
                                text3 = text3.TrimStart('\r', '\n');
                                fieldSB.AppendLine(text3);

                                text3 = texts[3].Replace("NAME", name);
                                if (dataType.IsEnum)
                                    text3 = text3.Replace("TypeSolver<TYPE>", $"({fieldType})TypeSolver<System.Int32>");
                                else
                                    text3 = text3.Replace("TYPE", fieldType);
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
            if (!Directory.Exists("Tools/Excel/"))
                return;
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
            excelNames.Add("设置Excel表类型");
            excelFiles.Add("SetExcelDataType");
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
                else if (currSelect == "SetExcelDataType")
                    SetExcelDataType();
                else if (currSelect != "None")
                    InternalEditorUtility.OpenFileAtLineExternal(m_ExcelPath[excelSelectedNew], 0);
            }
        }

        private static void SetExcelDataType()
        {
            foreach (var file in Directory.GetFiles("Tools/Excel/", "*.*"))
            {
                if (!file.EndsWith(".xlsx"))
                    continue;
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("~$"))
                    continue;
                using (var package = new ExcelPackage(file))
                {
                    var worksheet = package.Workbook.Worksheets["Type"];
                    if (worksheet == null)
                    {
                        worksheet = package.Workbook.Worksheets.Add("Type");
                        var rowTexts = @"ID	Name	Des
int	string	string
编号	类型名称	描述
1	bool	
2	byte	
3	sbyte	
4	char	
5	short	
6	ushort	
7	int	
8	uint	
9	float	
10	long	
11	ulong	
12	double	
13	decimal	
14	dateTime	
15	string	
		
100	boolArray	数组分隔为;或ALT+回车
101	byteArray	数组分隔为;或ALT+回车
102	sbyteArray	数组分隔为;或ALT+回车
103	charArray	数组分隔为;或ALT+回车
104	shortArray	数组分隔为;或ALT+回车
105	ushortArray	数组分隔为;或ALT+回车
106	intArray	数组分隔为;或ALT+回车
107	uintArray	数组分隔为;或ALT+回车
108	floatArray	数组分隔为;或ALT+回车
109	longArray	数组分隔为;或ALT+回车
110	ulongArray	数组分隔为;或ALT+回车
111	doubleArray	数组分隔为;或ALT+回车
112	decimalArray	数组分隔为;或ALT+回车
113	dateTimeArray	数组分隔为;或ALT+回车 写法: 2024-11-08 或 2024-11-08 14:30:00 或 2024/11/08 或 2024/11/08 14:30:00
114	stringArray	数组分隔为;或ALT+回车
		
200	boolList	数组分隔为;或ALT+回车
201	byteList	数组分隔为;或ALT+回车
202	sbyteList	数组分隔为;或ALT+回车
203	charList	数组分隔为;或ALT+回车
204	shortList	数组分隔为;或ALT+回车
205	ushortList	数组分隔为;或ALT+回车
206	intList	数组分隔为;或ALT+回车
207	uintList	数组分隔为;或ALT+回车
208	floatList	数组分隔为;或ALT+回车
209	longList	数组分隔为;或ALT+回车
210	ulongList	数组分隔为;或ALT+回车
211	doubleList	数组分隔为;或ALT+回车
212	decimalList	数组分隔为;或ALT+回车
213	dateTimeList	数组分隔为;或ALT+回车 写法: 2024-11-08 或 2024-11-08 14:30:00 或 2024/11/08 或 2024/11/08 14:30:00
214	stringList	数组分隔为;或ALT+回车
		
300	vector2	x,y,z 使用,区分
301	vector3	x,y,z 使用,区分
302	vector4	x,y,z 使用,区分
303	quaternion	x,y,z,w 使用,区分
304	rect	x,y,z,w 使用,区分
".Split(new string[] { "\r\n" }, 0);
                        for (int i = 0; i < rowTexts.Length; i++)
                        {
                            var rowText = rowTexts[i];
                            var items = rowText.Split('\t');
                            if (items.Length < 3)
                            {
                                worksheet.Cells[i + 1, 1].Value = null;
                                worksheet.Cells[i + 1, 2].Value = null;
                                worksheet.Cells[i + 1, 3].Value = null;
                            }
                            else
                            {
                                if (int.TryParse(items[0].Trim(), out var number))
                                    worksheet.Cells[i + 1, 1].Value = number;
                                else
                                    worksheet.Cells[i + 1, 1].Value = items[0].Trim();
                                worksheet.Cells[i + 1, 2].Value = items[1].Trim();
                                worksheet.Cells[i + 1, 3].Value = items[2].Trim();
                            }
                            worksheet.Cells[i + 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            worksheet.Cells[i + 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            worksheet.Cells[i + 1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                        worksheet.Column(1).Width = 7;
                        worksheet.Column(2).Width = 18;
                        worksheet.Column(3).Width = 100;
                    }
                    for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                    {
                        worksheet = package.Workbook.Worksheets[i];
                        if (worksheet.Name == "Type")
                            continue;
                        if (worksheet.Dimension == null)
                            continue;
                        int rowCount = worksheet.Dimension.Rows; // 获取总行数
                        int colCount = worksheet.Dimension.Columns; // 获取总列数
                        if (rowCount < 2)
                            continue;
                        worksheet.Cells[2, 1, 2, colCount].Style.Font.Bold = true;
                        var fillColor = System.Drawing.Color.FromArgb(50, 217, 217, 217); // 深色
                        worksheet.Cells[2, 1, 2, colCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[2, 1, 2, colCount].Style.Fill.BackgroundColor.SetColor(fillColor); // 设置深色
                        worksheet.Cells[2, 1, 2, colCount].Style.Font.Color.SetColor(System.Drawing.Color.Green); // 字体颜色为绿色
                        worksheet.DataValidations.Clear();
                        for (int col = 1; col <= colCount; col++)
                        {
                            var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[2, col].Address);
                            validation.Formula.ExcelFormula = "=Type!$B$4:$B$10000";
                        }
                    }
                    try
                    {
                        package.SaveAs(file);
                    }
                    catch
                    {
                        Debug.LogError($"文件已被其他程序打开! {file}");
                        continue;
                    }
                    Debug.Log($"设置成功:{file}");
                }
            }
        }
    }
}
