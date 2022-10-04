#if UNITY_EDITOR
using Net.Helper;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class InvokeHelperTools : EditorWindow
{
    SerializedObject serializedObject;

    [MenuItem("GameDesigner/Network/InvokeHelper")]
    static void ShowWindow()
    {
        var window = GetWindow<InvokeHelperTools>("字段同步，远程过程调用帮助工具");
        window.Show();
    }

    private void OnEnable()
    {
        LoadData();
        serializedObject = new SerializedObject(ConfigObject);
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.MaxHeight(position.height));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Config"), true);
        if (GUILayout.Button("执行", GUILayout.Height(30)))
        {
            OnScriptCompilation();
            Debug.Log("更新完成!");
        }
        GUILayout.EndScrollView();
        if (EditorGUI.EndChangeCheck())
            SaveData();
        serializedObject.ApplyModifiedProperties();
    }

    static InvokeHelperConfigObject configObject;
    internal static InvokeHelperConfigObject ConfigObject
    {
        get
        {
            if (configObject == null)
                configObject = CreateInstance<InvokeHelperConfigObject>();
            return configObject;
        }
    }
    internal static InvokeHelperConfig Config = new InvokeHelperConfig();
    private Vector2 scrollPosition;

    internal static void LoadData()
    {
        var path = Application.dataPath.Replace("Assets", "") + "InvokeHelper.txt";
        if (File.Exists(path))
        {
            var jsonStr = File.ReadAllText(path);
            Config = Newtonsoft_X.Json.JsonConvert.DeserializeObject<InvokeHelperConfig>(jsonStr);
            ConfigObject.Config = Config;
        }
    }

    internal static void SaveData()
    {
        var jsonstr = Newtonsoft_X.Json.JsonConvert.SerializeObject(Config);
        foreach (var data in Config.rpcConfig)
        {
            File.WriteAllText(data.readConfigPath + "/InvokeHelper.txt", jsonstr);
        }
        var path = Application.dataPath + "/../InvokeHelper.txt";
        File.WriteAllText(path, jsonstr);
    }

    [DidReloadScripts]
    public static void OnScriptCompilation()
    {
        LoadData();
        int change = 0;
        if (string.IsNullOrEmpty(Config.savePath))
        {
            var path = Application.dataPath + "/Scripts/Helper/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Config.savePath = path;
            change++;
        }
        if (Config.dllPaths.Count == 0)
        {
            var path = Application.dataPath + "/../Library/ScriptAssemblies/Assembly-CSharp.dll";
            Config.dllPaths.Add(path);
            change++;
        }
        if (change > 0)
            SaveData();
        InvokeHelperBuild.OnScriptCompilation(Config);
    }
}
#endif