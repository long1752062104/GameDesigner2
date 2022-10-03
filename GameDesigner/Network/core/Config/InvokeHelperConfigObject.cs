using System.Collections.Generic;
using System;
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
using UnityEngine;

[CreateAssetMenu(menuName = "Create InvokeHelperConfig")]
public class InvokeHelperConfigObject : ScriptableObject
{
    public InvokeHelperConfig Config = new InvokeHelperConfig();
}
#elif SERVICE
public class Header : Attribute 
{
    public Header(string text) { }
}
#endif
[Serializable]
public class InvokeHelperConfig
{
    [Header("生成的脚本存放路径(unity)")]
    public string savePath;
    [Header("收集程序集路径(unity)")]
    public List<string> dllPaths = new List<string>();
    [Header("Rpc辅助")]
#if UNITY_2020_1_OR_NEWER && UNITY_EDITOR
    [NonReorderable]
#endif
    public List<InvokeHelperConfigData> rpcConfig = new List<InvokeHelperConfigData>();
}

[Serializable]
public class InvokeHelperConfigData 
{
    public string name;
    [Header("VS项目文件路径")]
    public string csprojPath;
    [Header("生成的脚本存放路径")]
    public string savePath;
    [Header("收集程序集路径")]
    public List<string> dllPaths = new List<string>();
    [Header("读取配置数据路径")]
    public string readConfigPath;
}
