#if UNITY_EDITOR
using Net.Helper;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class InvokeHelperTools : EditorWindow
{
    private Vector2 scrollPosition, scrollPosition1, scrollPosition2, scrollPosition4, scrollPosition5, scrollPosition6;
    private static List<string> clientPaths = new List<string>();
    private static List<string> serverPaths = new List<string>();
    private static List<string> csprojPaths = new List<string>();
    private static List<string> clientBinPaths = new List<string>();
    private static List<string> serverBinPaths = new List<string>();
    private static List<string> serverConfigPaths = new List<string>();

    [MenuItem("GameDesigner/Network/InvokeHelper")]
    static void ShowWindow()
    {
        var window = GetWindow<InvokeHelperTools>("字段同步，远程过程调用帮助工具");
        window.Show();
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("VS项目文件列表:");
        if (GUILayout.Button("选择文件", GUILayout.Width(100)))
        {
            var csprojPath = EditorUtility.OpenFilePanel("选择文件", "", "csproj");
            if (!string.IsNullOrEmpty(csprojPath))
                csprojPaths.Add(csprojPath);
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.MaxHeight(position.height / 6));
        foreach (var path in csprojPaths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                csprojPaths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("服务器配置文件保存路径:");
        if (GUILayout.Button("选择路径", GUILayout.Width(100)))
        {
            var csprojPath = EditorUtility.OpenFolderPanel("选择路径", "", "");
            if (!string.IsNullOrEmpty(csprojPath))
                serverConfigPaths.Add(csprojPath);
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition6 = GUILayout.BeginScrollView(scrollPosition6, false, true, GUILayout.MaxHeight(position.height / 6));
        foreach (var path in serverConfigPaths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                serverConfigPaths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unity程序集路径:");
        if (GUILayout.Button("选择文件", GUILayout.Width(100)))
        {
            var csprojPath = EditorUtility.OpenFilePanelWithFilters("选择文件", "", new string[] { "dll/exe files", "dll,exe" });
            if (!string.IsNullOrEmpty(csprojPath))
                clientBinPaths.Add(csprojPath);
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition4 = GUILayout.BeginScrollView(scrollPosition4, false, true, GUILayout.MaxHeight(position.height / 6));
        foreach (var path in clientBinPaths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                clientBinPaths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unity脚本存放路径:");
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100)))
        {
            var path = EditorUtility.OpenFolderPanel("引用路径", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (!clientPaths.Contains(path))
                    clientPaths.Add(path);
            }
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, false, true, GUILayout.MaxHeight(position.height / 6));
        foreach (var path in clientPaths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                clientPaths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Server程序集路径:");
        if (GUILayout.Button("选择文件", GUILayout.Width(100)))
        {
            var csprojPath = EditorUtility.OpenFilePanelWithFilters("选择文件", "", new string[] { "dll/exe files", "dll,exe" });
            if (!string.IsNullOrEmpty(csprojPath))
                serverBinPaths.Add(csprojPath);
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition5 = GUILayout.BeginScrollView(scrollPosition5, false, true, GUILayout.MaxHeight(position.height / 6));
        foreach (var path in serverBinPaths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                serverBinPaths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Server脚本存放路径:");
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100)))
        {
            var path = EditorUtility.OpenFolderPanel("引用路径", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (!serverPaths.Contains(path))
                    serverPaths.Add(path);
            }
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, false, true, GUILayout.MaxHeight(position.height / 6));
        foreach (var path in serverPaths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                serverPaths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();

        if (GUILayout.Button("执行", GUILayout.Height(30)))
        {
            OnScriptCompilation();
            Debug.Log("更新完成!");
        }
        if (EditorGUI.EndChangeCheck())
            SaveData();
    }

    static void LoadData()
    {
        var path = Application.dataPath.Replace("Assets", "") + "InvokeHelper.txt";
        if (File.Exists(path))
        {
            var jsonStr = File.ReadAllText(path);
            var data = Newtonsoft_X.Json.JsonConvert.DeserializeObject<Data>(jsonStr);
            clientPaths = data.clientPaths;
            serverPaths = data.serverPaths;
            csprojPaths = data.csprojPaths;
            clientBinPaths = data.clientBinPaths;
            serverBinPaths = data.serverBinPaths;
            serverConfigPaths = data.serverConfigPaths;
        }
    }

    static void SaveData()
    {
        var data = new Data() { 
            clientPaths = clientPaths, 
            serverPaths = serverPaths, 
            csprojPaths = csprojPaths,
            clientBinPaths = clientBinPaths,
            serverBinPaths = serverBinPaths,
            serverConfigPaths = serverConfigPaths,
        };
        var jsonstr = Newtonsoft_X.Json.JsonConvert.SerializeObject(data);
        foreach (var path1 in serverConfigPaths)
        {
            File.WriteAllText(path1 + "/InvokeHelper.txt", jsonstr);
        }
        var path = Application.dataPath.Replace("Assets", "") + "InvokeHelper.txt";
        File.WriteAllText(path, jsonstr);
    }

    internal class Data
    {
        public List<string> csprojPaths = new List<string>();
        public List<string> clientPaths = new List<string>();
        public List<string> serverPaths = new List<string>();
        public List<string> clientBinPaths = new List<string>();
        public List<string> serverBinPaths = new List<string>();
        public List<string> serverConfigPaths = new List<string>();
    }

    [DidReloadScripts]
    public static void OnScriptCompilation()
    {
        LoadData();
        int change = 0;
        if (clientPaths.Count == 0)
        {
            var path = Application.dataPath + "/Scripts/Helper/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            clientPaths.Add(path);
            change++;
        }
        if (clientBinPaths.Count == 0)
        {
            var path = Application.dataPath + "/../Library/ScriptAssemblies/Assembly-CSharp.dll";
            clientBinPaths.Add(path);
            change++;
        }
        if (change > 0)
            SaveData();
        InvokeHelperBuild.OnScriptCompilation(clientPaths, serverPaths, csprojPaths, clientBinPaths, serverBinPaths);
    }
}
#endif