#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class ImportSettingWindow : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("GameDesigner/Import Window", priority = 0)]
    static void ShowWindow()
    {
        var window = GetWindow<ImportSettingWindow>("Import");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("导入模块:");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        EditorGUILayout.HelpBox("Tcp&Gcp模块 基础网络协议模块", MessageType.Info);
        var path = "Assets/Plugins/GameDesigner/Network/Gcp";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除Gcp模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入Gcp模块"))
        {
            Import("Network/Gcp~", "Network/Gcp");
        }
        EditorGUILayout.HelpBox("Udx模块 可用于帧同步，视频流，直播流，大数据传输", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Network/Udx";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除Udx模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入Udx模块"))
        {
            Import("Network/Udx~", "Network/Udx");
        }
        EditorGUILayout.HelpBox("Kcp模块 可用于帧同步 即时游戏", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Network/Kcp";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除Kcp模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入Kcp模块"))
        {
            Import("Network/Kcp~", "Network/Kcp");
        }
        EditorGUILayout.HelpBox("Web模块 可用于网页游戏 WebGL", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Network/Web";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除Web模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入Web模块"))
        {
            Import("Network/Web~", "Network/Web");
        }
        EditorGUILayout.HelpBox("StateMachine模块 可用格斗游戏，或基础游戏动作设计", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/StateMachine";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除StateMachine模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入StateMachine模块"))
        {
            Import("StateMachine~", "StateMachine");
        }
        EditorGUILayout.HelpBox("NetworkComponents模块 封装了一套完整的客户端网络组件", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Component";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除Network组件"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入Network组件"))
        {
            Import("Common~", "Common");//依赖
            Import("Component~", "Component");
        }
        EditorGUILayout.HelpBox("MVC模块 可用于帧同步设计，视图，逻辑分离", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/MVC";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除MVC模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入MVC模块"))
        {
            Import("MVC~", "MVC");
        }
        EditorGUILayout.HelpBox("ECS模块 可用于双端的独立代码运行", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/ECS";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除ECS模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入ECS模块"))
        {
            Import("ECS~", "ECS");
        }
        EditorGUILayout.HelpBox("Common模块 通用模块", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Common";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除Common模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入Common模块"))
        {
            Import("Common~", "Common");
        }
        EditorGUILayout.HelpBox("AOI模块 可用于MMORPG大地图同步方案，九宫格同步， 或者单机大地图分割显示", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/AOI";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除AOI模块"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入AOI模块"))
        {
            Import("AOI~", "AOI");
        }
        EditorGUILayout.HelpBox("Framework模块 客户端框架, 包含热更新，Excel读表，Global全局管理，其他管理", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("HyBridCLR地址,下载或Git导入包"))
        {
            Application.OpenURL(@"https://gitee.com/focus-creative-games/hybridclr_unity");
        }
        path = "Assets/Plugins/GameDesigner/Framework";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除客户端框架"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("导入客户端框架"))
        {
            Import("Framework~", "Framework");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("基础模块导入", MessageType.Warning);
        if (GUILayout.Button("基础模块导入", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp");
            Import("Component~", "Component");
            Import("Common~", "Common");
        }
        EditorGUILayout.HelpBox("所有模块导入", MessageType.Warning);
        if (GUILayout.Button("所有模块导入", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp");
            Import("Network/Udx~", "Network/Udx");
            Import("Network/Kcp~", "Network/Kcp");
            Import("Network/Web~", "Network/Web");
            Import("Component~", "Component");
            Import("StateMachine~", "StateMachine");
            Import("MVC~", "MVC");
            Import("ECS~", "ECS");
            Import("Common~", "Common");
            Import("AOI~", "AOI");
        }
        EditorGUILayout.HelpBox("所有案例导入，用于学习和快速上手", MessageType.Warning);
        if (GUILayout.Button("案例导入", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp");
            Import("Network/Udx~", "Network/Udx");
            Import("Network/Kcp~", "Network/Kcp");
            Import("Network/Web~", "Network/Web");
            Import("Component~", "Component");
            Import("StateMachine~", "StateMachine");
            Import("MVC~", "MVC");
            Import("ECS~", "ECS");
            Import("Common~", "Common");
            Import("AOI~", "AOI");
            Import("Example~", "Example", "Assets/Samples/GameDesigner/");
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

    private static void Import(string sourceProtocolName, string copyToProtocolName, string pluginsPath = "Assets/Plugins/GameDesigner/")
    {
        var path = $"Packages/com.gamedesigner.network/{sourceProtocolName}/";
        if (!Directory.Exists(path))
        {
            Debug.LogError("找不到路径:" + path);
            return;
        }
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var item = file.Split(new string[] { sourceProtocolName }, System.StringSplitOptions.RemoveEmptyEntries);
            var newPath = $"{pluginsPath}{copyToProtocolName}/{item[1]}";
            var path1 = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(path1))
                Directory.CreateDirectory(path1);
            File.Copy(file, newPath, true);
        }
        AssetDatabase.Refresh();
    }
}
#endif