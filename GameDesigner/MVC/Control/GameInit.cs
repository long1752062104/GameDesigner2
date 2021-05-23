﻿#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
namespace MVC.Control
{
    using UnityEngine;
    using System.IO;
    using ILRuntime.CLR.Method;
    using System;
    using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
    using ILRuntime.Runtime.CLRBinding;
    using ILRuntime.Runtime.Enviorment;

    public class GameInit : MonoBehaviour
    {
        private AppDomain appdomain;
        private MemoryStream dllStream;
        private MemoryStream pdbStream;
        private IMethod updateMethod;
        public string dllPath;
        public string pdbPath;

        // Start is called before the first frame update
        void Start()
        {
            appdomain = new AppDomain();
#if !UNITY_EDITOR
            dllPath = Application.persistentDataPath + "/Hotfix.dll";
            pdbPath = Application.persistentDataPath + "/Hotfix.pdb";
#endif
            if (File.Exists(dllPath))
                dllStream = new MemoryStream(File.ReadAllBytes(dllPath));
            if (File.Exists(pdbPath))
                pdbStream = new MemoryStream(File.ReadAllBytes(pdbPath));
            appdomain.LoadAssembly(dllStream, pdbStream, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
            CLRBindingUtils.Initialize(appdomain);
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((Action)act)();
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<bool>((value) =>
                {
                    ((Action<bool>)act)(value);
                });
            });
            appdomain.DelegateManager.RegisterMethodDelegate<bool>();
            appdomain.DelegateManager.RegisterMethodDelegate<bool, ILRuntime.Runtime.Intepreter.ILTypeInstance>();

            var method = appdomain.LoadedTypes["Hotfix.GameEntry"].GetMethod("Init", 0);
            appdomain.Invoke(method, null);
            updateMethod = appdomain.LoadedTypes["Hotfix.GameEntry"].GetMethod("Update", 0);
        }

        // Update is called once per frame
        void Update()
        {
            appdomain.Invoke(updateMethod, null);
        }

        private void OnDestroy()
        {
            if (dllStream != null)
                dllStream.Close();
            if (pdbStream != null)
                pdbStream.Close();
            appdomain = null;
        }
    }
}
#endif