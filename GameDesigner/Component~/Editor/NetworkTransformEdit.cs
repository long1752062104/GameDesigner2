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
        if (EditorApplication.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.LabelField("同步模式", nt.currMode.ToString());
            EditorGUILayout.LabelField("被控制时间", nt.currControlTime.ToString("f1"));
            var style = new GUIStyle() { richText = true };
            EditorGUILayout.LabelField($"<color=green>发送同步次数:{nt.WriteCount}</color>", style);
            EditorGUILayout.LabelField($"<color=green>接收同步次数:{nt.ReadCount}</color>", style);
            GUI.enabled = true;
        }
    }
}
#endif