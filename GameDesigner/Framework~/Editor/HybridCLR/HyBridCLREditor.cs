using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HyBridCLREditor
{
    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
        "UniTask.dll",
    };

    [MenuItem("GameDesigner/Framework/BuildDll_Copy_To_Hotfix", priority = 3)]
    public static void BuildAndCopyABAOTHotUpdateDlls()
    {
        CompileDllCommand.CompileDllActiveBuildTarget();
        string hotfixAssembliesDstDir = "AssetBundles/Hotfix/";
        CopyHotUpdateAssembliesToStreamingAssets(hotfixAssembliesDstDir);
    }

    [MenuItem("GameDesigner/Framework/BuildDll_Copy_To_Build", priority = 4)]
    public static void BuildDll_Copy_BuildPath()
    {
        CompileDllCommand.CompileDllActiveBuildTarget();
        string hotfixAssembliesDstDir = $"build/{Application.productName}_Data/StreamingAssets/AssetBundles/Hotfix/";
        CopyHotUpdateAssembliesToStreamingAssets(hotfixAssembliesDstDir);
    }

    public static void CopyHotUpdateAssembliesToStreamingAssets(string hotfixAssembliesDstDir)
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        if (!Directory.Exists(hotfixAssembliesDstDir))
            Directory.CreateDirectory(hotfixAssembliesDstDir);
        AOTMetaAssemblyNames.AddRange(SettingsUtil.HotUpdateAssemblyFiles);
        var xml = new XmlDocument();
        xml.Load("Assembly-CSharp.csproj");
        XmlNodeList node_list;
        var namespaceURI = xml.DocumentElement.NamespaceURI;
        if (!string.IsNullOrEmpty(namespaceURI))
        {
            var nsMgr = new XmlNamespaceManager(xml.NameTable); nsMgr.AddNamespace("ns", namespaceURI);
            node_list = xml.SelectNodes("/ns:Project/ns:ItemGroup", nsMgr);
        }
        else node_list = xml.SelectNodes("/Project/ItemGroup");
        foreach (var dll in AOTMetaAssemblyNames)
        {
            string dllPath = $"{hotfixDllSrcDir}/{dll}";
            if (!File.Exists(dllPath))
            {
                for (int i = 0; i < node_list.Count; i++)
                {
                    var node = node_list.Item(i);
                    var node_child = node.ChildNodes;
                    foreach (XmlNode child_node in node_child)
                    {
                        if (child_node.LocalName != "Reference")
                            continue;
                        var value = child_node.InnerText;
                        var name = Path.GetFileName(value);
                        if (name == dll)
                        {
                            dllPath = value;
                            goto J;
                        }
                    }
                }
            J:;
            }
            string dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
            File.Copy(dllPath, dllBytesPath, true);
            Debug.Log($"复制热更新dll到热更新目录: {dllPath} -> {dllBytesPath}");
        }
    }
}
