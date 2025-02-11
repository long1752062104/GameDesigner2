using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// ui管理器, 内置两个基础界面 Message和Loading, 请分写此组件定义你的其他界面
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public Transform UIRoot;
        public Transform[] Levels;
        public string sheetName = "UI";
        public Dictionary<string, UIBase> formDict = new Dictionary<string, UIBase>();
        public Stack<UIBase> formStack = new Stack<UIBase>();
        public IUserInterface Loading, Message, Tips, Wait;
        [SerializeField] private UIBase _Loading;
        [SerializeField] private UIBase _Message;
        [SerializeField] private UIBase _Tips;
        [SerializeField] private UIBase _Wait;
        [SerializeField] private UIBase[] _Forms;

        public virtual void Awake()
        {
            Loading = _Loading;
            Message = _Message;
            Tips = _Tips;
            Wait = _Wait;
            AddForm(_Loading);
            AddForm(_Message);
            AddForm(_Tips);
            foreach (var form in _Forms)
            {
                AddForm(form);
                form.HideUI(false);
            }
        }

        /// <summary>
        /// 将form界面添加到所有界面字典里面
        /// </summary>
        /// <param name="form"></param>
        public void AddForm(UIBase form)
        {
            if (form == null)
                return;
            formDict[form.GetType().Name] = form;
        }

        /// <summary>
        /// 打开一个界面, 如果不存在则从ab包加载, 如果存在直接显示并返回
        /// </summary>
        /// <typeparam name="T">ui表列的名称</typeparam>
        /// <param name="onBack">当关闭界面后回调</param>
        /// <param name="formMode">当前界面响应模式</param>
        /// <returns></returns>
        public T OpenForm<T>(Delegate onBack = null, UIMode formMode = UIMode.CloseCurrUI, params object[] pars) where T : UIBase<T>
        {
            var formName = typeof(T).Name;
            return OpenForm(formName, onBack, formMode, pars) as T;
        }

        /// <summary>
        /// 打开一个界面, 如果不存在则从ab包加载, 如果存在直接显示并返回
        /// </summary>
        /// <typeparam name="T">ui表列的名称</typeparam>
        /// <param name="onBack">当关闭界面后回调</param>
        /// <param name="formMode">当前界面响应模式</param>
        /// <returns></returns>
        public void OpenForm(UIBase uiForm, Delegate onBack = null, UIMode formMode = UIMode.CloseCurrUI, params object[] pars)
        {
            var formName = uiForm.GetType().Name;
            OpenForm(formName, onBack, formMode);
        }

        /// <summary>
        /// 打开一个界面, 如果不存在则从ab包加载, 如果存在直接显示并返回
        /// </summary>
        /// <param name="formName">ui表列的名称</param>
        /// <param name="onBack">当关闭界面后回调</param>
        /// <param name="formMode">当前界面响应模式</param>
        /// <returns></returns>
        public virtual UIBase OpenForm(string formName, Delegate onBack = null, UIMode formMode = UIMode.CloseCurrUI, params object[] pars)
        {
            if (formDict.TryGetValue(formName, out var form))
                if (form != null)
                    goto J;
            form = InstantiateForm(formName);
            formDict[formName] = form;
        J:
            var openMultiple = false;
            if (formStack.Count > 0)
            {
                UIBase form1;
                switch (formMode)
                {
                    case UIMode.HideCurrUI:
                        form1 = formStack.Peek();//只是隐藏当前界面不能弹出
                        form1.HideUI(false);
                        openMultiple = form1 == form; //控制不能多次压入UI栈
                        break;
                    case UIMode.CloseCurrUI:
                        form1 = formStack.Pop();//关闭上一个界面需要弹出
                        form1.HideUI(false);
                        break;
                    case UIMode.None://不做任何动作, Message消息框
                        form1 = formStack.Peek();
                        openMultiple = form1 == form; //控制不能多次压入UI栈
                        break;
                }
            }
            form.ShowUI(onBack, pars);
            if (formMode != UIMode.EmptyStack && !openMultiple)
                formStack.Push(form);//如果是消息框, 一定会关闭了才能再次打开, 不存在多次压入
            return form;
        }

        protected virtual UIBase InstantiateForm(string formName)
        {
            var dataTable = Global.Table.GetTable(sheetName);
            var dataRows = dataTable.Select($"Name = '{formName}'");
            if (dataRows.Length == 0)
                throw new Exception($"找不到界面:{formName}, 请配置!");
            var path = ObjectConverter.AsString(dataRows[0]["Path"]);
            var level = ObjectConverter.AsInt(dataRows[0]["Level"]);
            var form = Global.Resources.Instantiate<UIBase>(path, Levels[level]);
            return form;
        }

        public void CloseForm<T>(bool isBack, bool navigation = true, params object[] pars)
        {
            var formName = typeof(T).Name;
            CloseForm(formName, isBack, navigation, pars);
        }

        public void CloseForm(string formName, bool isBack = true, bool navigation = true, params object[] pars)
        {
            if (formDict.TryGetValue(formName, out var form))
            {
                CloseForm(form, isBack, navigation, pars);
            }
        }

        public virtual void CloseForm(UIBase form, bool isBack = true, bool navigation = true, params object[] pars)
        {
            if (formStack.Count > 0)
            {
                if (form != formStack.Peek())
                {
                    form.HideUI(isBack, pars);
                    return;
                }
                form = formStack.Pop();//弹出自己的面板
                form.HideUI(isBack, pars);
                if (formStack.Count > 0 && navigation)
                {
                    form = formStack.Peek();//弹出上一个界面进行显示, 但不会移除
                    form.ShowUI();
                }
            }
            else
            {
                form.HideUI(isBack, pars);
            }
        }

        public T GetForm<T>() where T : UIBase
        {
            var formName = typeof(T).Name;
            return GetForm(formName) as T;
        }

        public UIBase GetForm(string formName)
        {
            formDict.TryGetValue(formName, out var form);
            return form;
        }

        public T GetFormOrCreate<T>(bool isShow = false) where T : UIBase
        {
            var formName = typeof(T).Name;
            return GetFormOrCreate(formName, isShow) as T;
        }

        public UIBase GetFormOrCreate(string formName, bool isShow = false)
        {
            var form = GetForm(formName);
            if (form != null)
                return form;
            form = InstantiateForm(formName);
            form.gameObject.SetActive(isShow);
            formDict[formName] = form;
            return form;
        }

        public bool IsCurrentUI(UIBase form)
        {
            if (formStack.TryPeek(out var result))
                return form == result;
            return false;
        }

        public void CloseFormAll()
        {
            while (formStack.Count > 0)
            {
                var form = formStack.Pop();
                form.HideUI();
            }
        }

        public RectTransform GetUILayer(int uiLayer)
        {
            return (RectTransform)Levels[uiLayer];
        }
    }
}