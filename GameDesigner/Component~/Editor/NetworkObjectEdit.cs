#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Net.UnityComponent;

[CustomEditor(typeof(NetworkObject))]
[CanEditMultipleObjects]
public class NetworkObjectEdit : Editor
{
    private NetworkObject no;
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        no = target as NetworkObject;
        var networkBehaviours = no.GetComponentsInChildren<NetworkBehaviour>(true);
        for (int i = 0; i < networkBehaviours.Length; i++)
        {
            var networkBehaviour = networkBehaviours[i];
            if (networkBehaviour.NetComponentID == -1)
            {
                networkBehaviour.NetComponentID = i;
                EditorUtility.SetDirty(networkBehaviour);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUI.enabled = false;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        var style = new GUIStyle() { richText = true };
        foreach (var item in no.syncVarInfos.Values)
        {
            EditorGUILayout.LabelField(item.ToColorString(item.IsDispose ? "#808080" : "green"), style);
        }
        EditorGUILayout.EndScrollView();
        GUI.color = Color.yellow;
        EditorGUILayout.LabelField("Network Identity", no.Identity.ToString());
        GUI.enabled = true;
    }
}
#endif