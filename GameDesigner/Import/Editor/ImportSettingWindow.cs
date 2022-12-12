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
        EditorGUILayout.LabelField("����ģ��:");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        EditorGUILayout.HelpBox("Tcp&Gcpģ�� ��������Э��ģ��", MessageType.Info);
        var path = "Assets/Plugins/GameDesigner/Network/Gcp";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�Gcpģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����Gcpģ��"))
        {
            Import("Network/Gcp~", "Network/Gcp");
        }
        EditorGUILayout.HelpBox("Udxģ�� ������֡ͬ������Ƶ����ֱ�����������ݴ���", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Network/Udx";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�Udxģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����Udxģ��"))
        {
            Import("Network/Udx~", "Network/Udx");
        }
        EditorGUILayout.HelpBox("Kcpģ�� ������֡ͬ�� ��ʱ��Ϸ", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Network/Kcp";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�Kcpģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����Kcpģ��"))
        {
            Import("Network/Kcp~", "Network/Kcp");
        }
        EditorGUILayout.HelpBox("Webģ�� ��������ҳ��Ϸ WebGL", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Network/Web";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�Webģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����Webģ��"))
        {
            Import("Network/Web~", "Network/Web");
        }
        EditorGUILayout.HelpBox("StateMachineģ�� ���ø���Ϸ���������Ϸ�������", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/StateMachine";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�StateMachineģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����StateMachineģ��"))
        {
            Import("StateMachine~", "StateMachine");
        }
        EditorGUILayout.HelpBox("NetworkComponentsģ�� ��װ��һ�������Ŀͻ����������", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Component";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�Network���"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����Network���"))
        {
            Import("Common~", "Common");//����
            Import("Component~", "Component");
        }
        EditorGUILayout.HelpBox("MVCģ�� ������֡ͬ����ƣ���ͼ���߼�����", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/MVC";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�MVCģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����MVCģ��"))
        {
            Import("MVC~", "MVC");
        }
        EditorGUILayout.HelpBox("ECSģ�� ������˫�˵Ķ�����������", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/ECS";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�ECSģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����ECSģ��"))
        {
            Import("ECS~", "ECS");
        }
        EditorGUILayout.HelpBox("Commonģ�� ͨ��ģ��", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/Common";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�Commonģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����Commonģ��"))
        {
            Import("Common~", "Common");
        }
        EditorGUILayout.HelpBox("AOIģ�� ������MMORPG���ͼͬ���������Ź���ͬ���� ���ߵ������ͼ�ָ���ʾ", MessageType.Info);
        path = "Assets/Plugins/GameDesigner/AOI";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ�AOIģ��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����AOIģ��"))
        {
            Import("AOI~", "AOI");
        }
        EditorGUILayout.HelpBox("Frameworkģ�� �ͻ��˿��, �����ȸ��£�Excel����Globalȫ�ֹ�����������", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("HyBridCLR��ַ,���ػ�Git�����"))
        {
            Application.OpenURL(@"https://gitee.com/focus-creative-games/hybridclr_unity");
        }
        path = "Assets/Plugins/GameDesigner/Framework";
        if (Directory.Exists(path))
        {
            GUI.color = Color.red;
            if (GUILayout.Button("�Ƴ��ͻ��˿��"))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            GUI.color = Color.white;
        }
        else if (GUILayout.Button("����ͻ��˿��"))
        {
            Import("Framework~", "Framework");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("����ģ�鵼��", MessageType.Warning);
        if (GUILayout.Button("����ģ�鵼��", GUILayout.Height(20)))
        {
            Import("Network/Gcp~", "Network/Gcp");
            Import("Component~", "Component");
            Import("Common~", "Common");
        }
        EditorGUILayout.HelpBox("����ģ�鵼��", MessageType.Warning);
        if (GUILayout.Button("����ģ�鵼��", GUILayout.Height(20)))
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
        EditorGUILayout.HelpBox("���а������룬����ѧϰ�Ϳ�������", MessageType.Warning);
        if (GUILayout.Button("��������", GUILayout.Height(20)))
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

    private static void Import(string sourceProtocolName, string copyToProtocolName, string pluginsPath = "Assets/Plugins/GameDesigner/")
    {
        var path = $"Packages/com.gamedesigner.network/{sourceProtocolName}/";
        if (!Directory.Exists(path))
        {
            Debug.LogError("�Ҳ���·��:" + path);
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