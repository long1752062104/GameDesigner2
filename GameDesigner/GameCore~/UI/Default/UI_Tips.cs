using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class UI_Tips : UIBase<UI_Tips, TipsItem>
    {
        public float hideTime = 1.5f;
        public float interval = 0.2f;
        private float time;
        private readonly Queue<TipsItem> tipsQueue = new Queue<TipsItem>();

        private void Awake()
        {
            item.gameObject.SetActive(false);
        }

        public override void OnShowUI(string title, string info, Action<bool> action)
        {
            OnShowTips(info, 0f);
        }

        public override void OnShowUI(string title, float progress)
        {
            OnShowTips(title, progress);
        }

        private void OnShowTips(string info, float delay)
        {
            var item1 = Global.Pool.GetObject<TipsItem>(item, itemRoot);
            item1.info.text = info;
            item1.showTime = delay <= 0f ? interval : delay;
            item1.hideTime = hideTime;
            tipsQueue.Enqueue(item1);
        }

        private void Update()
        {
            if (tipsQueue.TryPeek(out var tipsItem))
            {
                time += Time.deltaTime;
                if (time > tipsItem.showTime)
                {
                    time = 0f;
                    tipsQueue.Dequeue();
                    tipsItem.transform.SetAsLastSibling();
                    tipsItem.gameObject.SetActive(true);
                }
            }
        }
    }
}