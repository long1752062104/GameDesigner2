using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameCore
{
    [Serializable]
    public class AssetBundleInfo
    {
        public string name;
        public string md5;
        public int fileSize;

        public AssetBundleInfo() { }

        public AssetBundleInfo(string name, string md5, int fileSize)
        {
            this.name = name;
            this.md5 = md5;
            this.fileSize = fileSize;
        }
    }

    public enum AssetBundleMode : byte
    {
        /// <summary>
        /// 本机路径, 也就是编辑器路径
        /// </summary>
        EditorMode,
        /// <summary>
        /// Resource加载模式
        /// </summary>
        ResourceMode,
        /// <summary>
        /// 流路径, 不需要网络下载的模式
        /// </summary>
        LocalMode,
        /// <summary>
        /// HFS服务器下载资源更新
        /// </summary>
        ServerMode,
    }

    [Serializable]
    public class AssetInfo
    {
        public string name;
        public string assetBundleName;
        public string md5;
    }

    public class AssetManifest
    {
        public List<AssetInfo> assetInfos = new List<AssetInfo>();
        public Dictionary<string, string[]> dependencies = new Dictionary<string, string[]>();
        private readonly Dictionary<string, AssetInfo> assetInfoDict = new Dictionary<string, AssetInfo>();
        private readonly Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

        public void Init()
        {
            assetInfoDict.Clear();
            foreach (var assetInfo in assetInfos)
                assetInfoDict.Add(assetInfo.name, assetInfo);
        }

        public string[] GetDirectDependencies(string assetBundleName)
        {
            if (dependencies.TryGetValue(assetBundleName, out var directDependencies))
                return directDependencies;
            return new string[0];
        }

        public bool TryGetAssetInfo(string assetPath, out AssetInfo assetInfo)
        {
            if (!assetInfoDict.TryGetValue(assetPath, out assetInfo))
            {
                Global.Logger.LogError($"加载资源:{assetPath}失败!");
                return false;
            }
            return true;
        }

        public bool TryGetAssetBundle(string assetBundleName, out AssetBundle assetBundle)
        {
            return assetBundles.TryGetValue(assetBundleName, out assetBundle);
        }

        public bool AddAssetBundle(string assetBundleName, AssetBundle assetBundle)
        {
            return assetBundles.TryAdd(assetBundleName, assetBundle);
        }

        public bool ContainsAssetBundle(string assetBundleName)
        {
            return assetBundles.ContainsKey(assetBundleName);
        }

        public void AddAssetInfo(string assetName, AssetInfo assetInfo)
        {
            lock (this)
            {
                assetInfos.Add(assetInfo);
                assetInfoDict.Add(assetName, assetInfo);
            }
        }

        public bool TryAddAssetInfo(string assetName, AssetInfo assetInfo)
        {
            if (!assetInfoDict.ContainsKey(assetName))
            {
                assetInfos.Add(assetInfo);
                assetInfoDict.Add(assetName, assetInfo);
                return true;
            }
            return false;
        }

        public bool ContainsAssetInfo(string assetName)
        {
            lock (this)
            {
                return assetInfoDict.ContainsKey(assetName);
            }
        }

        public void Destroy()
        {
            foreach (var assetBundle in assetBundles.Values)
            {
                if (assetBundle == null)
                    continue;
                assetBundle.Unload(true);
            }
        }

        public bool CheckMD5(AssetInfo assetInfo)
        {
            if (assetInfoDict.TryGetValue(assetInfo.name, out var assetInfo1))
                return assetInfo.md5 == assetInfo1.md5;
            return false;
        }
    }

    public class ResourcesManager : MonoBehaviour
    {
        private AssetManifest assetBundleManifest;
        public bool encrypt;
        public int password = 154789548;
#if UNITY_EDITOR
        private Dictionary<string, string> addressablesDict;//可寻址资源字典,仅用于编辑器模式
#endif
        protected readonly Dictionary<string, AssetBundleCreateRequest> assetBundleAsyncDict = new Dictionary<string, AssetBundleCreateRequest>();

        public virtual void Init()
        {
#if UNITY_EDITOR
            if (Global.I.addressables)
                SearchFilesInFolders();
#endif
            if ((byte)Global.I.Mode <= 1)
                return;
            var assetBundlePath = Global.I.AssetBundlePath;
            var manifestBundlePath = assetBundlePath + "assetBundleManifest.json";
            var json = LoadAssetFileReadAllText(manifestBundlePath);
            if (string.IsNullOrEmpty(json))
                throw new Exception("请构建AB包后再运行!");
            assetBundleManifest = Newtonsoft_X.Json.JsonConvert.DeserializeObject<AssetManifest>(json);
            assetBundleManifest.Init();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 搜索所有ab包目录，获取资源名字与路径的对应关系
        /// </summary>
        private void SearchFilesInFolders()
        {
            addressablesDict = new Dictionary<string, string>();
            foreach (var package in AssetBundleBuilder.Instance.Packages)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(package.path).Replace("Assets/", "");//获取路径并去掉Assets/，以免路径中重复包含Assets/
                SearchFilesInFolderRecursive(path, addressablesDict);
            }
        }

        /// <summary>
        /// 递归搜索文件夹，建立资源名和资源路径字典
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileDictionary"></param>
        private void SearchFilesInFolderRecursive(string folderPath, Dictionary<string, string> fileDictionary)
        {
            var files = GetAllFilesInFolder(Path.Combine(Application.dataPath, folderPath)); // 获取指定文件夹及其子文件夹中的所有文件
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath); // 获取文件名（不含后缀）
                var assetPath = filePath.Substring(filePath.IndexOf("Assets")); // 获取相对于Assets文件夹的路径（包含后缀）
                if (!fileDictionary.ContainsKey(fileName))
                    fileDictionary.Add(fileName, assetPath);
            }
        }

        /// <summary>
        /// 查找文件夹中的所有文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private List<string> GetAllFilesInFolder(string folderPath)
        {
            var allFiles = new List<string>();
            var files = Directory.GetFiles(folderPath);
            foreach (string filePath in files)
            {
                if (!filePath.EndsWith(".meta")) // 过滤掉.meta文件
                    allFiles.Add(filePath);
            }
            var subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders)
            {
                var subFolderFiles = GetAllFilesInFolder(subFolder);
                allFiles.AddRange(subFolderFiles);
            }
            return allFiles;
        }
