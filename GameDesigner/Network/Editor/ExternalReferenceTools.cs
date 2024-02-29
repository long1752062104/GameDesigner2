#if UNITY_EDITOR
using Net.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ExternalReferenceTools : EditorWindow
{
    private static ExternalReferencesConfig config;
    private static ExternalReferencesConfig Config
    {
        get
        {
            if (config == null)
                config = PersistHelper.Deserialize<ExternalReferencesConfig>("external References.json");
            return config;
        }
        set { config = value; }
    }
    private Vector2 scrollPosition, scrollPosition1;
    private ReorderableListHandler csprojPathList;
    private ReorderableListHandler dataPathList;

    [MenuItem("GameDesigner/Network/ExternalReference")]
    static void ShowWindow()
    {
        var window = GetWindow<ExternalReferenceTools>("多个项目外部引用工具");
        window.Show();
    }
    private void OnEnable()
    {
        LoadData();
        csprojPathList = new ReorderableListHandler(Config.ProjPaths);
        dataPathList = new ReorderableListHandler(Config.DataPaths);
    }

    private void OnDisable()
    {
        SaveData();
    }

    public class ReorderableListHandler
    {
        public List<CsprojData> paths = new List<CsprojData>();
        private readonly ReorderableList pathList;

        public ReorderableListHandler(List<CsprojData> paths)
        {
            this.paths = paths;
            pathList = new ReorderableList(paths, typeof(CsprojData), true, false, false, false)
            {
                elementHeightCallback = OnElementHeightCallback,
                drawElementCallback = OnDrawElementCallback
            };
        }

        public void DoLayoutList()
        {
            pathList.DoLayoutList();
        }

        private float OnElementHeightCallback(int index)
        {
            return 40f;
        }

        private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var data = paths[index];
            EditorGUI.BeginChangeCheck();
            data.path = EditorGUI.TextField(new Rect(rect) { width = rect.width - 30f, height = 20f }, "C#项目文件:", data.path, GUI.skin.label);
            data.extraPath = EditorGUI.TextField(new Rect(rect) { width = rect.width - 30f, height = 20f, y = rect.y + 20f }, "根路径:", data.extraPath);
            if (GUI.Button(new Rect(rect) { width = 25, height = 30f, x = rect.width - 3f, y = rect.y + 5f, }, "x"))
            {
                paths.RemoveAt(index);
                SaveData();
            }
            if (EditorGUI.EndChangeCheck())
            {
                SaveData();
            }
        }
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("项目文件列表:");
        if (GUILayout.Button("选择文件", GUILayout.Width(100)))
        {
            var csprojPath = EditorUtility.OpenFilePanel("选择文件", "", "csproj");
            if (!string.IsNullOrEmpty(csprojPath))
            {
                //相对于Assets路径
                var path = PathHelper.GetRelativePath(Application.dataPath, csprojPath);
                if (!Config.ProjPaths.Contains(new CsprojData(path)))
                    Config.ProjPaths.Add(new CsprojData(path));
            }
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.MaxHeight(position.height / 2));
        csprojPathList.DoLayoutList();
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("引用文件夹列表:");
        if (GUILayout.Button("添加引用路径", GUILayout.Width(100)))
        {
            var dataPath = EditorUtility.OpenFolderPanel("引用路径", "", "");
            if (!string.IsNullOrEmpty(dataPath))
            {
                //相对于Assets路径
                var path = PathHelper.GetRelativePath(Application.dataPath, dataPath);
                if (!Config.DataPaths.Contains(new CsprojData(path)))
                    Config.DataPaths.Add(new CsprojData(path));
            }
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, false, true, GUILayout.MaxHeight(position.height / 2));
        dataPathList.DoLayoutList();
        GUILayout.EndScrollView();
        Config.searchPattern = EditorGUILayout.DelayedTextField("引用文件格式:", Config.searchPattern);
        if (GUILayout.Button("执行!", GUILayout.Height(30)))
        {
            foreach (var csprojPath in Config.ProjPaths)
            {
                var xml = new XmlDocument();
                xml.Load(csprojPath.path);
                ChangeCSProject(xml, csprojPath, Config.searchPattern, Config.DataPaths);
                xml.Save(csprojPath.path);
            }
            Debug.Log("更新完成!");
        }
        if (EditorGUI.EndChangeCheck())
            SaveData();
    }

    public static string ChangeCSProject(string genCsProjectPath, string content)
    {
        foreach (var csprojPath in Config.ProjPaths)
        {
            var name = Path.GetFileName(csprojPath.path);
            var name1 = Path.GetFileName(genCsProjectPath);
            if (name != name1)
                continue;
            var xml = new XmlDocument();
            xml.LoadXml(content);
            return ChangeCSProject(xml, csprojPath, Config.searchPattern, Config.DataPaths);
        }
        return content;
    }

    private static string ChangeCSProject(XmlDocument xml, CsprojData csprojPath, string searchPattern, List<CsprojData> paths)
    {
        XmlNodeList node_list;
        var documentElement = xml.DocumentElement;
        var namespaceURI = xml.DocumentElement.NamespaceURI;
        if (!string.IsNullOrEmpty(namespaceURI))
        {
            var nsMgr = new XmlNamespaceManager(xml.NameTable);
            nsMgr.AddNamespace("ns", namespaceURI);
            node_list = xml.SelectNodes("/ns:Project/ns:ItemGroup", nsMgr);
        }
        else node_list = xml.SelectNodes("/Project/ItemGroup");
        foreach (var path in paths)
        {
            var path1 = Path.GetFullPath(path.path);
            var dir = new DirectoryInfo(path1);
            var dirName = dir.Parent.FullName + Path.DirectorySeparatorChar;
            var files = Directory.GetFiles(path1, "*.*", SearchOption.AllDirectories);
            var patterns = searchPattern.Replace("*", "").Split('|');
            var fileList = new List<string>();
            //相对于服务器路径
            var csPath = Path.GetFullPath(csprojPath.path);
            path1 = PathHelper.GetRelativePath(csPath, path1, true);
            //只包含三级目录的相对路径
            dirName = PathHelper.GetRelativePath(csPath, dirName, true);
            foreach (var file in files)
            {
                foreach (var pattern in patterns)
                {
                    if (string.IsNullOrEmpty(pattern))
                        continue;
                    if (file.EndsWith(pattern))
                    {
                        var rPath = PathHelper.GetRelativePath(csPath, file, true);
                        fileList.Add(rPath);
                        break;
                    }
                }
            }
            files = fileList.ToArray();
            var extraPath = $"{csprojPath.extraPath}{Path.DirectorySeparatorChar}{path.extraPath}{Path.DirectorySeparatorChar}";
            while (extraPath.StartsWith(Path.DirectorySeparatorChar))
                extraPath = extraPath.TrimStart(Path.DirectorySeparatorChar);
            var exist = CheckNodeContains(xml, node_list, namespaceURI, dirName, path1, files, extraPath);
            if (!exist)
            {
                var node = CreateItemGroup(xml, namespaceURI, dirName, files, extraPath);
                documentElement.AppendChild(node);
            }
        }
        var settings = new XmlWriterSettings
        {
            Indent = true,          // 启用缩进
            NewLineChars = "\n",    // 设置换行符
            NewLineHandling = NewLineHandling.Replace,
            Encoding = System.Text.Encoding.UTF8,
            ConformanceLevel = ConformanceLevel.Document // 设置符合级别
        };
        using (var sw = new StringWriter())
        {
            using (XmlWriter xmlWriter = XmlWriter.Create(sw, settings))
            {
                xml.WriteTo(xmlWriter);
            }
            return sw.ToString();
        }
    }

    private static bool CheckNodeContains(XmlDocument xml, XmlNodeList node_list, string namespaceURI, string dirName, string path1, string[] files, string extraPath)
    {
        bool exist = false;
        for (int i = 0; i < node_list.Count; i++)
        {
            var node = node_list.Item(i);
            var node_child = node.ChildNodes;
            foreach (XmlNode child_node in node_child)
            {
                if (child_node.LocalName != "Compile" & child_node.LocalName != "Content")
                    continue;
                var attribute = child_node.Attributes.GetNamedItem("Include");
                if (attribute == null)
                    continue;
                var value = attribute.Value;
                if (value.Contains(path1))
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
                continue;
            foreach (var file in files)
            {
                bool isExist = false;
                foreach (XmlNode child_node in node_child)
                {
                    if (child_node.LocalName != "Compile" & child_node.LocalName != "Content")
                        continue;
                    var attribute = child_node.Attributes.GetNamedItem("Include");
                    if (attribute == null)
                        continue;
                    var value = attribute.Value;
                    if (file == value) //必须一致性
                    {
                        isExist = true;
                        break;
                    }
                }
                if (!isExist)
                    CreateItemElement(xml, node, file, namespaceURI, dirName, extraPath);
            }
            foreach (XmlNode child_node in node_child) //检查移除的文件
            {
                if (child_node.LocalName != "Compile" & child_node.LocalName != "Content")
                    continue;
                var attribute = child_node.Attributes.GetNamedItem("Include");
                if (attribute == null)
                    continue;
                var value = attribute.Value;
                if (!files.Contains(value))
                    node.RemoveChild(child_node);
            }
            break;
        }
        return exist;
    }

    private static XmlNode CreateItemGroup(XmlDocument xml, string namespaceURI, string dirName, string[] files, string extraPath)
    {
        var node = xml.CreateElement("ItemGroup", namespaceURI);
        foreach (var file in files)
            CreateItemElement(xml, node, file, namespaceURI, dirName, extraPath);
        return node;
    }

    private static void CreateItemElement(XmlDocument xml, XmlNode node, string file, string namespaceURI, string dirName, string extraPath)
    {
        if (file.EndsWith(".cs"))
        {
            var e = xml.CreateElement("Compile", namespaceURI);
            e.SetAttribute("Include", file);
            var innerText = file.Replace(dirName, "");
            if (file != innerText) //相同的目录就不需要link了
            {
                var e1 = xml.CreateElement("Link", namespaceURI);
                e1.InnerText = extraPath + innerText;
                e.AppendChild(e1);
            }
            node.AppendChild(e);
        }
        else
        {
            var e = xml.CreateElement("Content", namespaceURI);
            e.SetAttribute("Include", file);
            var innerText = file.Replace(dirName, "");
            if (file != innerText) //相同的目录就不需要link了
            {
                var e1 = xml.CreateElement("Link", namespaceURI);
                e1.InnerText = extraPath + innerText;
                e.AppendChild(e1);
            }
            node.AppendChild(e);
        }
    }

    static void LoadData()
    {
        Config = PersistHelper.Deserialize<ExternalReferencesConfig>("external References.json");
    }

    static void SaveData()
    {
        PersistHelper.Serialize(Config, "external References.json");
    }

    public class ExternalReferencesConfig
    {
        public List<CsprojData> ProjPaths = new List<CsprojData>();
        public List<CsprojData> DataPaths = new List<CsprojData>();
        public string searchPattern = "*.cs|*.txt|*.ini|*.conf|*.xls|*.xlsx|*.json";
    }

    [Serializable]
    public class CsprojData
    {
        public string path;
        public string extraPath;

        public CsprojData() { }
        public CsprojData(string path)
        {
            this.path = path;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is CsprojData data)
                return data.path == path;
            return false;
        }
    }
}
#endif