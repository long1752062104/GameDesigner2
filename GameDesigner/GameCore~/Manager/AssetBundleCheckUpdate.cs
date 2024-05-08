using Cysharp.Threading.Tasks;
using Net.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
#if HYBRIDCLR
using HybridCLR;
#endif

namespace GameCore
{
    public class AssetBundleCheckUpdate : MonoBehaviour
    {
        public string url = "http://192.168.1.5/";
        public string metadataList = "Assets/Arts/Hotfix/MetadataList.bytes";
        public string hotfixDll = "Assets/Arts/Hotfix/Main.dll.bytes";
        public bool checkFileMD5;
        [Tooltip("使用首包下载, 在CDN服务器上会有个所有资源的压缩文件, 当游戏是第一次下载时, 会下载这个压缩文件, 而不是一个个AB文件下载")]
        public bool useFirstPackage = true;
        [Tooltip("首包压缩文件在StreamingAssets路径下，当游戏启动后会在StreamingAssets路径复制到持久化文件夹下，就不需要下载首包文件了")]
        public bool firstPackageInStreamingAssets = false;
        [Tooltip("检查多少个AB包刷新一下Loading界面")]
        public int checkAssetsRefresh = 1;

        public virtual void Start()
        {
            var mode = Global.I.Mode;
            if ((byte)mode <= 2)
                LoadAssetBundle();
            else
                _ = CheckAssetBundleUpdate();
        }

        public void LoadAssetBundle()
        {
            Global.Resources.Init();
            Global.Table.Init();
            LoadMetadataForAOTAssemblies();
            Global.I.OnInit();
        }

        private async UniTask<Dictionary<string, AssetBundleInfo>> VersionCheck(string url, Action<string> error = null)
        {
            UnityWebRequest request = null;
            try
            {
                if (!url.StartsWith("http"))
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    url = "file://" + url;
#endif
                }
                request = UnityWebRequest.Get(url);
                await request.SendWebRequest();
                if (!string.IsNullOrEmpty(request.error))
                {
                    error?.Invoke(request.error);
                    return null;
                }
                var jsonBytes = request.downloadHandler.data;
                if (Global.I.compressionJson)
                    jsonBytes = UnZipHelper.Decompress(jsonBytes);
                var text = Encoding.UTF8.GetString(jsonBytes);
                var assetBundleInfos = Newtonsoft_X.Json.JsonConvert.DeserializeObject<List<AssetBundleInfo>>(text);
                var assetBundleDict = new Dictionary<string, AssetBundleInfo>();
                foreach (var item in assetBundleInfos)
                    assetBundleDict.Add(item.name, item);
                return assetBundleDict;
            }
            catch (Exception ex)
            {
                error?.Invoke(ex.Message);
                return null;
            }
            finally
            {
                request?.Dispose();
            }
        }

