#if UNITY_EDITOR
using Net.UnityComponent;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkTransformMulti))]
[CanEditMultipleObjects]
public class NetworkTransformMultiEdit : Editor
{
    private NetworkTransformMulti nt;

    private void OnEnable()
    {
        nt = target as NetworkTransformMulti;
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
        if (GUILayout.Button("更新子物体"))
        {
            var childs1 = nt.transform.GetComponentsInChildren<Transform>();
            var list = new List<ChildTransform>();
            foreach (var child in childs1)
            {
                if (child == nt.transform)
                    continue;
                if (!child.gameObject.activeInHierarchy)
                    continue;
                list.Add(new ChildTransform()
                {
                    name = child.name,
                    transform = child,
                    syncPosition = nt.syncPosition,
                    syncRotation = nt.syncRotation,
                    syncScale = nt.syncScale,
                });
            }
            nt.childs = list.ToArray();
            EditorUtility.SetDirty(nt);
        }
    }
}
#endif