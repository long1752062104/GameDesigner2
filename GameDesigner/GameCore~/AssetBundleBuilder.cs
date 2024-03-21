#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameCore
{
    public enum CollectType
    {
        TopDirectoryOnly,
        TopDirectoryOnlyHashName,
        AllDirectories,
        AllDirectoriesHashName,
        AllDirectoriesSplit,
        AllDirectoriesSplitHashName,
    }

    public enum CompressOptions
    {
        Uncompressed = 0,
        StandardCompression,
        ChunkBasedCompression,
    }

    [Flags]
    public enum FilterOptions
    {
        None = 0,
        AnimationClip = 1 << 1,
        AudioClip = 1 << 2,
        AudioMixer = 1 << 3,
        ComputeShader = 1 << 4,
        Font = 1 << 5,
        GUISkin = 1 << 6,
        Material = 1 << 7,
        Mesh = 1 << 8,
        Model = 1 << 9,
        PhysicMaterial = 1 << 10,
        Prefab = 1 << 11,
        Scene = 1 << 12,
        Script = 1 << 13,
        Shader = 1 << 14,
        Sprite = 1 << 15,
        Texture = 1 << 16,
        VideoClip = 1 << 17,
    }

    [Serializable]
    public class AssetBundlePackage
    {
        public string name;
        public bool enable = true;
        public CollectType type;
        public Object path;
        public FilterOptions filter;
        public List<Object> filters = new List<Object>();
    }

    public class AssetBundleBuilder : ScriptableObject
    {
        public BuildTarget buildTarget;
        public string version = "1.0.0";
        public bool autoIncrement = false;
        public string outputPath = "AssetBundles/";
        public bool clearFolders = true;
        public bool copyToStreamingAssets;
        public bool clearManifestFile = true;
        [Tooltip("Uncompressed: 不压缩 StandardCompression:正常压缩 ChunkBasedCompression: 使用基于语块的LZ4压缩")]
        public CompressOptions compression = CompressOptions.ChunkBasedCompression;
        [Tooltip("打包AssetBundle时不包含类型信息，这会使文件变小并稍微快一些加载时间。")]
        public bool ExcludeTypeInformation;
        [Tooltip("强制重新构建AssetBundle")]
        public bool ForceRebuild = true;
        [Tooltip("在进行增量构建检查时，会忽略类型树的变化。")]
        public bool IgnoreTypeTreeChanges;
        [Tooltip("打包AssetBundle时追加hash字符串")]
        public bool AppendHash;
        [Tooltip("当你使用这个选项时，如果在构建过程中出现任何错误，构建将立即失败。")]
        public bool StrictMode;
        [Tooltip("当你使用这个选项时，会进行一个干运行的构建。这意味着，构建过程不会实际生成任何AssetBundle文件，而是会检查是否有任何需要重新构建的AssetBundle，并返回这些AssetBundle的列表。这对于检查哪些AssetBundle需要重新构建非常有用，而不会消耗实际的构建时间")]
        public bool DryRunBuild;
        [Tooltip("AssetBundle文件格式后缀")]
        public string packageFormat;
        public bool encrypt;
        public int password = 154789548;
        [Tooltip("AssetBundle文件如果超过理想大小, 则进行分包")]
        public long singlePackageSize = 1024 * 1024 * 20; //单包最大20m
        public bool compressionJson;
        [Tooltip("使用首包压缩文件, 当项目有上千个AB文件时, 玩家第一次下载游戏, 只需要下载这个压缩文件下来,然后解压即可. 如果不使用首包, 则需要一个个AB文件下载")]
        public bool useFirstPackage = true;
        [Tooltip("首包压缩文件复制到流路径，这样可以跟着apk一起打包，就不需要下载首包了")]
        public bool firstPackageCopyToStreamingAssets = false;
        public List<AssetBundlePackage> Packages = new List<AssetBundlePackage>();
        public string tablePath = "Assets/Arts/Table";
        public string tableScriptPath = "Assets/Scripts/Data/Config";
        public string hotfixPath = "Assets/Arts/Hotfix";
        [Tooltip("资源加载仅使用资源名，当你使用这个选项时，加载资源时不需要资源路径和后缀；不开启时，使用完整路径加载资源")]
        public bool ResNameNotPath;
        [Header("补充元数据")]
        public List<string> AOTMetaAssemblyNames = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
            "UniTask.dll",
        };
        public string MainAssemblyName = "Main.dll";
        public bool copyPdb = true;
        private static AssetBundleBuilder instance;
        public static AssetBundleBuilder Instance
        {
            get
            {
                if (instance == null)
                {
                    var guids = AssetDatabase.FindAssets("AssetBundleBuilder");
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var assetBundleBuilder = AssetDatabase.LoadAssetAtPath<AssetBundleBuilder>(path);
                        if (assetBundleBuilder != null)
                        {
                            instance = assetBundleBuilder;
                            break;
                        }
                    }
                }
                return instance;
            }
        }

        [MenuItem("Assets/GameCore/Create AssetBundleBuilder")]
        public static void CreateAssetBundleBuilder()
        {
            var assetBundleBuilder = CreateInstance<AssetBundleBuilder>();
            assetBundleBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
                path = "Assets";
            else if (Path.GetExtension(path) != "")
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/AssetBundleBuilder.asset");
            AssetDatabase.CreateAsset(assetBundleBuilder, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = assetBundleBuilder;
        }
    }
}
#endif