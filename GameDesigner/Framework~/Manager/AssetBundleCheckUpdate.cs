using HybridCLR;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework
{
    public class AssetBundleCheckUpdate : MonoBehaviour
    {
        public string url = "http://192.168.1.5/";
        public string entryRes = "Assets/Resources/Prefabs/GameEntry.prefab";

        void Start()
        {
            if (Global.Resources.Mode == AssetBundleMode.LocalPath)
                LocalLoadAB();
            else
                StartCoroutine(CheckUpdate());
        }

        private void LocalLoadAB()
        {
            Global.Table.Init();
            Global.Resources.InitAssetBundleInfos();
            LoadMetadataForAOTAssemblies();
            Global.Resources.Instantiate(entryRes);
        }

        private IEnumerator CheckUpdate()
        {
            var abPath = Application.persistentDataPath + "/";
            var versionFile = abPath + "AssetBundles/Version.txt";
            var versionUrl = url + "AssetBundles/Version.txt";
            var webRequest = UnityWebRequest.Get(versionUrl);
            yield return webRequest.SendWebRequest();
            if (!string.IsNullOrEmpty(webRequest.error))
            {
                Debug.LogError($"{versionUrl} 获取失败:" + webRequest.error);
                Global.UI.Message.ShowUI("资源请求", "版本信息请求失败!", result =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    Application.Quit();
                });
                yield break;
            }
            var text = webRequest.downloadHandler.text;
            var versionText = "";
            if (File.Exists(versionFile))
                versionText = File.ReadAllText(versionFile);
            if (text != versionText)
            {
                bool msgClose = false;
                bool msgResult = false;
                Global.UI.Message.ShowUI("资源请求", "有版本需要更新", result =>
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
                    yield return null;
                if (!msgResult)
                    yield break;
                var path = Path.GetDirectoryName(versionFile);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Split('|');
                    var abUrl = url + line[0];
                    var savePath = abPath + line[0];
                    using (var request = UnityWebRequest.Get(abUrl))
                    {
                        request.downloadHandler = new DownloadHandlerFile(savePath);
                        var requestAsyc = request.SendWebRequest();
                        if (!string.IsNullOrEmpty(request.error))
                        {
                            Debug.LogError($"{abUrl} 获取失败:" + request.error);
                            yield break;
                        }
                        string progressText;
                        while (!requestAsyc.isDone)
                        {
                            progressText = $"{line[0]}下载进度:{(requestAsyc.progress * 100f).ToString("f2")}";
                            Global.UI.Loading.ShowUI(progressText, requestAsyc.progress);
                            yield return null;
                        }
                        progressText = $"{line[0]}下载完成!";
                        Global.UI.Loading.ShowUI(progressText, requestAsyc.progress);
                    }
                }
                File.WriteAllText(versionFile, text);
                yield return new WaitForSeconds(1f);
            }
            LocalLoadAB();
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            string path;
            if (Global.Resources.Mode == AssetBundleMode.LocalPath)
                path = Application.streamingAssetsPath + "/AssetBundles/Hotfix/";
            else
                path = Application.persistentDataPath + "/AssetBundles/Hotfix/";
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            if (!Directory.Exists(path))
                return;
            var files = Directory.GetFiles(path, "*.bytes");
            var mode = HomologousImageMode.SuperSet;
            foreach (var dllPath in files)
            {
                if (dllPath.Contains("Assembly-CSharp.dll.bytes"))
                    continue;
                var dllBytes = File.ReadAllBytes(dllPath);
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{dllPath}. mode:{mode} ret:{err}");
            }
#if !UNITY_EDITOR
            var hotfixDll = path + "Assembly-CSharp.dll.bytes";
            if (File.Exists(hotfixDll))
                System.Reflection.Assembly.Load(File.ReadAllBytes(hotfixDll));
#endif
        }
    }
}