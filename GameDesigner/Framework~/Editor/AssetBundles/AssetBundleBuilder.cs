#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Text;
#if HYBRIDCLR
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
#endif

namespace Framework
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
        public List<AssetBundlePackage> Packages = new List<AssetBundlePackage>();
        public string tablePath = "Assets/Arts/Table";
        public string tableScriptPath = "Assets/Scripts/Data/Config";
        public string hotfixPath = "Assets/Arts/Hotfix";
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

        [MenuItem("Assets/Framework/Create AssetBundleBuilder")]
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

    [CustomEditor(typeof(AssetBundleBuilder))]
    public class AssetBundleBuilderEditor : Editor
    {
        private AssetBundleBuilder assetBundleBuilder;
        private readonly Dictionary<FilterOptions, string[]> filterOptionDict = new()
        {
            { FilterOptions.AnimationClip, new string[]{ ".anim" } },
            { FilterOptions.AudioClip, new string[]{ ".ogg", ".wav", ".mp3" } },
            { FilterOptions.AudioMixer, new string[]{ ".mixer" } },
            { FilterOptions.ComputeShader, new string[]{ ".compute" } },
            { FilterOptions.Font, new string[]{ ".ttf", ".fontsettings", ".TTF" } },
            { FilterOptions.GUISkin, new string[]{ ".guiskin" } },
            { FilterOptions.Material, new string[]{ ".mat" } },
            { FilterOptions.Mesh, new string[]{ ".fbx", ".FBX", ".obj" } },
            { FilterOptions.Model, new string[]{ ".fbx", ".FBX", ".obj" } },
            { FilterOptions.PhysicMaterial, new string[]{ ".physicMaterial" } },
            { FilterOptions.Prefab, new string[]{ ".prefab" } },
            { FilterOptions.Scene, new string[]{ ".unity" } },
            { FilterOptions.Script, new string[]{ ".cs" } },
            { FilterOptions.Shader, new string[]{ ".shader" } },
            { FilterOptions.Sprite, new string[]{ ".jpg", ".png", ".bmp", ".tiff", ".psd", ".svg", ".jpeg" } },
            { FilterOptions.Texture, new string[]{ ".jpg", ".png", ".bmp", ".tiff", ".psd", ".svg", ".jpeg" } },
            { FilterOptions.VideoClip, new string[]{ ".mp4", ".avi", ".mkv", ".flv", ".wmv" } },
        };

        private void OnEnable()
        {
            assetBundleBuilder = target as AssetBundleBuilder;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
#if HYBRIDCLR
            if (GUILayout.Button("BuildAssembly"))
            {
                CompileDllCommand.CompileDllActiveBuildTarget();
                CopyHotUpdateAssembliesToStreamingAssets(assetBundleBuilder.hotfixPath);
            }
#endif
            if (GUILayout.Button("BuildGameConfig"))
            {
                ExcelTools.GenerateExcelData();
            }
            if (GUILayout.Button("BuildGameConfigScript"))
            {
                ExcelTools.GenerateExcelDataAll();
            }
            if (GUILayout.Button("BuildAssetBundle"))
            {
                if (assetBundleBuilder.clearFolders)
                {
                    if (Directory.Exists(assetBundleBuilder.outputPath))
                        Directory.Delete(assetBundleBuilder.outputPath, true);
                    if (assetBundleBuilder.copyToStreamingAssets)
                    {
                        var m_streamingPath = Application.streamingAssetsPath + "/AssetBundles/";
                        if (Directory.Exists(m_streamingPath))
                            Directory.Delete(m_streamingPath, true);
                    }
                }
                if (!Directory.Exists(assetBundleBuilder.outputPath))
                    Directory.CreateDirectory(assetBundleBuilder.outputPath);
                var opt = BuildAssetBundleOptions.None;
                if (assetBundleBuilder.compression == CompressOptions.Uncompressed)
                    opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
                else if (assetBundleBuilder.compression == CompressOptions.ChunkBasedCompression)
                    opt |= BuildAssetBundleOptions.ChunkBasedCompression;
                if (assetBundleBuilder.ExcludeTypeInformation)
                    opt |= BuildAssetBundleOptions.DisableWriteTypeTree;
                if (assetBundleBuilder.ForceRebuild)
                    opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                if (assetBundleBuilder.IgnoreTypeTreeChanges)
                    opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
                if (assetBundleBuilder.AppendHash)
                    opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
                if (assetBundleBuilder.StrictMode)
                    opt |= BuildAssetBundleOptions.StrictMode;
                if (assetBundleBuilder.DryRunBuild)
                    opt |= BuildAssetBundleOptions.DryRunBuild;
                var buildList = new Dictionary<string, AssetBundleBuild>();
                var assetInfoList = new Dictionary<string, AssetInfo>();
                string[] files;
                for (int i = 0; i < assetBundleBuilder.Packages.Count; i++)
                {
                    var package = assetBundleBuilder.Packages[i];
                    if (!package.enable)
                        continue;
                    var assetPath = AssetDatabase.GetAssetPath(package.path);
                    var isFolder = AssetDatabase.IsValidFolder(assetPath);
                    if (isFolder)
                        AssetBundleCollect(buildList, assetInfoList, assetPath, package, i / (float)assetBundleBuilder.Packages.Count);
                    else
                        AddSinglePackage(buildList, assetInfoList, assetPath, package);
                }
                EditorUtility.ClearProgressBar();
                Debug.Log($"收集的资源包数量:{buildList.Count} 资源文件数量:{assetInfoList.Count}");
                var outputPath = $"{assetBundleBuilder.outputPath}/{assetBundleBuilder.buildTarget}/{assetBundleBuilder.version}/";
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);
                var assetInfoListPath = outputPath + "assetInfoList.json";
                var assetInfoListSource = new Dictionary<string, AssetInfo>();
                if (File.Exists(assetInfoListPath))
                {
                    var assetInfoListBytes = File.ReadAllBytes(assetInfoListPath);
                    if (assetBundleBuilder.compressionJson)
                        assetInfoListBytes = Net.Helper.UnZipHelper.Decompress(assetInfoListBytes);
                    var assetInfoListJson = Encoding.UTF8.GetString(assetInfoListBytes);
                    assetInfoListSource = Newtonsoft_X.Json.JsonConvert.DeserializeObject<Dictionary<string, AssetInfo>>(assetInfoListJson);
                }
                var assetBundleBuildList = new Dictionary<string, AssetBundleBuild>();
                foreach (var assetInfo in assetInfoList)
                {
                    if (assetInfoListSource.TryGetValue(assetInfo.Key, out var assetInfo1))
                        if (assetInfo.Value.md5 == assetInfo1.md5)
                            continue;
                    if (buildList.TryGetValue(assetInfo.Value.assetBundleName, out var assetBundleBuild))
                        assetBundleBuildList[assetInfo.Value.assetBundleName] = assetBundleBuild;
                }
                Debug.Log($"筛选后要构建的资源包数量:{assetBundleBuildList.Count}");
                if (assetBundleBuildList.Count == 0)
                {
                    Debug.Log("没有增量更新包!");
                    return;
                }
                var assetBundleManifest = BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuildList.Values.ToArray(), opt, assetBundleBuilder.buildTarget);
                var manifestPath = outputPath + "assetBundleManifest.json";
                var assetManifest = new AssetManifest();
                if (File.Exists(manifestPath))
                {
                    var manifestBytes = File.ReadAllBytes(manifestPath);
                    if (assetBundleBuilder.compressionJson)
                        manifestBytes = Net.Helper.UnZipHelper.Decompress(manifestBytes);
                    var manifestJson = Encoding.UTF8.GetString(manifestBytes);
                    assetManifest = Newtonsoft_X.Json.JsonConvert.DeserializeObject<AssetManifest>(manifestJson);
                }
                if (assetBundleManifest != null)
                {
                    var allAssetBundles = assetBundleManifest.GetAllAssetBundles();
                    foreach (var allAssetBundle in allAssetBundles)
                        assetManifest.dependencies[allAssetBundle] = assetBundleManifest.GetDirectDependencies(allAssetBundle);
                }
                var json = Newtonsoft_X.Json.JsonConvert.SerializeObject(assetManifest, Newtonsoft_X.Json.Formatting.Indented);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                if (assetBundleBuilder.compressionJson)
                    jsonBytes = Net.Helper.UnZipHelper.Compress(jsonBytes);
                File.WriteAllBytes(outputPath + "assetBundleManifest.json", jsonBytes);
                json = Newtonsoft_X.Json.JsonConvert.SerializeObject(assetInfoList, Newtonsoft_X.Json.Formatting.Indented);
                jsonBytes = Encoding.UTF8.GetBytes(json);
                if (assetBundleBuilder.compressionJson)
                    jsonBytes = Net.Helper.UnZipHelper.Compress(jsonBytes);
                File.WriteAllBytes(outputPath + "assetInfoList.json", jsonBytes);
                var assetBundleInfos = new List<AssetBundleInfo>();
                files = Directory.GetFiles(outputPath, "*.*").Where(s => !s.EndsWith(".meta") && !s.EndsWith(".manifest")).ToArray();
                foreach (var file in files)
                {
                    var bytes = File.ReadAllBytes(file);
                    if (assetBundleBuilder.encrypt)
                    {
                        Net.Helper.EncryptHelper.ToEncrypt(assetBundleBuilder.password, bytes);
                        File.WriteAllBytes(file, bytes);
                        Debug.Log($"加密文件:{file}完成!");
                    }
                    var md5 = Net.Helper.EncryptHelper.GetMD5(bytes);
                    assetBundleInfos.Add(new AssetBundleInfo(Path.GetFileName(file), md5, bytes.Length));
                }
                json = Newtonsoft_X.Json.JsonConvert.SerializeObject(assetBundleInfos, Newtonsoft_X.Json.Formatting.Indented);
                jsonBytes = Encoding.UTF8.GetBytes(json);
                if (assetBundleBuilder.compressionJson)
                    jsonBytes = Net.Helper.UnZipHelper.Compress(jsonBytes);
                File.WriteAllBytes(outputPath + "../" + "version.json", jsonBytes);
                if (assetBundleBuilder.copyToStreamingAssets)
                {
                    var m_streamingPath = Application.streamingAssetsPath + "/AssetBundles/";
                    DirectoryCopy(assetBundleBuilder.outputPath, m_streamingPath);
                }
                CheckAutoIncrement();
                AssetDatabase.Refresh();
                Debug.Log("构建资源完成!");
            }
        }

        private string GetAssetBundleName(string assetPath, CollectType type)
        {
            string assetBundleName;
            if (type == CollectType.AllDirectoriesSplitHashName | type == CollectType.TopDirectoryOnlyHashName | type == CollectType.AllDirectoriesHashName)
                assetBundleName = Net.Helper.EncryptHelper.GetMD5(assetPath);
            else
                assetBundleName = assetPath.Replace("\\", "_").Replace("/", "_").Replace(".", "_").Replace(" ", "").Replace("-", "_").ToLower();
            if (!string.IsNullOrEmpty(assetBundleBuilder.packageFormat))
                assetBundleName += "." + assetBundleBuilder.packageFormat;
            return assetBundleName;
        }

        private void AssetBundleCollect(Dictionary<string, AssetBundleBuild> buildList, Dictionary<string, AssetInfo> assetInfoList, string assetPath, AssetBundlePackage package, float progress)
        {
            EditorUtility.DisplayProgressBar("AssetBundleCollect", $"收集路径:{assetPath}", progress);
            var type = package.type;
            SearchOption searchOption;
            if (type == CollectType.AllDirectories)
                searchOption = SearchOption.AllDirectories;
            else
                searchOption = SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(assetPath, "*.*", searchOption).Where(s =>
            {
                if (s.EndsWith(".meta"))
                    return false;
                if (s.EndsWith(".cs"))
                    return false;
                s = s.Replace('\\', '/');
                foreach (var filterObject in package.filters)
                {
                    var assetPath = AssetDatabase.GetAssetPath(filterObject);
                    if (Net.Helper.StringHelper.StartsWith(s, assetPath))
                        return false;
                }
                foreach (FilterOptions flag in Enum.GetValues(typeof(FilterOptions)))
                {
                    if (!package.filter.HasFlag(flag))
                        continue;
                    if (!filterOptionDict.TryGetValue(flag, out var expands))
                        continue;
                    foreach (var expand in expands)
                        if (s.EndsWith(expand))
                            return false;
                }
                return true;
            }).ToList();
            for (int i = 0; i < files.Count; i++)
            {
                files[i] = files[i].Replace('\\', '/');
                if (files[i].EndsWith(".unity"))
                {
                    var sceneAssetBundleName = AddSinglePackage(buildList, assetInfoList, files[i], package);
                    files.RemoveAt(i);
                    if (i >= 0) i--;
                    EditorUtility.DisplayProgressBar("AssetBundleCollect", $"收集资源包:{sceneAssetBundleName}完成!", progress);
                }
            }
            int count = 1;
            while (files.Count > 0)
            {
                long totalSize = 0;
                var assetBundleName = GetAssetBundleName(assetPath, type);
                int sizeIndex = files.Count;
                for (int i = 0; i < files.Count; i++)
                {
                    totalSize += new FileInfo(files[i]).Length;
                    if (totalSize >= assetBundleBuilder.singlePackageSize)
                    {
                        sizeIndex = i == 0 ? 1 : i; //解决不足1死循环问题
                        assetBundleName += "_" + count++;
                        break;
                    }
                }
                var assetNames = files.GetRange(0, sizeIndex);
                files.RemoveRange(0, sizeIndex);
                for (int i = 0; i < assetNames.Count; i++)
                {
                    var lastModified = File.GetLastWriteTime(assetNames[i]);
                    var lastModified1 = File.GetLastWriteTime(assetNames[i] + ".meta");
                    var md5 = Net.Helper.EncryptHelper.GetMD5($"{lastModified}-{lastModified1}");
                    assetInfoList.Add(assetNames[i], new AssetInfo()
                    {
                        name = Path.GetFileName(assetNames[i]),
                        assetBundleName = assetBundleName,
                        md5 = md5,
                    });
                }
                var assetBundleBuild = new AssetBundleBuild
                {
                    assetBundleName = assetBundleName,
                    assetNames = assetNames.ToArray()
                };
                buildList.Add(assetBundleName, assetBundleBuild);
                EditorUtility.DisplayProgressBar("AssetBundleCollect", $"收集资源包:{assetBundleName}完成!", progress);
            }
            if (type < CollectType.AllDirectoriesSplit)
                return;
            var directories = Directory.GetDirectories(assetPath);
            foreach (var directorie in directories)
            {
                AssetBundleCollect(buildList, assetInfoList, directorie, package, progress);
            }
        }

        private string AddSinglePackage(Dictionary<string, AssetBundleBuild> buildList, Dictionary<string, AssetInfo> assetInfoList, string assetPath, AssetBundlePackage package)
        {
            var sceneAssetBundleName = GetAssetBundleName(assetPath, package.type);
            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = sceneAssetBundleName,
                assetNames = new string[] { assetPath }
            };
            buildList.Add(sceneAssetBundleName, assetBundleBuild);
            var lastModified = File.GetLastWriteTime(assetPath);
            var lastModified1 = File.GetLastWriteTime(assetPath + ".meta");
            var md5 = Net.Helper.EncryptHelper.GetMD5($"{lastModified}-{lastModified1}");
            assetInfoList.Add(assetPath, new AssetInfo()
            {
                name = Path.GetFileName(assetPath),
                assetBundleName = sceneAssetBundleName,
                md5 = md5,
            });
            return sceneAssetBundleName;
        }

        public string CheckAutoIncrement()
        {
            if (assetBundleBuilder.autoIncrement)
            {
                var versions = assetBundleBuilder.version.Split('.');
                var v1s = int.Parse(versions[0]);
                var v2s = int.Parse(versions[1]);
                var v3s = int.Parse(versions[2]);
                if (++v3s >= 10)
                {
                    v3s = 0;
                    if (++v2s >= 10)
                    {
                        v1s++;
                        v2s = 0;
                    }
                }
                assetBundleBuilder.version = $"{v1s}.{v2s}.{v3s}";
#if UNITY_EDITOR
                EditorUtility.SetDirty(assetBundleBuilder);
#endif
            }
            return assetBundleBuilder.version;
        }

        private void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
            }

            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                var fileName = Path.GetFileName(filePath);
                string newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

                File.Copy(filePath, newFilePath, true);
            }
        }

