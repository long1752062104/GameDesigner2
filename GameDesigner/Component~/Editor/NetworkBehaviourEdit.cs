#if UNITY_EDITOR
using UnityEditor;
using Net.UnityComponent;

[CustomEditor(typeof(NetworkBehaviour), editorForChildClasses: true)]
[CanEditMultipleObjects]
public class NetworkBehaviourEdit : Editor
{
    private NetworkBehaviour nb;

    private void OnEnable()
    {
        nb = target as NetworkBehaviour;
        nb.CheckNetworkObjectIsNull();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif
