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
        GUI.enabled = false;
        EditorGUILayout.LabelField("mode", nt.currMode.ToString());
        EditorGUILayout.LabelField("被控制时间", nt.currControlTime.ToString("f1"));
        GUI.enabled = true;
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