#if HYBRIDCLR
        public void CopyHotUpdateAssembliesToStreamingAssets(string hotfixAssembliesDstDir)
        {
            if (!Directory.Exists(hotfixAssembliesDstDir))
                Directory.CreateDirectory(hotfixAssembliesDstDir);
            var target = EditorUserBuildSettings.activeBuildTarget;
            var stripDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            var metadataList = new List<string>();
            foreach (var assemblyName in assetBundleBuilder.AOTMetaAssemblyNames)
                CopyAssembliesToHotfixPath(metadataList, stripDir, hotfixAssembliesDstDir, assemblyName);
            stripDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            foreach (var assemblyName in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
                CopyAssembliesToHotfixPath(null, stripDir, hotfixAssembliesDstDir, assemblyName);
            var json = Newtonsoft_X.Json.JsonConvert.SerializeObject(metadataList);
            File.WriteAllText($"{hotfixAssembliesDstDir}/MetadataList.bytes", json);
            AssetDatabase.Refresh();
        }

        private void CopyAssembliesToHotfixPath(List<string> metadataList, string stripDir, string hotfixAssembliesDstDir, string assemblyName)
        {
            if (!assemblyName.EndsWith(".dll"))
                assemblyName += ".dll";
            var dllPath = $"{stripDir}/{assemblyName}";
            if (File.Exists(dllPath))
            {
                var dllBytesPath = $"{hotfixAssembliesDstDir}/{assemblyName}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                metadataList?.Add(dllBytesPath);
            }
            if (!assetBundleBuilder.copyPdb)
                return;
            dllPath = dllPath.Replace("dll", "pdb");
            if (File.Exists(dllPath))
            {
                var pdbBytesPath = $"{hotfixAssembliesDstDir}/{assemblyName.Replace("dll", "pdb")}.bytes";
                File.Copy(dllPath, pdbBytesPath, true);
            }
        }
#endif
    }
}
#endif