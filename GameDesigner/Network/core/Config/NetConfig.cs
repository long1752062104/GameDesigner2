﻿using System;
using System.IO;
using System.Collections.Generic;

namespace Net.Config
{
    public class Config
    {
        private static bool init;
        private static int baseCapacity = 1024;
        /// <summary>
        /// 内存接收缓冲区基础容量 默认1024
        /// </summary>
        public static int BaseCapacity
        {
            get
            {
                Init();
                return baseCapacity;
            }
            set
            {
                if (baseCapacity == value)
                    return;
                baseCapacity = value;
                Save();
            }
        }

        /// <summary>
        /// 项目的持久路径, 网络需要处理文件时的目录
        /// </summary>
        public static string BasePath
        {
            get
            {
#if UNITY_STANDALONE || UNITY_WSA || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
                var path = Unity.UnityThreadContext.Get(() => UnityEngine.Application.persistentDataPath);
#else
                var path = AppDomain.CurrentDomain.BaseDirectory;
#endif
                return path;
            }
        }

        /// <summary>
        /// 配置文件的目录
        /// </summary>
        public static string ConfigPath
        {
            get
            {
#if UNITY_STANDALONE || UNITY_WSA || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
                var path = Unity.UnityThreadContext.Get(() => UnityEngine.Application.streamingAssetsPath);
#else
                var path = AppDomain.CurrentDomain.BaseDirectory;
#endif
                return path;
            }
        }

        private static string dataPath;
        /// <summary>
        /// 获取项目路径
        /// </summary>
        public static string DataPath
        {
            get
            {
                if (!string.IsNullOrEmpty(dataPath))
                    return dataPath;
#if UNITY_STANDALONE || UNITY_WSA || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
                dataPath = Unity.UnityThreadContext.Get(() => UnityEngine.Application.dataPath); //根路径必须保证在项目内, 这样编译之后才能读取
#else
                dataPath = AppDomain.CurrentDomain.BaseDirectory;
#endif
                return dataPath;
            }
        }

        private static bool mainThreadTick = false;
        /// <summary>
        /// 在主线程处理所有网络功能? 否则会在多线程进行
        /// </summary>
        public static bool MainThreadTick
        {
            get
            {
                Init();
                return mainThreadTick;
            }
            set
            {
                if (mainThreadTick == value)
                    return;
                mainThreadTick = value;
                Save();
            }
        }

        private static void Init()
        {
            if (init)
                return;
            init = true;
            var configPath = ConfigPath + "/network.config";
#if UNITY_STANDALONE || UNITY_WSA || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
            Unity.UnityThreadContext.Call(LoadConfigFile, configPath);
#else
            if (File.Exists(configPath))
            {
                var textRows = File.ReadAllLines(configPath);
                Init(textRows);
            }
            else
            {
                Save();
            }
#endif

        }

#if UNITY_STANDALONE || UNITY_WSA || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
        private static void LoadConfigFile(string configPath)
        {

#if UNITY_STANDALONE_OSX
            using (var request = UnityEngine.Networking.UnityWebRequest.Get("file:///" + configPath)) //支持mac方式
#else
            using (var request = UnityEngine.Networking.UnityWebRequest.Get(configPath))
#endif
            {
                var oper = request.SendWebRequest();
                while (!oper.isDone)
                    global::System.Threading.Thread.Yield(); //这里只能这样了，使用UniTask.Yield会导致ThreadManager的BeforeSceneLoad先执行，导致Unitask初始化晚于这个后出问题
                if (!string.IsNullOrEmpty(request.error))
                {
                    Save();
                    return;
                }
                var textRows = request.downloadHandler.text.Split(new string[] { "\r\n" }, 0);
                Init(textRows);
            }
        }
#endif

        private static void Init(string[] textRows)
        {
            foreach (var item in textRows)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                var texts = item.Split('=');
                var key = texts[0].Trim().ToLower();
                var value = texts[1].Split('#')[0].Trim();
                switch (key)
                {
                    case "basecapacity":
                        baseCapacity = int.Parse(value);
                        break;
                    case "mainthreadtick":
                        if (bool.TryParse(value, out var value2))
                            mainThreadTick = value2;
                        break;
                }
            }
        }

        private static void Save()
        {
#if UNITY_EDITOR
            var list = new List<string>();
            var text = $"baseCapacity={baseCapacity}#当客户端连接时分配的初始缓冲区大小";
            list.Add(text);
            text = $"mainThreadTick={mainThreadTick}#在Unity主线程处理网络事件? 否则会在多线程处理网络事件";
            list.Add(text);
            var configPath = ConfigPath + "/network.config";
            var path = Path.GetDirectoryName(configPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllLines(configPath, list);
#endif
        }
    }
}