#endif

        public virtual string LoadAssetFileReadAllText(string assetPath)
        {
            var bytes = LoadAssetFile(assetPath);
            if (bytes == null)
                return null;
            if (Global.I.compressionJson)
                bytes = Net.Helper.UnZipHelper.Decompress(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public virtual AssetBundle LoadAssetBundle(string assetBundlePath)
        {
            var bytes = LoadAssetFile(assetBundlePath);
            if (bytes == null)
                return null;
            var assetBundle = AssetBundle.LoadFromMemory(bytes);
            return assetBundle;
        }

        public virtual async UniTask<AssetBundle> LoadAssetBundleAsync(string assetBundlePath)
        {
            if (assetBundleAsyncDict.TryGetValue(assetBundlePath, out var assetBundleCreate))
                goto J;
            var bytes = LoadAssetFile(assetBundlePath);
            if (bytes == null)
                return null;
            assetBundleCreate = AssetBundle.LoadFromMemoryAsync(bytes);
            assetBundleAsyncDict.Add(assetBundlePath, assetBundleCreate);
        J: while (!assetBundleCreate.isDone)
                await UniTask.Yield();
            return assetBundleCreate.assetBundle;
        }

        public virtual byte[] LoadAssetFile(string assetPath)
        {
            if (!assetPath.StartsWith("http"))
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                assetPath = "file://" + assetPath;
#endif
            }
            Global.Logger.Log("读取文件:" + assetPath);
            using (var request = UnityWebRequest.Get(assetPath))
            {
                var oper = request.SendWebRequest();
                while (!oper.isDone)
                    System.Threading.Thread.Yield();
                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError($"读取资源:{assetPath}失败! " + request.error);
                    return null;
                }
                var bytes = request.downloadHandler.data;
                if (encrypt)
                    Net.Helper.EncryptHelper.ToDecrypt(password, bytes);
                return bytes;
            }
        }

        public virtual void OnDestroy()
        {
            if (assetBundleManifest == null)
                return;
            assetBundleManifest.Destroy();
        }

        /// <summary>
        /// 加载资源，遍历所有资源进行查找尝试加载资源， 如果成功则直接返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public virtual T LoadAsset<T>(string assetPath) where T : Object
        {
            assetPath = GetAssetPath(assetPath);
#if UNITY_EDITOR
            if (Global.I.Mode == AssetBundleMode.EditorMode)
                return LoadAssetAtPath<T>(assetPath);
#endif
            if (Global.I.Mode == AssetBundleMode.ResourceMode)
                return ResourcesLoad<T>(assetPath);
            var assetBundle = GetAssetBundle(assetPath);
            if (assetBundle == null)
                return default;
            if (typeof(T) == typeof(Sprite))
                return assetBundle.LoadAsset<T>(assetPath);
            //这里只能获取Object类型，如果直接获取T会获取失败，比如你获取GameObject里面的Animator组件，你直接LoadAsset<Animator>("xx")会失败!
            var assetObject = assetBundle.LoadAsset(assetPath, typeof(Object));
            return GetAsset<T>(assetObject);
        }

        public virtual async UniTask<T> LoadAssetAsync<T>(string assetPath) where T : Object
        {
            assetPath = GetAssetPath(assetPath);
#if UNITY_EDITOR
            if (Global.I.Mode == AssetBundleMode.EditorMode)
                return LoadAssetAtPath<T>(assetPath);
#endif
            if (Global.I.Mode == AssetBundleMode.ResourceMode)
                return ResourcesLoad<T>(assetPath);
            var assetBundle = await GetAssetBundleAsync(assetPath);
            if (assetBundle == null)
                return default;
            if (typeof(T) == typeof(Sprite))
                return assetBundle.LoadAsset<T>(assetPath);
            //这里只能获取Object类型，如果直接获取T会获取失败，比如你获取GameObject里面的Animator组件，你直接LoadAsset<Animator>("xx")会失败!
            var assetObject = await assetBundle.LoadAssetAsync(assetPath, typeof(Object));
            return GetAsset<T>(assetObject);
        }

        public virtual T[] LoadAssetWithSubAssets<T>(string assetPath) where T : Object
        {
            assetPath = GetAssetPath(assetPath);
            Object[] assetObjects;
#if UNITY_EDITOR
            if (Global.I.Mode == AssetBundleMode.EditorMode)
            {
                assetObjects = LoadAllAssetsAtPath(assetPath);
                goto J;
            }
#endif
            if (Global.I.Mode == AssetBundleMode.ResourceMode)
                return ResourcesLoadAll<T>(assetPath);
            var assetBundle = GetAssetBundle(assetPath);
            if (assetBundle == null)
                return default;
            assetObjects = assetBundle.LoadAssetWithSubAssets(assetPath);
        J: return ConvertObjects<T>(assetObjects);
        }

        public virtual async UniTask<T[]> LoadAssetWithSubAssetsAsync<T>(string assetPath) where T : Object
        {
            assetPath = GetAssetPath(assetPath);
            Object[] assetObjects;
#if UNITY_EDITOR
            if (Global.I.Mode == AssetBundleMode.EditorMode)
            {
                assetObjects = LoadAllAssetsAtPath(assetPath);
                goto J;
            }
#endif
            if (Global.I.Mode == AssetBundleMode.ResourceMode)
                return ResourcesLoadAll<T>(assetPath);
            var assetBundle = await GetAssetBundleAsync(assetPath);
            if (assetBundle == null)
                return default;
            var assetBundleRequest = assetBundle.LoadAssetWithSubAssetsAsync(assetPath);
            await assetBundleRequest;
            assetObjects = assetBundleRequest.allAssets;
        J: return ConvertObjects<T>(assetObjects);
        }

        private T[] ConvertObjects<T>(Object[] assetObjects) where T : Object
        {
            var subAssets = new List<T>(assetObjects.Length);
            for (int i = 0; i < assetObjects.Length; i++)
                if (assetObjects[i] is T assetObject) //这里获取Sprite, 这里的第一个元素会是Texture2D
                    subAssets.Add(assetObject);
            return subAssets.ToArray();
        }

