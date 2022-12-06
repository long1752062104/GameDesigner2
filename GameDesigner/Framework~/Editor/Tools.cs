#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Tools
{
	[MenuItem("CONTEXT/MonoBehaviour/ChangeFieldNames")]
	private static void ChangeFieldNames(MenuCommand menuCommand)
	{
		var target = menuCommand.context;
		var type = target.GetType();
		var fields = type.GetFields(BindingFlags.Instance| BindingFlags.Public| BindingFlags.NonPublic);
        foreach (var field in fields)
        {
			if (!field.FieldType.IsSubclassOf(typeof(Object))) 
				continue;
			var value = field.GetValue(target);
			if (value == null)
				continue;
			var obj = value as Object;
			obj.name = field.Name;
		}
		EditorUtility.SetDirty(target);
	}

	[MenuItem("GameDesigner/Framework/Install", priority = 1)]
	private static void Install()
	{
		var path = "Tools/Excel/";
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
		var files = Directory.GetFiles(Application.dataPath, "GameData.xls", SearchOption.AllDirectories);
		var excelPath = "";
		foreach (var file in files)
        {
			var info = new FileInfo(file);
			if (info.Directory.Name == "Template")
			{
				excelPath = file;
				break;
			}
        }
		var excelPath1 = path + "GameData.xls";
		if (!File.Exists(excelPath1))//如果存在表则不能复制进去了, 避免使用者数据丢失
			File.Copy(excelPath, excelPath1);
		Debug.Log($"复制配置表格文件完成:{excelPath1}");

		Debug.Log($"请生成ab文件!!!!!!!!");
	}
}
#endif