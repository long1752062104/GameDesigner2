using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    public class TipsItem : MonoBehaviour
    {
        public Text info;
        internal float time;

        private async void OnEnable()
        {
            await UniTask.Delay((int)(time * 1000f));
            gameObject.SetActive(false);
            Global.Pool.Recycling(this);
        }
    }
}