#if UNITY_EDITOR
        private T LoadAssetAtPath<T>(string assetPath) where T : Object
        {
            if (Global.I.addressables)
                addressablesDict.TryGetValue(assetPath, out assetPath);
            return assetPath == null ? default : UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private Object[] LoadAllAssetsAtPath(string assetPath)
        {
            if (Global.I.addressables)
                addressablesDict.TryGetValue(assetPath, out assetPath);
            return assetPath == null ? default : UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
        }
#endif

        private T ResourcesLoad<T>(string assetPath) where T : Object
        {
            assetPath = GetResourcesPath(assetPath);
            return Resources.Load<T>(assetPath);
        }

        private T[] ResourcesLoadAll<T>(string assetPath) where T : Object
        {
            assetPath = GetResourcesPath(assetPath);
            return Resources.LoadAll<T>(assetPath);
        }

        private string GetResourcesPath(string assetPath)
        {
            var index = assetPath.IndexOf("Resources");
            assetPath = assetPath.Remove(0, index + 10);
            index = assetPath.LastIndexOf(".");
            assetPath = assetPath.Remove(index, assetPath.Length - index);
            return assetPath;
        }

        protected virtual AssetBundle GetAssetBundle(string assetPath)
        {
            if (!assetBundleManifest.TryGetAssetInfo(assetPath, out var assetInfo))
                return null;
            if (!assetBundleManifest.TryGetAssetBundle(assetInfo.assetBundleName, out var assetBundle))
            {
                var fullPath = Global.I.AssetBundlePath + assetInfo.assetBundleName;
                assetBundle = LoadAssetBundle(fullPath);
                if (assetBundle != null)
                {
                    var result = assetBundleManifest.AddAssetBundle(assetInfo.assetBundleName, assetBundle);
                    if (result)
                        DirectDependencies(assetInfo.assetBundleName);
                }
            }
            return assetBundle;
        }

        protected virtual async UniTask<AssetBundle> GetAssetBundleAsync(string assetPath)
        {
            if (!assetBundleManifest.TryGetAssetInfo(assetPath, out var assetInfo))
                return null;
            if (!assetBundleManifest.TryGetAssetBundle(assetInfo.assetBundleName, out var assetBundle))
            {
                var fullPath = Global.I.AssetBundlePath + assetInfo.assetBundleName;
                assetBundle = await LoadAssetBundleAsync(fullPath);
                if (assetBundle != null)
                {
                    var result = assetBundleManifest.AddAssetBundle(assetInfo.assetBundleName, assetBundle);
                    if (result)
                        await DirectDependenciesAsync(assetInfo.assetBundleName);
                }
            }
            return assetBundle;
        }

        public virtual bool LoadAssetScene(string assetPath, LoadSceneMode mode = LoadSceneMode.Single)
        {
            assetPath = GetAssetPath(assetPath);
            if ((byte)Global.I.Mode <= 1)
            {
                assetPath = Path.GetFileNameWithoutExtension(assetPath);
                goto J;
            }
            var assetBundle = GetAssetBundle(assetPath);
            if (assetBundle == null)
                return false;
            J: UnityEngine.SceneManagement.SceneManager.LoadScene(assetPath, mode);
            return true;
        }

        public virtual async UniTask LoadAssetSceneAsync(string assetPath, Action onLoadComplete = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            await LoadAssetSceneAsync(assetPath, mode, (progress) => Global.UI.Loading.ShowUI("加载场景中..." + (progress * 100f).ToString("f0") + "%", progress), () =>
            {
                Global.UI.Loading.HideUI();
                onLoadComplete?.Invoke();
            });
        }

        public virtual async UniTask LoadAssetSceneAsync(string assetPath, LoadSceneMode mode = LoadSceneMode.Single, Action<float> progress = null, Action onLoadComplete = null)
        {
            assetPath = GetAssetPath(assetPath);
            if ((byte)Global.I.Mode <= 1)
            {
                assetPath = Path.GetFileNameWithoutExtension(assetPath);
                goto J;
            }
            var assetBundle = await GetAssetBundleAsync(assetPath);
            if (assetBundle == null)
                return;
            J: var asyncOper = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(assetPath, mode);
            asyncOper.allowSceneActivation = false;
            while (asyncOper.progress < 0.9f)
            {
                progress?.Invoke(asyncOper.progress);
                await UniTask.Yield();
            }
            progress?.Invoke(1f);
            await UniTask.Delay(1000);
            asyncOper.allowSceneActivation = true;
            onLoadComplete?.Invoke();
        }

        protected virtual void DirectDependencies(string assetBundleName)
        {
            var dependencies = assetBundleManifest.GetDirectDependencies(assetBundleName);
            foreach (var dependencie in dependencies)
            {
                if (assetBundleManifest.ContainsAssetBundle(dependencie))
                    continue;
                var fullPath = Global.I.AssetBundlePath + dependencie;
                var assetBundle = LoadAssetBundle(fullPath);
                var result = assetBundleManifest.AddAssetBundle(dependencie, assetBundle);
                if (result)
                    DirectDependencies(dependencie);
            }
        }

        protected virtual async UniTask DirectDependenciesAsync(string assetBundleName)
        {
            var dependencies = assetBundleManifest.GetDirectDependencies(assetBundleName);
            foreach (var dependencie in dependencies)
            {
                if (assetBundleManifest.ContainsAssetBundle(dependencie))
                    continue;
                var fullPath = Global.I.AssetBundlePath + dependencie;
                var assetBundle = await LoadAssetBundleAsync(fullPath);
                var result = assetBundleManifest.AddAssetBundle(dependencie, assetBundle);
                if (result)
                    await DirectDependenciesAsync(dependencie);
            }
        }

        public T Instantiate<T>(string assetPath, Transform parent = null) where T : Object
        {
            return Instantiate<T>(assetPath, Vector3.zero, Quaternion.identity, parent);
        }

        public async UniTask<T> InstantiateAsync<T>(string assetPath, Transform parent = null) where T : Object
        {
            return await InstantiateAsync<T>(assetPath, Vector3.zero, Quaternion.identity, parent);
        }

        public T Instantiate<T>(string assetPath, Vector3 position, Quaternion rotation, Transform parent = null) where T : Object
        {
            var assetObj = LoadAsset<T>(assetPath);
            if (assetObj == null)
            {
                Global.Logger.LogError($"资源加载失败:{assetPath}");
                return null;
            }
            return Instantiate(assetObj, position, rotation, parent);
        }

        public async UniTask<T> InstantiateAsync<T>(string assetPath, Vector3 position, Quaternion rotation, Transform parent = null) where T : Object
        {
            var assetObj = await LoadAssetAsync<T>(assetPath);
            if (assetObj == null)
            {
                Global.Logger.LogError($"资源加载失败:{assetPath}");
                return null;
            }
            return Instantiate(assetObj, position, rotation, parent);
        }

        /// <summary>
        /// 转移对象信息, 保留对象数据, 可能旧的ResourcesManager被销毁, 转移数据给新的ResourcesManager组件
        /// </summary>
        /// <param name="resources"></param>
        public void Change(ResourcesManager resources)
        {
            assetBundleManifest = resources.assetBundleManifest;
#if UNITY_EDITOR
            addressablesDict = resources.addressablesDict;
            resources.addressablesDict = null;
#endif
            resources.assetBundleManifest = null;
        }

        /// <summary>
        /// 获取资源名，仅使用文件名时，不包含后缀；否则需要带后缀后的完整路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private string GetAssetPath(string assetPath)
        {
            if (Global.I.addressables)//仅使用文件名
                assetPath = Path.GetFileNameWithoutExtension(assetPath);
            return assetPath;
        }

        private T GetAsset<T>(Object assetObject) where T : Object
        {
            if (assetObject is GameObject gameObject)
            {
                if (typeof(T) == typeof(GameObject) | typeof(T) == typeof(Object)) //如果获取的是游戏物体或者基类则直接返回
                    return assetObject as T;
                if (gameObject.TryGetComponent(typeof(T), out var component))
                    return component as T;
            }
            return assetObject as T;
        }
    }
}