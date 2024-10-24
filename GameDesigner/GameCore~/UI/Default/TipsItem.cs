using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameCore
{
    public class TipsItem : MonoBehaviour
    {
        public Text info;
        internal float showTime;
        internal float hideTime;

        private async void OnEnable()
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            transform.DOScale(1f, 0.2f);
            await UniTask.Delay((int)(hideTime * 1000f));
            transform.DOLocalMoveY(450f, 0.8f);
            await UniTask.Delay(800);
            gameObject.SetActive(false);
            Global.Pool.Recycling(this);
        }
    }
}