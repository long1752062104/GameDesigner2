#if UNITY_EDITOR
using Net.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
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

    [MenuItem("GameDesigner/Network/ExternalReference")]
    static void ShowWindow()
    {
        var window = GetWindow<ExternalReferenceTools>("多个项目外部引用工具");
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
                Config.csprojPaths.Add(PathHelper.GetRelativePath(Application.dataPath, csprojPath));
            }
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.MaxHeight(position.height / 2));
        foreach (var path in Config.csprojPaths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                Config.csprojPaths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("引用文件夹列表:");
        if (GUILayout.Button("添加引用路径", GUILayout.Width(100)))
        {
            var path = EditorUtility.OpenFolderPanel("引用路径", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (!Config.paths.Contains(path))
                {
                    //相对于Assets路径
                    Config.paths.Add(PathHelper.GetRelativePath(Application.dataPath, path));
                }
            }
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, false, true, GUILayout.MaxHeight(position.height / 2));
        foreach (var path in Config.paths)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(rect.position, rect.size - new Vector2(50, 0)), path);
            if (GUI.Button(new Rect(rect.position + new Vector2(position.width - 50, 0), new Vector2(20, rect.height)), "x"))
            {
                Config.paths.Remove(path);
                SaveData();
                return;
            }
        }
        GUILayout.EndScrollView();
        Config.searchPattern = EditorGUILayout.DelayedTextField("引用文件格式:", Config.searchPattern);
        if (GUILayout.Button("执行!", GUILayout.Height(30)))
        {
            foreach (var csprojPath in Config.csprojPaths)
            {
                var xml = new XmlDocument();
                xml.Load(csprojPath);
                ChangeCSProject(xml, csprojPath, Config.searchPattern, Config.paths);
                xml.Save(csprojPath);
            }
            Debug.Log("更新完成!");
        }
        if (GUILayout.Button("编译dll!", GUILayout.Height(30)))
        {
            
        }
        if (EditorGUI.EndChangeCheck())
            SaveData();
    }

    public static string ChangeCSProject(string genCsProjectPath, string content)
    {
        foreach (var csprojPath in Config.csprojPaths)
        {
            var name = Path.GetFileName(csprojPath);
            var name1 = Path.GetFileName(genCsProjectPath);
            if (name != name1)
                continue;
            var xml = new XmlDocument();
            xml.LoadXml(content);
            return ChangeCSProject(xml, csprojPath, Config.searchPattern, Config.paths);
        }
        return content;
    }

    private static string ChangeCSProject(XmlDocument xml, string csprojPath, string searchPattern, List<string> paths)
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
            var path1 = Path.GetFullPath(path);
            var dir = new DirectoryInfo(path1);
            var dirName = dir.Parent.FullName + Path.DirectorySeparatorChar;
            var files = Directory.GetFiles(path1, "*.*", SearchOption.AllDirectories);
            var patterns = searchPattern.Replace("*", "").Split('|');
            var fileList = new List<string>();
            //相对于服务器路径
            var csPath = Path.GetFullPath(csprojPath);
            path1 = PathHelper.GetRelativePath(csPath, path1, true);
            //只包含三级目录的相对路径
            dirName = PathHelper.GetRelativePath(csPath, dirName, true);
            foreach (var file in files)
            {
                foreach (var pattern in patterns)
                {
                    if (file.EndsWith(pattern))
                    {
                        var rPath = PathHelper.GetRelativePath(csPath, file, true);
                        fileList.Add(rPath);
                        break;
                    }
                }
            }
            files = fileList.ToArray();
            var exist = CheckNodeContains(xml, node_list, namespaceURI, dirName, path1, files);
            if (!exist)
            {
                var node = CreateItemGroup(xml, namespaceURI, dirName, files);
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

    private static bool CheckNodeContains(XmlDocument xml, XmlNodeList node_list, string namespaceURI, string dirName, string path1, string[] files)
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
                    CreateItemElement(xml, node, file, namespaceURI, dirName);
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

    private static XmlNode CreateItemGroup(XmlDocument xml, string namespaceURI, string dirName, string[] files)
    {
        var node = xml.CreateElement("ItemGroup", namespaceURI);
        foreach (var file in files)
            CreateItemElement(xml, node, file, namespaceURI, dirName);
        return node;
    }

    private static void CreateItemElement(XmlDocument xml, XmlNode node, string file, string namespaceURI, string dirName)
    {
        if (file.EndsWith(".cs"))
        {
            var e = xml.CreateElement("Compile", namespaceURI);
            e.SetAttribute("Include", file);
            var innerText = file.Replace(dirName, "");
            if (file != innerText) //相同的目录就不需要link了
            {
                var e1 = xml.CreateElement("Link", namespaceURI);
                e1.InnerText = innerText;
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
                e1.InnerText = innerText;
                e.AppendChild(e1);
            }
            node.AppendChild(e);
        }
    }

    void LoadData()
    {
        Config = PersistHelper.Deserialize<ExternalReferencesConfig>("external References.json");
    }

    void SaveData()
    {
        PersistHelper.Serialize(Config, "external References.json");
    }

    public class ExternalReferencesConfig
    {
        public List<string> csprojPaths = new List<string>();
        public List<string> paths = new List<string>();
        public string searchPattern = "*.cs|*.txt|*.ini|*.conf|*.xls|*.xlsx|*.json";
    }
}
#endif