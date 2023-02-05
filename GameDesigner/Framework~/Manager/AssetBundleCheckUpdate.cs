using Cysharp.Threading.Tasks;
using HybridCLR;
using Net.Share;
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

        private async void LocalLoadAB()
        {
            await Global.Table.Init();
            Global.Resources.InitAssetBundleInfos();
            LoadMetadataForAOTAssemblies();
            Global.Resources.Instantiate(entryRes);
        }

        private IEnumerator CheckUpdate()
        {
            var versionUrl = url + "AssetBundles/Version.txt";
            var request = UnityWebRequest.Get(versionUrl);
            yield return request.SendWebRequest();
            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError($"{versionUrl} ��ȡʧ��:" + request.error);
                Global.UI.Message.ShowUI("��Դ����", "�汾��Ϣ����ʧ��!", result =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    Application.Quit();
                });
                yield break;
            }
            var text = request.downloadHandler.text;
            request.Dispose();
            var abPath = Application.persistentDataPath + "/";
            var oldDict = GlobalSetting.Instance.GetVersionPathDict(abPath);
            if (text != oldDict["Version"])
            {
                bool msgClose = false;
                bool msgResult = false;
                Global.UI.Message.ShowUI("��Դ����", "�а汾��Ҫ����", result =>
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
                var dict = GlobalSetting.Instance.GetVersionDict(text);
                foreach (var item in dict)
                {
                    if (item.Key == "Version")
                        continue;
                    if (oldDict.TryGetValue(item.Key, out var value))
                        if (item.Value == value)
                            continue;
                    var abUrl = url + item.Key;
                    var savePath = abPath + item.Key;
                    request = UnityWebRequest.Head(abUrl);
                    yield return request.SendWebRequest();
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogError($"{abUrl} ��ȡ��Դʧ��:" + request.error);
                        yield break;
                    }
                    ulong totalLength = ulong.Parse(request.GetResponseHeader("Content-Length"));
                    request.Dispose();
                    request = new UnityWebRequest(abUrl, "GET", new DownloadHandlerFile(savePath), null);
                    request.SendWebRequest();
                    string progressText;
                    string name = Path.GetFileName(item.Key);
                    while (!request.isDone)
                    {
                        progressText = $"{name}���ؽ���:{ByteHelper.ToString(request.downloadedBytes)}/{ByteHelper.ToString(totalLength)}";
                        Global.UI.Loading.ShowUI(progressText, request.downloadProgress);
                        yield return null;
                    }
                    progressText = $"{name}�������!";
                    Global.UI.Loading.ShowUI(progressText, request.downloadProgress);
                    request.Dispose();
                }
                GlobalSetting.Instance.SaveVersionDict(dict, abPath);
                yield return new WaitForSeconds(1f);
            }
            LocalLoadAB();
        }

        /// <summary>
        /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�
        /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            string path;
            if (Global.Resources.Mode == AssetBundleMode.LocalPath)
                path = Application.streamingAssetsPath + "/AssetBundles/Hotfix/";
            else
                path = Application.persistentDataPath + "/AssetBundles/Hotfix/";
            /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
            /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
            /// 
            if (!Directory.Exists(path))
                return;
            var files = Directory.GetFiles(path, "*.bytes");
            var mode = HomologousImageMode.SuperSet;
            foreach (var dllPath in files)
            {
                //if (dllPath.Contains("Assembly-CSharp.dll.bytes"))
                //    continue;
                var dllBytes = File.ReadAllBytes(dllPath);
                // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
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