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
    [Header("���ɵĽű����·��(unity)")]
    public string savePath;
    [Header("�ռ�����·��(unity)")]
    public List<string> dllPaths = new List<string>();
    [Header("Rpc����")]
#if UNITY_2020_1_OR_NEWER && UNITY_EDITOR
    [NonReorderable]
#endif
    public List<InvokeHelperConfigData> rpcConfig = new List<InvokeHelperConfigData>();
}

[Serializable]
public class InvokeHelperConfigData 
{
    public string name;
    [Header("VS��Ŀ�ļ�·��")]
    public string csprojPath;
    [Header("���ɵĽű����·��")]
    public string savePath;
    [Header("�ռ�����·��")]
    public List<string> dllPaths = new List<string>();
    [Header("��ȡ��������·��")]
    public string readConfigPath;
}