        private async UniTaskVoid CheckAssetBundleUpdate()
        {
            var versionUrl = $"{url}AssetBundles/{Global.I.platform}/version.json";
            var serverAssetBundleDict = await VersionCheck(versionUrl, (error) =>
            {
                Debug.LogError($"{url} 获取失败:" + error);
                Global.UI.Message.ShowUI("资源请求", "版本信息请求失败!", result =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    Application.Quit();
                });
            });
            if (serverAssetBundleDict == null)
                return;
            versionUrl = Global.I.AssetBundlePath + "../version.json";
            versionUrl = Path.GetFullPath(versionUrl).Replace("\\", "/");
            var localAssetBundleDict = await VersionCheck(versionUrl);
            if (localAssetBundleDict == null)
            {
                if (useFirstPackage)
                {
                    var complete = await FirstPackageDownload();
                    if (!complete)
                        return;
                    goto J;
                }
                localAssetBundleDict = new Dictionary<string, AssetBundleInfo>();
            }
            var result = await CheckAssetBundle(serverAssetBundleDict, localAssetBundleDict);
            if (!result)
                return;
            J: var json = Newtonsoft_X.Json.JsonConvert.SerializeObject(serverAssetBundleDict.Values.ToList());
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            if (Global.I.compressionJson)
                jsonBytes = UnZipHelper.Compress(jsonBytes);
            File.WriteAllBytes(versionUrl, jsonBytes);
            await UniTask.Delay(1000);
            LoadAssetBundle();
        }

        protected virtual async UniTask<bool> FirstPackageDownload()
        {
            var serverAssetBundleUrl = $"{url}AssetBundles/{Global.I.platform}/{Global.I.version}.zip";
            var localAssetBundleUrl = $"{Application.persistentDataPath}/AssetBundles/{Global.I.platform}/{Global.I.version}.zip";
            if (File.Exists(localAssetBundleUrl))
                goto J;
            if (firstPackageInStreamingAssets)
            {
                var item1 = await CopyToPersistentDataPath($"{Application.streamingAssetsPath}/{Global.I.version}.zip", localAssetBundleUrl);
                if (!item1.Item1)
                {
                    Global.UI.Message.ShowUI("首包复制", $"复制首包文件出错:{item1.Item2}");
                    return false;
                }
                goto J;
            }
            var item = await GetFileLength(serverAssetBundleUrl);
            if (!item.Item1)
            {
                Global.UI.Message.ShowUI("资源请求", "资源包请求失败!");
                return false;
            }
            var msgResult = await DownloadRequestTips(item.Item2);
            if (!msgResult)
                return false;
            item = await DownloadFile(serverAssetBundleUrl, localAssetBundleUrl, $"资源包{Global.I.version}", item.Item2);
            if (!item.Item1)
            {
                Global.UI.Message.ShowUI("资源请求", "资源包请求失败!");
                return false;
            }
        J: Global.UI.Loading.ShowUI("正在解压资源包...", 0f);
            var result = await UnZipHelper.DecompressFile(localAssetBundleUrl, $"{Application.persistentDataPath}/AssetBundles/{Global.I.platform}/", null,
                (name, progress) => Global.UI.Loading.ShowUI($"正在解压资源包:{name}", progress));
            if (!result)
            {
                Global.UI.Loading.ShowUI("解压初始包失败!", 1f);
                File.Delete(localAssetBundleUrl);
                return false;
            }
            Global.UI.Loading.ShowUI("解压完成", 1f);
            await UniTask.Delay(100);
            File.Delete(localAssetBundleUrl);
            Global.UI.Loading.HideUI();
            return true;
        }

        private async UniTask<ValueTuple<bool, string>> CopyToPersistentDataPath(string fileUrl, string destPath)
        {
            using (var request = UnityWebRequest.Get(fileUrl))
            {
                await request.SendWebRequest();
                if (!string.IsNullOrEmpty(request.error))
                    return (false, request.error);
                var destDir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
                var data = request.downloadHandler.data;
                File.WriteAllBytes(destPath, data);
                return (true, null);
            }
        }

        private async UniTask<bool> CheckAssetBundle(Dictionary<string, AssetBundleInfo> serverAssetBundleDict, Dictionary<string, AssetBundleInfo> localAssetBundleDict)
        {
            var assetBundleInfos = new List<AssetBundleInfo>();
            ulong totalSize = 0;
            ulong currSize = 0;
            int index = 0;
            int count = serverAssetBundleDict.Count;
            int checkAssetsCount = 0;
            Global.UI.Loading.ShowUI("正在检查资源中...", 0f);
            foreach (var assetBundleInfo in serverAssetBundleDict.Values)
            {
                var localAssetBundleUrl = $"{Global.I.AssetBundlePath}/{assetBundleInfo.name}";
                Global.UI.Loading.ShowUI($"检查资源:{assetBundleInfo.name}", index++ / (float)count);
                if (++checkAssetsCount >= checkAssetsRefresh)
                {
                    checkAssetsCount = 0;
                    await UniTask.Yield();
                }
                if (localAssetBundleDict.TryGetValue(assetBundleInfo.name, out var assetBundleInfo1))
                {
                    if (assetBundleInfo1.md5 != assetBundleInfo.md5)
                        goto J;
                }
                if (File.Exists(localAssetBundleUrl))
                {
                    if (!checkFileMD5)
                        continue;
                    var md5 = EncryptHelper.ToMD5(localAssetBundleUrl);
                    if (md5 == assetBundleInfo.md5)
                        continue;
                }
            J: assetBundleInfos.Add(assetBundleInfo);
                totalSize += (ulong)assetBundleInfo.fileSize;
            }
            Global.UI.Loading.HideUI();
            if (totalSize > 0)
            {
                var msgResult = await DownloadRequestTips(totalSize);
                if (!msgResult)
                    return false;
            }
            foreach (var assetBundleInfo in assetBundleInfos)
            {
                var serverAssetBundleUrl = $"{url}AssetBundles/{Global.I.platform}/{Global.I.version}/{assetBundleInfo.name}";
                var localAssetBundleUrl = $"{Global.I.AssetBundlePath}/{assetBundleInfo.name}";
                var item = await DownloadFile(serverAssetBundleUrl, localAssetBundleUrl, assetBundleInfo.name,
                    (progressText, downloadedBytes) => Global.UI.Loading.ShowUI(progressText, (float)((currSize + downloadedBytes) / (double)totalSize)),
                    (progressText) => Global.UI.Loading.ShowUI(progressText, (float)(currSize / (double)totalSize)));
                if (!item.Item1)
                    return false;
                currSize += item.Item2;
            }
            return true;
        }

        protected virtual async UniTask<bool> DownloadRequestTips(ulong totalLength)
        {
            var msgClose = false;
            var msgResult = false;
            Global.UI.Message.ShowUI("资源请求", $"有资源需要更新:{ByteHelper.ToString(totalLength)}", result =>
            {
                msgClose = true;
                msgResult = result;
                if (!result)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    Application.Quit();
                }
            });
            while (!msgClose)
                await UniTask.Yield();
            return msgResult;
        }

        protected virtual async UniTask<ValueTuple<bool, ulong>> DownloadFile(string url, string filePath, string name, Action<string, ulong> downloadUpdate = null, Action<string> downloadCompleted = null)
        {
            var item = await GetFileLength(url);
            if (!item.Item1)
                return (false, 0UL);
            var totalLength = item.Item2;
            return await DownloadFile(url, filePath, name, totalLength, downloadUpdate, downloadCompleted);
        }

        protected virtual async UniTask<ValueTuple<bool, ulong>> DownloadFile(string url, string filePath, string name, ulong totalLength, Action<string, ulong> downloadUpdate = null, Action<string> downloadCompleted = null)
        {
            using (var request = new UnityWebRequest(url, "GET", new DownloadHandlerFile(filePath), null))
            {
                _ = request.SendWebRequest();
                string progressText;
                while (!request.isDone)
                {
                    progressText = $"{name}下载进度:{ByteHelper.ToString(request.downloadedBytes)}/{ByteHelper.ToString(totalLength)}";
                    if (downloadUpdate == null)
                        Global.UI.Loading.ShowUI(progressText, (float)(request.downloadedBytes / (double)totalLength));
                    else
                        downloadUpdate(progressText, request.downloadedBytes);
                    await UniTask.Yield();
                }
                progressText = $"{name}下载完成!";
                if (downloadCompleted == null)
                    Global.UI.Loading.ShowUI(progressText, 1f);
                else
                    downloadCompleted(progressText);
                return (true, totalLength);
            }
        }

        protected virtual async UniTask<ValueTuple<bool, ulong>> GetFileLength(string url)
        {
            using (var request = UnityWebRequest.Head(url))
            {
                await request.SendWebRequest();
                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError($"{url} 获取资源失败:" + request.error);
                    return (false, 0UL);
                }
                var totalLength = ulong.Parse(request.GetResponseHeader("Content-Length"));
                return (true, totalLength);
            }
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private void LoadMetadataForAOTAssemblies()
        {
#if HYBRIDCLR
            if (Global.HotfixAssembly != null)
                return;
            var textAsset = Global.Resources.LoadAsset<TextAsset>(metadataList);
            if (textAsset == null)
            {
                Debug.LogError("获取元数据清单资源失败!");
                return;
            }
            var files = Newtonsoft_X.Json.JsonConvert.DeserializeObject<List<string>>(textAsset.text);
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            var mode = HomologousImageMode.SuperSet;
            foreach (var dllPath in files)
            {
                var metaAsset = Global.Resources.LoadAsset<TextAsset>(dllPath);
                if (metaAsset == null)
                {
                    Debug.LogError($"补充元数据dll:{dllPath}获取失败!");
                    continue;
                }
                var dllBytes = metaAsset.bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{dllPath}. mode:{mode} ret:{err}");
            }
#if UNITY_EDITOR
            //检查热更新是不是在项目里面被加载了， 如果已经加载就不进行Load了
            Global.HotfixAssembly = AssemblyHelper.GetRunAssembly(Path.GetFileNameWithoutExtension(hotfixDll).Replace(".dll", ""));
            if (Global.HotfixAssembly != null)
                return;
#endif
            byte[] rawBytes = null;
            byte[] pdbBytes = null;
            var dllAsset = Global.Resources.LoadAsset<TextAsset>(hotfixDll);
            if (dllAsset != null)
                rawBytes = dllAsset.bytes;
            var hotfixPdb = hotfixDll.Replace("dll", "pdb");
            var pdbAsset = Global.Resources.LoadAsset<TextAsset>(hotfixPdb);
            if (pdbAsset != null)
                pdbBytes = pdbAsset.bytes;
            Global.HotfixAssembly = System.Reflection.Assembly.Load(rawBytes, pdbBytes);
#endif
        }
    }
}