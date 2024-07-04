using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    public class UI_Wait : UIBase<UI_Wait>
    {
        public Transform circle;
        public Text waitTip;
        private string message;
        private float waitTime;

        private void OnEnable()
        {
            waitTime = Time.time;
        }

        private void Update()
        {
            circle.Rotate(0, 0, 1f);
            waitTip.text = message + $" ({(int)(Time.time - waitTime)})";
        }

        public override void OnShowUI(string title, string info, Action<bool> action)
        {
            waitTip.text = message = info;
        }

        public override void OnShowUI(string title, float progress)
        {
            waitTip.text = message = title;
        }
    }
}