using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SM = UnityEngine.SceneManagement.SceneManager;

namespace GameCore
{
    public class SceneManager : MonoBehaviour
    {
        public string sheetName = "Scene";

        public void Load(string sceneName, Action onLoadComplete = null)
        {
            StartCoroutine(AsyncLoadScene(sceneName, onLoadComplete));
        }

        private IEnumerator AsyncLoadScene(string sceneName, Action onLoadComplete = null)
        {
            var op = SM.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
            {
                Global.UI.Loading.ShowUI("加载场景中..." + (op.progress * 100f).ToString("f0") + "%", op.progress);
                yield return null;
            }
            Global.UI.Loading.ShowUI("加载完成", 1f);
            yield return new WaitForSeconds(1f);
            op.allowSceneActivation = true;
            Global.UI.Loading.HideUI();
            onLoadComplete?.Invoke();
        }

        public virtual async UniTask LoadAssetSceneAsync(string assetPath, Action onLoadComplete = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            await LoadAssetSceneAsync(assetPath, mode, (progress) => Global.UI.Loading.ShowUI("加载场景中..." + (progress * 100f).ToString("f0") + "%", progress), () => Global.UI.Loading.ShowUI("加载完成", 1f));
        }

        public virtual async UniTask LoadAssetSceneAsync(string assetPath, LoadSceneMode mode = LoadSceneMode.Single, Action<float> progress = null, Action onLoadComplete = null)
        {
            await Global.Resources.LoadAssetSceneAsync(assetPath, mode, progress, onLoadComplete);
        }
    }
}