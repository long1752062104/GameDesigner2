#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InvokeHelperConfigObject))]
[CanEditMultipleObjects]
public class InvokeHelperConfigEdit : Editor
{
    private InvokeHelperConfigObject config;

    private void OnEnable()
    {
        config = target as InvokeHelperConfigObject;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("±£¥Ê≈‰÷√", GUILayout.Height(30)))
        {
            InvokeHelperTools.Config = config.Config;
            InvokeHelperTools.SaveData();
        }
    }
}
#endif