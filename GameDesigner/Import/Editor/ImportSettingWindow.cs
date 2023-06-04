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

    private void DrawGUI(string path, string name, string sourceProtocolName, string copyToProtocolName, Action import = null, string pluginsPath = "Assets/Plugins/GameDesigner/") 
    {
        if (Directory.Exists(path))
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"���µ���{name}ģ��"))
            {
                import?.Invoke();
                Import(sourceProtocolName, copyToProtocolName, pluginsPath);
            }
            if (data.develop == 1)
            {
                if (GUILayout.Button($"����{name}ģ��", GUILayout.Width(200)))
                {
                    import?.Invoke();
                    ReverseImport(sourceProtocolName, copyToProtocolName, pluginsPath);
                }
            }
            GUI.color = Color.red;
            if (GUILayout.Button($"�Ƴ�{name}ģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }
        else if (GUILayout.Button($"����{name}ģ��"))
        {
            import?.Invoke();
            Import(sourceProtocolName, copyToProtocolName, pluginsPath);
        }
    }

    private readonly string[] displayedOptions = new string[] { "ʹ����", "������" };

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        data.develop = EditorGUILayout.Popup("ʹ��ģʽ", data.develop, displayedOptions);
        if (EditorGUI.EndChangeCheck())
            SaveData();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("����·��:", data.path);
        if (GUILayout.Button("ѡ��·��", GUILayout.Width(80)))
        {
            var importPath = EditorUtility.OpenFolderPanel("ѡ����·��", "", "");
            //�����Assets·��
            var uri = new Uri(Application.dataPath.Replace('/', '\\'));
            var relativeUri = uri.MakeRelativeUri(new Uri(importPath));
            data.path = relativeUri.ToString();
            SaveData();
        }
        EditorGUILayout.EndHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        EditorGUILayout.HelpBox("Tcp&Gcpģ�� ��������Э��ģ��", MessageType.Info);
        var path = data.path + "/Network/Gcp";
        DrawGUI(path, "Gcp", "Network/Gcp~", "Network/Gcp", null, data.path + "/");

        EditorGUILayout.HelpBox("Udxģ�� ������֡ͬ������Ƶ����ֱ�����������ݴ���", MessageType.Info);
        path = data.path + "/Network/Udx";
        DrawGUI(path, "Udx", "Network/Udx~", "Network/Udx", null, data.path + "/");
        
        EditorGUILayout.HelpBox("Kcpģ�� ������֡ͬ�� ��ʱ��Ϸ", MessageType.Info);
        path = data.path + "/Network/Kcp";
        DrawGUI(path, "Kcp", "Network/Kcp~", "Network/Kcp", null, data.path + "/");
        
        EditorGUILayout.HelpBox("Webģ�� ��������ҳ��Ϸ WebGL", MessageType.Info);
        path = data.path + "/Network/Web";
        DrawGUI(path, "Web", "Network/Web~", "Network/Web", null, data.path + "/");
        
        EditorGUILayout.HelpBox("StateMachineģ�� ���ø���Ϸ���������Ϸ�������", MessageType.Info);
        path = data.path + "/StateMachine";
        DrawGUI(path, "StateMachine", "StateMachine~", "StateMachine", null, data.path + "/");
        
        EditorGUILayout.HelpBox("NetworkComponentsģ�� ��װ��һ�������Ŀͻ����������", MessageType.Info);
        path = data.path + "/Component";
        DrawGUI(path, "NetworkComponent", "Component~", "Component", ()=> {
            Import("Common~", "Common");//����
        }, data.path + "/");

        EditorGUILayout.HelpBox("MVCģ�� ������֡ͬ����ƣ���ͼ���߼�����", MessageType.Info);
        path = data.path + "/MVC";
        DrawGUI(path, "MVC", "MVC~", "MVC", null, data.path + "/"); 
        
        EditorGUILayout.HelpBox("ECSģ�� ������˫�˵Ķ�����������", MessageType.Info);
        path = data.path + "/ECS";
        DrawGUI(path, "ECS", "ECS~", "ECS", null, data.path + "/");
        
        EditorGUILayout.HelpBox("Commonģ�� ����ģ��", MessageType.Info);
        path = data.path + "/Common";
        DrawGUI(path, "Common", "Common~", "Common", null, data.path + "/");
        
        EditorGUILayout.HelpBox("MMORPGģ�� ����MMORPG��ƹ����, Ѳ�ߵ�, ��ͼ���ݵ�", MessageType.Info);
        path = data.path + "/MMORPG";
        DrawGUI(path, "MMORPG", "MMORPG~", "MMORPG", () => {
            Import("AOI~", "AOI");//����
        }, data.path + "/");

        EditorGUILayout.HelpBox("AOIģ�� ������MMORPG���ͼͬ���������Ź���ͬ���� ���ߵ������ͼ�ָ���ʾ", MessageType.Info);
        path = data.path + "/AOI";
        DrawGUI(path, "AOI", "AOI~", "AOI", null, data.path + "/");
        
        EditorGUILayout.HelpBox("Frameworkģ�� �ͻ��˿��, �����ȸ��£�Excel����Globalȫ�ֹ�����������", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("HyBridCLR��ַ,���ػ�Git�����"))
        {
            Application.OpenURL(@"https://gitee.com/focus-creative-games/hybridclr_unity");
        }
        path = data.path + "/Framework";
        DrawGUI(path, "Framework", "Framework~", "Framework", null, data.path + "/");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("ParrelSync���, ���Կ�¡����һģһ������Ŀ��������ͬ����ʽ, ����������ͬ������", MessageType.Info);
        path = data.path + "/ParrelSync";
        DrawGUI(path, "ParrelSync", "ParrelSync~", "ParrelSync", null, data.path + "/");

        EditorGUILayout.HelpBox("����ģ�鵼��", MessageType.Warning);
        if (GUILayout.Button("����ģ�鵼��", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp", data.path + "/");
            Import("Component~", "Component", data.path + "/");
            Import("Common~", "Common", data.path + "/");
        }
        EditorGUILayout.HelpBox("����ģ�鵼��", MessageType.Warning);
        if (GUILayout.Button("����ģ�鵼��", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp", data.path + "/");
            Import("Network/Udx~", "Network/Udx", data.path + "/");
            Import("Network/Kcp~", "Network/Kcp", data.path + "/");
            Import("Network/Web~", "Network/Web", data.path + "/");
            Import("Component~", "Component", data.path + "/");
            Import("StateMachine~", "StateMachine", data.path + "/");
            Import("MVC~", "MVC", data.path + "/");
            Import("ECS~", "ECS", data.path + "/");
            Import("Common~", "Common", data.path + "/");
            Import("MMORPG~", "MMORPG", data.path + "/");
            Import("AOI~", "AOI", data.path + "/");
        }
        EditorGUILayout.HelpBox("���а������룬����ѧϰ�Ϳ�������", MessageType.Warning);
        if (GUILayout.Button("��������", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp", data.path + "/");
            Import("Network/Udx~", "Network/Udx", data.path + "/");
            Import("Network/Kcp~", "Network/Kcp", data.path + "/");
            Import("Network/Web~", "Network/Web", data.path + "/");
            Import("Component~", "Component", data.path + "/");
            Import("StateMachine~", "StateMachine", data.path + "/");
            Import("MVC~", "MVC", data.path + "/");
            Import("ECS~", "ECS", data.path + "/");
            Import("Common~", "Common", data.path + "/");
            Import("MMORPG~", "MMORPG", data.path + "/");
            Import("AOI~", "AOI", data.path + "/");
            Import("Example~", "Example", "Assets/Samples/GameDesigner/");
        }
        EditorGUILayout.HelpBox("���µ����ѵ����ģ��", MessageType.Warning);
        if (GUILayout.Button("���µ����ѵ����ģ��", GUILayout.Height(20)))
        {
            ReImport("Network/Gcp~", "Network/Gcp", data.path + "/");
            ReImport("Network/Udx~", "Network/Udx", data.path + "/");
            ReImport("Network/Kcp~", "Network/Kcp", data.path + "/");
            ReImport("Network/Web~", "Network/Web", data.path + "/");
            ReImport("Component~", "Component", data.path + "/");
            ReImport("StateMachine~", "StateMachine", data.path + "/");
            ReImport("MVC~", "MVC", data.path + "/");
            ReImport("ECS~", "ECS", data.path + "/");
            ReImport("Common~", "Common", data.path + "/");
            ReImport("MMORPG~", "MMORPG", data.path + "/");
            ReImport("AOI~", "AOI", data.path + "/");
        }
        if (data.develop == 1) 
        {
            EditorGUILayout.HelpBox("�����ѵ����ģ��", MessageType.Warning);
            if (GUILayout.Button("�����ѵ����ģ��", GUILayout.Height(20)))
            {
                ReverseImport("Network/Gcp~", "Network/Gcp", data.path + "/");
                ReverseImport("Network/Udx~", "Network/Udx", data.path + "/");
                ReverseImport("Network/Kcp~", "Network/Kcp", data.path + "/");
                ReverseImport("Network/Web~", "Network/Web", data.path + "/");
                ReverseImport("Component~", "Component", data.path + "/");
                ReverseImport("StateMachine~", "StateMachine", data.path + "/");
                ReverseImport("MVC~", "MVC", data.path + "/");
                ReverseImport("ECS~", "ECS", data.path + "/");
                ReverseImport("Common~", "Common", data.path + "/");
                ReverseImport("MMORPG~", "MMORPG", data.path + "/");
                ReverseImport("AOI~", "AOI", data.path + "/");
            }
        }
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Gitee", GUILayout.Height(20)))
        {
            Application.OpenURL(@"https://gitee.com/leng_yue/GameDesigner");
        }
        if (GUILayout.Button("����QQȺ:825240544", GUILayout.Height(20))) 
        {
            Application.OpenURL(@"https://jq.qq.com/?_wv=1027&k=nx1Psgjz");
        }
        if (GUILayout.Button("�汾:2022.12.12", GUILayout.Height(20)))
        {
        }
        GUILayout.EndHorizontal(); 
        GUILayout.Space(10);
    }

    private static void ReImport(string sourceProtocolName, string copyToProtocolName, string pluginsPath = "Assets/Plugins/GameDesigner/")
    {
        var rootPath = "Packages/com.gamedesigner.network";//���ĸ�·��
        if (!Directory.Exists(rootPath))
            rootPath = Application.dataPath + "/GameDesigner";//ֱ�ӷ�AssetsĿ¼��·��
        if (!Directory.Exists(rootPath))
        {
            Debug.LogError("�Ҳ�����·��, �޷�ִ��, ��ʹ�ð����������gdnet, ���߸�·��������AssetsĿ¼��!");
            return;
        }
        var path = $"{pluginsPath}{copyToProtocolName}/";
        if (!Directory.Exists(path))
            return;
        Import(sourceProtocolName, copyToProtocolName, pluginsPath);
    }

    private static void Import(string sourceProtocolName, string copyToProtocolName, string pluginsPath = "Assets/Plugins/GameDesigner/")
    {
        var rootPath = "Packages/com.gamedesigner.network";//���ĸ�·��
        if (!Directory.Exists(rootPath))
            rootPath = Application.dataPath + "/GameDesigner";//ֱ�ӷ�AssetsĿ¼��·��
        if (!Directory.Exists(rootPath))
        {
            Debug.LogError("�Ҳ�����·��, �޷�ִ��, ��ʹ�ð����������gdnet, ���߸�·��������AssetsĿ¼��!");
            return;
        }
        var path = $"{rootPath}/{sourceProtocolName}/";
        if (!Directory.Exists(path))
        {
            Debug.LogError("�Ҳ���·��:" + path);
            return;
        }
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var item = file.Split(new string[] { sourceProtocolName }, StringSplitOptions.RemoveEmptyEntries);
            var newPath = $"{pluginsPath}{copyToProtocolName}/{item[1]}";
            var path1 = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(path1))
                Directory.CreateDirectory(path1);
            File.Copy(file, newPath, true);
        }
        Debug.Log($"����{Path.GetFileName(copyToProtocolName)}���!");
        AssetDatabase.Refresh();
    }

    private static void ReverseImport(string sourceProtocolName, string copyToProtocolName, string pluginsPath = "Assets/Plugins/GameDesigner/")
    {
        var rootPath = "Packages/com.gamedesigner.network";//���ĸ�·��
        if (!Directory.Exists(rootPath))
            rootPath = Application.dataPath + "/GameDesigner";//ֱ�ӷ�AssetsĿ¼��·��
        if (!Directory.Exists(rootPath))
        {
            Debug.LogError("�Ҳ�����·��, �޷�ִ��, ��ʹ�ð����������gdnet, ���߸�·��������AssetsĿ¼��!");
            return;
        }
        var path = $"{pluginsPath}{copyToProtocolName}/";
        if (!Directory.Exists(path))
        {
            Debug.LogError("�Ҳ�������·��!");
            return;
        }
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var newFile = file.Replace(path, "");
            var newPath = $"{rootPath}/{sourceProtocolName}/{newFile}";
            File.Copy(file, newPath, true);
        }
        Debug.Log($"������{Path.GetFileName(copyToProtocolName)}���!");
        AssetDatabase.Refresh();
    }
}
#endif