#if UNITY_EDITOR
using Net.Helper;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ImportSettingWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Data data;
    private readonly string[] displayedOptions = new string[] { "使用者", "开发者" };

    public class Data
    {
        public string path = "Assets/Plugins/GameDesigner";
        public int develop;
    }

    [MenuItem("GameDesigner/Import Window", priority = 0)]
    static void ShowWindow()
    {
        var window = GetWindow<ImportSettingWindow>("Import");
        window.Show();
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnDisable()
    {
        SaveData();
    }

    void LoadData()
    {
        data = PersistHelper.Deserialize<Data>("importdata.json");
    }

    void SaveData()
    {
        PersistHelper.Serialize(data, "importdata.json");
    }

    private void DrawGUI(string path, string name, string sourceProtocolName, string copyToProtocolName, Action import /*= null*/, string pluginsPath /*= "Assets/Plugins/GameDesigner/"*/)
    {
        if (Directory.Exists(path))
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"重新导入{name}模块"))
            {
                import?.Invoke();
                Import(sourceProtocolName, copyToProtocolName, pluginsPath);
            }
            if (data.develop == 1)
            {
                if (GUILayout.Button($"反导{name}模块", GUILayout.Width(200)))
                    ReverseImport(sourceProtocolName, copyToProtocolName, pluginsPath);
            }
            GUI.color = Color.red;
            if (GUILayout.Button($"移除{name}模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }
        else if (GUILayout.Button($"导入{name}模块"))
        {
            import?.Invoke();
            Import(sourceProtocolName, copyToProtocolName, pluginsPath);
        }
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        data.develop = EditorGUILayout.Popup("使用模式", data.develop, displayedOptions);
        if (EditorGUI.EndChangeCheck())
            SaveData();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("导入路径:", data.path);
        if (GUILayout.Button("选择路径", GUILayout.Width(80)))
        {
            var importPath = EditorUtility.OpenFolderPanel("选择导入路径", "", "");
            //相对于Assets路径
            data.path = PathHelper.GetRelativePath(Application.dataPath, importPath);
            SaveData();
        }
        EditorGUILayout.EndHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        var pathRoot = $"{Application.dataPath}/{data.path.Replace("Assets", string.Empty)}/"; //防止指向磁盘根目录问题
        EditorGUILayout.HelpBox("Tcp&Gcp模块 基础网络协议模块", MessageType.Info);
        var path = pathRoot + "Network/Gcp";
        DrawGUI(path, "Gcp", "Network/Gcp~", "Network/Gcp", null, pathRoot);

        EditorGUILayout.HelpBox("Udx模块 可用于帧同步，视频流，直播流，大数据传输", MessageType.Info);
        path = pathRoot + "Network/Udx";
        DrawGUI(path, "Udx", "Network/Udx~", "Network/Udx", null, pathRoot);

        EditorGUILayout.HelpBox("Kcp模块 可用于帧同步 即时游戏", MessageType.Info);
        path = pathRoot + "Network/Kcp";
        DrawGUI(path, "Kcp", "Network/Kcp~", "Network/Kcp", null, pathRoot);

        EditorGUILayout.HelpBox("Web模块 可用于网页游戏 WebGL", MessageType.Info);
        path = pathRoot + "Network/Web";
        DrawGUI(path, "Web", "Network/Web~", "Network/Web", null, pathRoot);

        EditorGUILayout.HelpBox("StateMachine模块 可用格斗游戏，或基础游戏动作设计", MessageType.Info);
        path = pathRoot + "StateMachine";
        DrawGUI(path, "StateMachine", "StateMachine~", "StateMachine", null, pathRoot);

        EditorGUILayout.HelpBox("NetworkComponents模块 封装了一套完整的客户端网络组件", MessageType.Info);
        path = pathRoot + "Component";
        DrawGUI(path, "NetworkComponent", "Component~", "Component", null, pathRoot);

        EditorGUILayout.HelpBox("MVC模块 可用于帧同步设计，视图，逻辑分离", MessageType.Info);
        path = pathRoot + "MVC";
        DrawGUI(path, "MVC", "MVC~", "MVC", null, pathRoot);

        EditorGUILayout.HelpBox("ECS模块 可用于双端的独立代码运行", MessageType.Info);
        path = pathRoot + "ECS";
        DrawGUI(path, "ECS", "ECS~", "ECS", null, pathRoot);

        EditorGUILayout.HelpBox("MMORPG模块 用于MMORPG设计怪物点, 巡逻点, 地图数据等", MessageType.Info);
        path = pathRoot + "MMORPG";
        DrawGUI(path, "MMORPG", "MMORPG~", "MMORPG", () =>
        {
            Import("AOI~", "AOI", pathRoot);//依赖
            Import("Recast~", "Recast", pathRoot);//依赖
        }, pathRoot);

        EditorGUILayout.HelpBox("AOI模块 可用于MMORPG大地图同步方案，九宫格同步， 或者单机大地图分割显示", MessageType.Info);
        path = pathRoot + "AOI";
        DrawGUI(path, "AOI", "AOI~", "AOI", null, pathRoot);

        EditorGUILayout.HelpBox("Recast & Detour寻路模块 用于双端AI寻路", MessageType.Info);
        path = pathRoot + "Recast";
        DrawGUI(path, "Recast", "Recast~", "Recast", null, pathRoot);

        EditorGUILayout.HelpBox("Framework模块 客户端框架, 包含热更新，Excel读表，Global全局管理，其他管理", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("HyBridCLR地址,下载或Git导入包"))
        {
            Application.OpenURL(@"https://gitee.com/focus-creative-games/hybridclr_unity");
        }
        path = pathRoot + "Framework";
        DrawGUI(path, "Framework", "Framework~", "Framework", null, pathRoot);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Entities模块 可用于独立环境运行, 代替ECS模块", MessageType.Info);
        path = pathRoot + "Entities";
        DrawGUI(path, "Entities", "Entities~", "Entities", null, pathRoot);

        EditorGUILayout.HelpBox("ParrelSync插件, 可以克隆两个一模一样的项目进行网络同步调式, 极快解决联机同步问题", MessageType.Info);
        path = pathRoot + "ParrelSync";
        DrawGUI(path, "ParrelSync", "ParrelSync~", "ParrelSync", null, pathRoot);

        EditorGUILayout.HelpBox("基础模块导入", MessageType.Warning);
        if (GUILayout.Button("基础模块导入", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp", pathRoot);
            Import("Component~", "Component", pathRoot);
        }
        EditorGUILayout.HelpBox("所有模块导入", MessageType.Warning);
        if (GUILayout.Button("所有模块导入", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp", pathRoot);
            Import("Network/Udx~", "Network/Udx", pathRoot);
            Import("Network/Kcp~", "Network/Kcp", pathRoot);
            Import("Network/Web~", "Network/Web", pathRoot);
            Import("Component~", "Component", pathRoot);
            Import("StateMachine~", "StateMachine", pathRoot);
            Import("MVC~", "MVC", pathRoot);
            Import("ECS~", "ECS", pathRoot);
            Import("MMORPG~", "MMORPG", pathRoot);
            Import("AOI~", "AOI", pathRoot);
            Import("Recast~", "Recast", pathRoot);
            Import("Framework~", "Framework", pathRoot);
            Import("Entities~", "Entities", pathRoot);
        }
        EditorGUILayout.HelpBox("所有案例导入，用于学习和快速上手", MessageType.Warning);
        if (GUILayout.Button("案例导入", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp", pathRoot);
            Import("Network/Udx~", "Network/Udx", pathRoot);
            Import("Network/Kcp~", "Network/Kcp", pathRoot);
            Import("Network/Web~", "Network/Web", pathRoot);
            Import("Component~", "Component", pathRoot);
            Import("StateMachine~", "StateMachine", pathRoot);
            Import("MVC~", "MVC", pathRoot);
            Import("ECS~", "ECS", pathRoot);
            Import("MMORPG~", "MMORPG", pathRoot);
            Import("AOI~", "AOI", pathRoot);
            Import("Recast~", "Recast", pathRoot);
            Import("Entities~", "Entities", pathRoot);
            Import("Example~", "Example", "Assets/Samples/GameDesigner/");
        }
        EditorGUILayout.HelpBox("重新导入已导入的模块", MessageType.Warning);
        if (GUILayout.Button("重新导入已导入的模块", GUILayout.Height(20)))
        {
            ReImport("Network/Gcp~", "Network/Gcp", pathRoot);
            ReImport("Network/Udx~", "Network/Udx", pathRoot);
            ReImport("Network/Kcp~", "Network/Kcp", pathRoot);
            ReImport("Network/Web~", "Network/Web", pathRoot);
            ReImport("Component~", "Component", pathRoot);
            ReImport("StateMachine~", "StateMachine", pathRoot);
            ReImport("MVC~", "MVC", pathRoot);
            ReImport("ECS~", "ECS", pathRoot);
            ReImport("MMORPG~", "MMORPG", pathRoot);
            ReImport("AOI~", "AOI", pathRoot);
            ReImport("Recast~", "Recast", pathRoot);
            ReImport("Framework~", "Framework", pathRoot);
            ReImport("Entities~", "Entities", pathRoot);
        }
        if (data.develop == 1)
        {
            EditorGUILayout.HelpBox("反导已导入的模块", MessageType.Warning);
            if (GUILayout.Button("反导已导入的模块", GUILayout.Height(20)))
            {
                ReverseImport("Network/Gcp~", "Network/Gcp", pathRoot);
                ReverseImport("Network/Udx~", "Network/Udx", pathRoot);
                ReverseImport("Network/Kcp~", "Network/Kcp", pathRoot);
                ReverseImport("Network/Web~", "Network/Web", pathRoot);
                ReverseImport("Component~", "Component", pathRoot);
                ReverseImport("StateMachine~", "StateMachine", pathRoot);
                ReverseImport("MVC~", "MVC", pathRoot);
                ReverseImport("ECS~", "ECS", pathRoot);
                ReverseImport("MMORPG~", "MMORPG", pathRoot);
                ReverseImport("AOI~", "AOI", pathRoot);
                ReverseImport("Recast~", "Recast", pathRoot);
                ReverseImport("Framework~", "Framework", pathRoot);
                ReverseImport("Entities~", "Entities", pathRoot);
            }
        }
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Gitee", GUILayout.Height(20)))
        {
            Application.OpenURL(@"https://gitee.com/leng_yue/GameDesigner");
        }
        if (GUILayout.Button("加入QQ群:825240544", GUILayout.Height(20)))
        {
            Application.OpenURL(@"https://jq.qq.com/?_wv=1027&k=nx1Psgjz");
        }
        if (GUILayout.Button("版本:2022.12.12", GUILayout.Height(20)))
        {
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private static void ReImport(string sourceProtocolName, string copyToProtocolName, string pluginsPath)
    {
        var rootPath = "Packages/com.gamedesigner.network";//包的根路径
        if (!Directory.Exists(rootPath))
            rootPath = Application.dataPath + "/GameDesigner";//直接放Assets目录的路径
        if (!Directory.Exists(rootPath))
        {
            Debug.LogError("找不到根路径, 无法执行, 请使用包管理器添加gdnet, 或者根路径必须在Assets目录下!");
            return;
        }
        var path = $"{pluginsPath}{copyToProtocolName}/";
        if (!Directory.Exists(path))
            return;
        try { Directory.Delete(path, true); } catch { } //删除原文件再导入新文件
        Import(sourceProtocolName, copyToProtocolName, pluginsPath);
    }

    private static void Import(string sourceProtocolName, string copyToProtocolName, string pluginsPath)
    {
        var rootPath = "Packages/com.gamedesigner.network";//包的根路径
        if (!Directory.Exists(rootPath))
            rootPath = Application.dataPath + "/GameDesigner";//直接放Assets目录的路径
        if (!Directory.Exists(rootPath))
        {
            Debug.LogError("找不到根路径, 无法执行, 请使用包管理器添加gdnet, 或者根路径必须在Assets目录下!");
            return;
        }
        var path = $"{rootPath}/{sourceProtocolName}/";
        if (!Directory.Exists(path))
        {
            Debug.LogError("找不到路径:" + path);
            return;
        }
        var commonPath = $"{pluginsPath}/Common/";
        if (Directory.Exists(commonPath))
            try { Directory.Delete(commonPath, true); } catch { } //删除Common文件夹, 因为Common已经内置了
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var item = file.Split(new string[] { sourceProtocolName }, StringSplitOptions.RemoveEmptyEntries);
            var newPath = $"{pluginsPath}{copyToProtocolName}/{item[1]}";
            var path1 = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(path1))
                Directory.CreateDirectory(path1);
            try { File.Copy(file, newPath, true); }
            catch (Exception ex) { Debug.LogError(ex); }
        }
        Debug.Log($"导入{Path.GetFileName(copyToProtocolName)}完成!");
        AssetDatabase.Refresh();
    }

    private static void ReverseImport(string sourceProtocolName, string copyToProtocolName, string pluginsPath)
    {
        var rootPath = "Packages/com.gamedesigner.network";//包的根路径
        if (!Directory.Exists(rootPath))
            rootPath = Application.dataPath + "/GameDesigner";//直接放Assets目录的路径
        if (!Directory.Exists(rootPath))
        {
            Debug.LogError("找不到根路径, 无法执行, 请使用包管理器添加gdnet, 或者根路径必须在Assets目录下!");
            return;
        }
        var path = $"{pluginsPath}{copyToProtocolName}/";
        if (!Directory.Exists(path))
            return;
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var newFile = file.Replace(path, "");
            var newPath = $"{rootPath}/{sourceProtocolName}/{newFile}";
            if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(newPath));
            File.Copy(file, newPath, true);
        }
        Debug.Log($"反导出{Path.GetFileName(copyToProtocolName)}完成!");
        AssetDatabase.Refresh();
    }
}
#endif