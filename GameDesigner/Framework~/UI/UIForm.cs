using System;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 界面打开操作模式
    /// </summary>
    public enum UIFormMode 
    {
        /// <summary>
        /// 不做任何响应
        /// </summary>
        None,
        /// <summary>
        /// 关闭当前界面, 并打开新的界面
        /// </summary>
        CloseCurrForm,
        /// <summary>
        /// 只隐藏当前界面, 然后打开新的界面
        /// </summary>
        HideCurrForm,
    }


    /// <summary>
    /// UI界面基类
    /// </summary>
    public class UIFormBase : MonoBehaviour, IForm
    {
        public Action onBack;

        public void ShowUI(Action onBack = null)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (onBack != null)
                this.onBack = onBack;
        }

        public void ShowUI(string title, string info, Action<bool> action, Action onBack = null)
        {
            ShowUI(onBack);
            OnShowUI(title, info, action);
        }

        public void ShowUI(string title, float progress, Action onBack = null)
        {
            ShowUI(onBack);
            OnShowUI(title, progress);
        }

        public void HideUI(bool isBack = true)
        {
            gameObject.SetActive(false);
            if (isBack & onBack != null)
            {
                onBack();
                onBack = null;
            }
        }

        public virtual void OnShowUI(string title, string info, Action<bool> action)
        {
            throw new Exception($"请重写OnShowUI方法处理你的消息提示");
        }

        public virtual void OnShowUI(string title, float progress)
        {
            throw new Exception($"请重写OnShowUI方法处理你的进度加载界面");
        }
    }

    /// <summary>
    /// UI界面基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UIFormBase<T> : UIFormBase where T : UIFormBase<T>
    {
        public static T Show(Action onBack = null, UIFormMode formMode = UIFormMode.CloseCurrForm)
        {
            var form = Global.UI.OpenForm<T>(onBack, formMode);
            return form;
        }

        public static void Show(string title, string info, Action<bool> result = null)
        {
            var i = Show(null, UIFormMode.None);
            i.OnShowUI(title, info, result);
        }

        public static void Hide(bool isBack = true)
        {
            Global.UI.CloseForm<T>(isBack);
        }
    }
}