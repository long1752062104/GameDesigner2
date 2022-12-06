using UnityEditor;
using System.IO;
using ExcelDataReader;
using UnityEngine;

public class ExcelTools
{
    [MenuItem("GameDesigner/Framework/GenerateExcelData", priority = 2)]
    public static void GenerateExcelData()
    {
        string path = "AssetBundles/Table/GameData.json";
        if (!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        string excelPath = "Tools/Excel/GameData.xls";
        var temp = excelPath + ".temp";
        File.Copy(excelPath, temp, true);
        try
        {
            using (var stream = new FileStream(temp, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet();
                    var jsonStr = Newtonsoft_X.Json.JsonConvert.SerializeObject(dataSet);
                    File.WriteAllText(path, jsonStr);
                    Debug.Log("生成表格数据完成!!!!!" + System.Environment.CurrentDirectory + "/" + path);
                }
            }
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
