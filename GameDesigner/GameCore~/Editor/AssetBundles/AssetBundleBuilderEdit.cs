#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text;
using Net.Helper;
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
        private readonly Dictionary<FilterOptions, string[]> filterOptionDict = new Dictionary<FilterOptions, string[]>()
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
                BuildAssetBundle();
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
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            var assetInfoListPath = outputPath + "assetInfoList.json";
            var assetInfoListSource = new Dictionary<string, AssetInfo>();
            if (File.Exists(assetInfoListPath))
            {
                var assetInfoListBytes = File.ReadAllBytes(assetInfoListPath);
                if (assetBundleBuilder.compressionJson)
                    assetInfoListBytes = UnZipHelper.Decompress(assetInfoListBytes);
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
                    manifestBytes = UnZipHelper.Decompress(manifestBytes);
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
                jsonBytes = UnZipHelper.Compress(jsonBytes);
            File.WriteAllBytes(outputPath + "assetBundleManifest.json", jsonBytes);
            json = Newtonsoft_X.Json.JsonConvert.SerializeObject(assetInfoList, Newtonsoft_X.Json.Formatting.Indented);
            jsonBytes = Encoding.UTF8.GetBytes(json);
            if (assetBundleBuilder.compressionJson)
                jsonBytes = UnZipHelper.Compress(jsonBytes);
            File.WriteAllBytes(outputPath + "assetInfoList.json", jsonBytes);
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
                if (assetBundleBuilder.encrypt)
                {
                    EncryptHelper.ToEncrypt(assetBundleBuilder.password, bytes);
                    File.WriteAllBytes(file, bytes);
                    Debug.Log($"加密文件:{file}完成!");
                }
                var md5 = EncryptHelper.GetMD5(bytes);
                assetBundleInfos.Add(new AssetBundleInfo(fileName, md5, bytes.Length));
            }
            if (assetBundleBuilder.useFirstPackage)
            {
                EditorUtility.DisplayProgressBar("AssetBundleCollect", $"正在压缩初始包...", 0f);
                await UnZipHelper.CompressFiles(outputPath, outputPath + $"../{assetBundleBuilder.version}.zip", System.IO.Compression.CompressionLevel.Optimal, true, null,
                    (name, progress) => EditorUtility.DisplayProgressBar("AssetBundleCollect", $"正在压缩初始包:{name}", progress));
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
                    var md5 = EncryptHelper.GetMD5($"{lastModified}-{lastModified1}");
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
            var md5 = EncryptHelper.GetMD5($"{lastModified}-{lastModified1}");
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