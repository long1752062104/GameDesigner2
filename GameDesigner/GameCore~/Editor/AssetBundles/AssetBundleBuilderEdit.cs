#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text;
using Net.Helper;
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor.TestTools.TestRunner.Api;




#if HYBRIDCLR
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
#endif

namespace GameCore
{
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
        private readonly ConcurrentQueue<ParallelLoopResult> parallelLoopResults = new();
        private readonly ConcurrentQueue<ValueTuple<string, float>> displayInfos = new();
        private int errorCount;

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
                ExcelTools.GenerateExcelData();
            if (GUILayout.Button("BuildGameConfigScript"))
                ExcelTools.GenerateExcelDataAll();
            if (GUILayout.Button("BuildAssetBundle"))
                BuildAssetBundle();
            if (GUILayout.Button("打开资源包目录"))
            {
                var outputPath = $"{assetBundleBuilder.outputPath}/{assetBundleBuilder.buildTarget}/";
                outputPath = PathHelper.Combine(outputPath, "/");
                Process.Start("explorer.exe", outputPath);
            }
        }

        private async void BuildAssetBundle()
        {
            var outputPath = $"{assetBundleBuilder.outputPath}/{assetBundleBuilder.buildTarget}/{assetBundleBuilder.version}/";
            if (assetBundleBuilder.clearFolders)
            {
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                if (assetBundleBuilder.copyToStreamingAssets)
                {
                    var m_streamingPath = $"{Application.streamingAssetsPath}/AssetBundles/{assetBundleBuilder.buildTarget}/{assetBundleBuilder.version}/";
                    if (Directory.Exists(m_streamingPath))
                        Directory.Delete(m_streamingPath, true);
                }
            }
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
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
            var abBuildList = new ConcurrentDictionary<string, AssetBundleBuild>();
            var assetManifestNew = new AssetManifest();
            string[] files;
            errorCount = 0;
            parallelLoopResults.Clear();
            displayInfos.Clear();
            var abi = AssetBundleBuilder.Instance;
            for (int i = 0; i < assetBundleBuilder.Packages.Count; i++)
            {
                var package = assetBundleBuilder.Packages[i];
                if (!package.enable)
                    continue;
                var assetPath = AssetDatabase.GetAssetPath(package.path);
                var isFolder = AssetDatabase.IsValidFolder(assetPath);
                if (isFolder)
                {
                    var filters = new List<string>();
                    foreach (var filterObject in package.filters)
                        filters.Add(AssetDatabase.GetAssetPath(filterObject));
                    AssetBundleCollect(abBuildList, assetManifestNew, assetPath, package, i / (float)assetBundleBuilder.Packages.Count, filters);
                }
                else AddSinglePackage(abBuildList, assetManifestNew, assetPath, package);
            }
            while (parallelLoopResults.TryDequeue(out var parallelLoopResult))
            {
                while (!parallelLoopResult.IsCompleted)
                    Thread.Sleep(1);
                while (displayInfos.TryDequeue(out var displayInfo))
                {
                    EditorUtility.DisplayProgressBar("AssetBundleCollect", displayInfo.Item1, displayInfo.Item2);
                    Thread.Sleep(assetBundleBuilder.displayProgressTime);
                }
            }
            EditorUtility.ClearProgressBar();
            if (errorCount > 0)
            {
                Debug.Log("请先解决错误后再打包!");
                return;
            }
            Debug.Log($"收集的资源包数量:{abBuildList.Count} 资源文件数量:{assetManifestNew.assetInfos.Count}");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            var assetManifestPath = outputPath + "assetBundleManifest.json";
            var assetManifestOld = new AssetManifest();
            if (File.Exists(assetManifestPath))
            {
                var assetManifestBytes = File.ReadAllBytes(assetManifestPath);
                if (assetBundleBuilder.encrypt)
                    EncryptHelper.ToDecrypt(assetBundleBuilder.password, assetManifestBytes);
                if (assetBundleBuilder.compressionJson)
                    assetManifestBytes = UnZipHelper.Decompress(assetManifestBytes); //如果上次打包没有选择压缩会导致错误
                var assetManifestJson = Encoding.UTF8.GetString(assetManifestBytes);
                assetManifestOld = Newtonsoft_X.Json.JsonConvert.DeserializeObject<AssetManifest>(assetManifestJson);
                assetManifestOld.Init();
            }
            var assetBundleBuildList = new Dictionary<string, AssetBundleBuild>();
            foreach (var assetInfo in assetManifestNew.assetInfos)
            {
                if (assetManifestOld.CheckMD5(assetInfo))
                    continue;
                var assetBundleName = assetInfo.assetBundleName;
                if (abBuildList.TryGetValue(assetBundleName, out var assetBundleBuild))
                    assetBundleBuildList[assetBundleName] = assetBundleBuild;
                assetManifestOld.TryAddAssetInfo(assetInfo.name, assetInfo);
            }
            if (assetBundleBuilder.incrementalPackaging)
                assetManifestNew = assetManifestOld;
            Debug.Log($"筛选后要构建的资源包数量:{assetBundleBuildList.Count}");
            if (assetBundleBuildList.Count == 0)
            {
                Debug.Log("没有增量更新包!");
                return;
            }
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuildList.Values.ToArray(), opt, assetBundleBuilder.buildTarget);
            if (assetBundleManifest == null) //取消打包
            {
                Debug.Log("取消打包!");
                return;
            }
            var allAssetBundles = assetBundleManifest.GetAllAssetBundles();
            foreach (var allAssetBundle in allAssetBundles)
                assetManifestNew.dependencies[allAssetBundle] = assetBundleManifest.GetDirectDependencies(allAssetBundle);
            var json = Newtonsoft_X.Json.JsonConvert.SerializeObject(assetManifestNew, Newtonsoft_X.Json.Formatting.Indented);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            if (assetBundleBuilder.compressionJson)
                jsonBytes = UnZipHelper.Compress(jsonBytes);
            File.WriteAllBytes(outputPath + "assetBundleManifest.json", jsonBytes);
            var assetBundleInfos = new List<AssetBundleInfo>();
            files = Directory.GetFiles(outputPath, "*.*").Where(s => !s.EndsWith(".meta")).ToArray();
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (file.EndsWith(".manifest") | fileName == assetBundleBuilder.version)
                {
                    if (assetBundleBuilder.clearManifestFile)
                        File.Delete(file);
                    continue;
                }
                var bytes = File.ReadAllBytes(file);
                if (assetBundleBuildList.ContainsKey(fileName) || fileName.EndsWith(".json")) //增量更新的文件才需要加密，否则导致多次加密问题
                {
                    if (assetBundleBuilder.encrypt)
                    {
                        EncryptHelper.ToEncrypt(assetBundleBuilder.password, bytes);
                        File.WriteAllBytes(file, bytes);
                        Debug.Log($"加密文件:{file}完成!");
                    }
                }
                var md5 = EncryptHelper.GetMD5(bytes);
                assetBundleInfos.Add(new AssetBundleInfo(fileName, md5, bytes.Length));
            }
            if (assetBundleBuilder.useFirstPackage)
            {
                EditorUtility.DisplayProgressBar("AssetBundleCollect", $"正在压缩初始包...", 0f);
                await UnZipHelper.CompressFiles(outputPath, outputPath + $"../{assetBundleBuilder.version}.zip", System.IO.Compression.CompressionLevel.Optimal, true, null,
                    (name, progress) => EditorUtility.DisplayProgressBar("AssetBundleCollect", $"正在压缩初始包:{name}", progress));
                if (assetBundleBuilder.firstPackageCopyToStreamingAssets)
                    File.Copy(outputPath + $"../{assetBundleBuilder.version}.zip", $"{Application.streamingAssetsPath}/{Global.I.version}.zip", true);
                EditorUtility.ClearProgressBar();
            }
            json = Newtonsoft_X.Json.JsonConvert.SerializeObject(assetBundleInfos, Newtonsoft_X.Json.Formatting.Indented);
            jsonBytes = Encoding.UTF8.GetBytes(json);
            if (assetBundleBuilder.compressionJson)
                jsonBytes = UnZipHelper.Compress(jsonBytes);
            File.WriteAllBytes(outputPath + "../version.json", jsonBytes);
            if (assetBundleBuilder.copyToStreamingAssets)
            {
                var m_streamingPath = Application.streamingAssetsPath + "/AssetBundles/";
                DirectoryCopy(assetBundleBuilder.outputPath, m_streamingPath);
            }
            CheckAutoIncrement();
            AssetDatabase.Refresh();
            Debug.Log("构建资源完成!");
        }

        private string GetAssetBundleName(string assetPath, CollectType type)
        {
            string assetBundleName;
            if (type == CollectType.AllDirectoriesSplitHashName | type == CollectType.TopDirectoryOnlyHashName | type == CollectType.AllDirectoriesHashName)
                assetBundleName = EncryptHelper.GetMD5(assetPath);
            else
                assetBundleName = assetPath.Replace("\\", "_").Replace("/", "_").Replace(".", "_").Replace(" ", "").Replace("-", "_").ToLower();
            if (!string.IsNullOrEmpty(assetBundleBuilder.packageFormat))
                assetBundleName += "." + assetBundleBuilder.packageFormat;
            return assetBundleName;
        }

        private void AssetBundleCollect(ConcurrentDictionary<string, AssetBundleBuild> buildList, AssetManifest assetManifest, string assetPath, AssetBundlePackage package, float progress, List<string> filters)
        {
            displayInfos.Enqueue(($"收集路径:{assetPath}", progress));
            var type = package.type;
            SearchOption searchOption;
            if (type == CollectType.AllDirectories | type == CollectType.AllDirectoriesHashName) //只有这两种获取所有文件夹的文件，Split模式则不能使用
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
                foreach (var assetPath in filters)
                {
                    if (StringHelper.StartsWith(s, assetPath))
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
            for (int i = files.Count - 1; i >= 0; i--)
            {
                files[i] = files[i].Replace('\\', '/');
                if (files[i].EndsWith(".unity"))
                {
                    var sceneAssetBundleName = AddSinglePackage(buildList, assetManifest, files[i], package);
                    files.RemoveAt(i);
                    displayInfos.Enqueue(($"收集资源包:{sceneAssetBundleName}完成!", progress));
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
                var addressableNames = new List<string>();
                files.RemoveRange(0, sizeIndex);
                var addressables = AssetBundleBuilder.Instance.addressables;
                for (int i = 0; i < assetNames.Count; i++)
                {
                    var lastModified = File.GetLastWriteTime(assetNames[i]);
                    var lastModified1 = File.GetLastWriteTime(assetNames[i] + ".meta");
                    var md5 = EncryptHelper.GetMD5($"{lastModified}-{lastModified1}");
                    var assetName = assetNames[i];
                    if (addressables)//资源名不包含路径和后缀
                    {
                        assetName = Path.GetFileNameWithoutExtension(assetName);
                        addressableNames.Add(assetName);
                    }
                    if (assetManifest.ContainsAssetInfo(assetName))
                    {
                        Interlocked.Increment(ref errorCount);
                        Debug.LogError($"资源{assetName}有同名, 可寻址模式资源不可同名!"); //资源名需要在所有ab中唯一（不能同名）
                        continue;
                    }
                    assetManifest.AddAssetInfo(assetName, new AssetInfo()
                    {
                        name = assetName,
                        assetBundleName = assetBundleName,
                        md5 = md5,
                    });
                }
                var assetBundleBuild = new AssetBundleBuild
                {
                    assetBundleName = assetBundleName,
                    assetNames = assetNames.ToArray(),
                    addressableNames = addressableNames.ToArray()
                };
                buildList.TryAdd(assetBundleName, assetBundleBuild);
                displayInfos.Enqueue(($"收集资源包:{assetBundleName}完成!", progress));
            }
            if (type < CollectType.AllDirectoriesSplit)
                return;
            var directories = Directory.GetDirectories(assetPath);
            var parallelLoopResult = Parallel.ForEach(directories, directorie => AssetBundleCollect(buildList, assetManifest, directorie, package, progress, filters));
            parallelLoopResults.Enqueue(parallelLoopResult);
        }

        private string AddSinglePackage(ConcurrentDictionary<string, AssetBundleBuild> buildList, AssetManifest assetManifest, string assetPath, AssetBundlePackage package)
        {
            var lastModified = File.GetLastWriteTime(assetPath);
            var lastModified1 = File.GetLastWriteTime(assetPath + ".meta");
            var md5 = EncryptHelper.GetMD5($"{lastModified}-{lastModified1}");
            var addressableNames = new List<string>();
            var addressables = AssetBundleBuilder.Instance.addressables;
            if (addressables)//资源名不包含路径和后缀
            {
                assetPath = Path.GetFileNameWithoutExtension(assetPath);
                addressableNames.Add(assetPath);
            }
            if (assetManifest.ContainsAssetInfo(assetPath))
            {
                Interlocked.Increment(ref errorCount);
                Debug.LogError($"资源{assetPath}有同名, 可寻址模式资源不可同名!"); //资源名需要在所有ab中唯一（不能同名）
                return string.Empty;
            }
            var sceneAssetBundleName = GetAssetBundleName(assetPath, package.type);
            assetManifest.AddAssetInfo(assetPath, new AssetInfo()
            {
                name = assetPath,
                assetBundleName = sceneAssetBundleName,
                md5 = md5,
            });
            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = sceneAssetBundleName,
                assetNames = new string[] { assetPath },
                addressableNames = addressableNames.ToArray()
            };
            buildList.TryAdd(sceneAssetBundleName, assetBundleBuild);
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
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);
            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
            }
            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                var fileName = Path.GetFileName(filePath);
                var newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);
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