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

namespace Framework
{
    public class AssetBundleCheckUpdate : MonoBehaviour
    {
        public string url = "http://192.168.1.5/";
        public string metadataList = "Assets/Arts/Hotfix/MetadataList.bytes";
        public string hotfixDll = "Assets/Arts/Hotfix/Main.dll.bytes";
        public bool checkFileMD5;

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
            var localAssetBundleDict = await VersionCheck(versionUrl);
            localAssetBundleDict ??= new Dictionary<string, AssetBundleInfo>();
            var result = await CheckAssetBundle(serverAssetBundleDict, localAssetBundleDict);
            if (!result)
                return;
            var json = Newtonsoft_X.Json.JsonConvert.SerializeObject(serverAssetBundleDict.Values.ToList());
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            if (Global.I.compressionJson)
                jsonBytes = UnZipHelper.Compress(jsonBytes);
            File.WriteAllBytes(versionUrl, jsonBytes);
            await UniTask.Delay(1000);
            LoadAssetBundle();
        }

        private async UniTask<bool> CheckAssetBundle(Dictionary<string, AssetBundleInfo> serverAssetBundleDict, Dictionary<string, AssetBundleInfo> localAssetBundleDict)
        {
            var assetBundleInfos = new List<AssetBundleInfo>();
            ulong totalSize = 0;
            ulong currSize = 0;
            int index = 0;
            int count = serverAssetBundleDict.Count;
            Global.UI.Loading.ShowUI("正在检查资源中...", 0f);
            foreach (var assetBundleInfo in serverAssetBundleDict.Values)
            {
                var localAssetBundleUrl = $"{Global.I.AssetBundlePath}/{assetBundleInfo.name}";
                Global.UI.Loading.ShowUI($"检查资源:{assetBundleInfo.name}", index++ / (float)count);
                await UniTask.Yield();
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
                var msgClose = false;
                var msgResult = false;
                Global.UI.Message.ShowUI("资源请求", $"有资源需要更新:{ByteHelper.ToString(totalSize)}", result =>
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
                if (!msgResult)
                    return false;
            }
            foreach (var assetBundleInfo in assetBundleInfos)
            {
                var serverAssetBundleUrl = $"{url}AssetBundles/{Global.I.platform}/{Global.I.version}/{assetBundleInfo.name}";
                var localAssetBundleUrl = $"{Global.I.AssetBundlePath}/{assetBundleInfo.name}";
                var request = UnityWebRequest.Head(serverAssetBundleUrl);
                await request.SendWebRequest();
                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError($"{serverAssetBundleUrl} 获取资源失败:" + request.error);
                    return false;
                }
                ulong totalLength = ulong.Parse(request.GetResponseHeader("Content-Length"));
                request.Dispose();
                request = new UnityWebRequest(serverAssetBundleUrl, "GET", new DownloadHandlerFile(localAssetBundleUrl), null);
                _ = request.SendWebRequest();
                string progressText;
                string name = assetBundleInfo.name;
                while (!request.isDone)
                {
                    progressText = $"{name}下载进度:{ByteHelper.ToString(request.downloadedBytes)}/{ByteHelper.ToString(totalLength)}";
                    Global.UI.Loading.ShowUI(progressText, (float)(currSize / (double)totalSize));
                    await UniTask.Yield();
                }
                progressText = $"{name}下载完成!";
                currSize += request.downloadedBytes;
                Global.UI.Loading.ShowUI(progressText, (float)(currSize / (double)totalSize));
                request.Dispose();
            }
            return true;
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