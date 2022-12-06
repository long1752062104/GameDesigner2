using System;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// ����򿪲���ģʽ
    /// </summary>
    public enum UIFormMode 
    {
        /// <summary>
        /// �����κ���Ӧ
        /// </summary>
        None,
        /// <summary>
        /// �رյ�ǰ����, �����µĽ���
        /// </summary>
        CloseCurrForm,
        /// <summary>
        /// ֻ���ص�ǰ����, Ȼ����µĽ���
        /// </summary>
        HideCurrForm,
    }


    /// <summary>
    /// UI�������
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
            throw new Exception($"����дOnShowUI�������������Ϣ��ʾ");
        }

        public virtual void OnShowUI(string title, float progress)
        {
            throw new Exception($"����дOnShowUI����������Ľ��ȼ��ؽ���");
        }
    }

    /// <summary>
    /// UI�������
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