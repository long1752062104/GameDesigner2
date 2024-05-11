#if UNITY_EDITOR
using Net.UnityComponent;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkTransform))]
[CanEditMultipleObjects]
public class NetworkTransformEdit : Editor
{
    private NetworkTransform nt;

    private void OnEnable()
    {
        nt = target as NetworkTransform;
        nt.CheckNetworkObjectIsNull();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUI.enabled = false;
        EditorGUILayout.LabelField("mode", nt.currMode.ToString());
        EditorGUILayout.LabelField("被控制时间", nt.currControlTime.ToString("f1"));
        GUI.enabled = true;
    }
}
#endif