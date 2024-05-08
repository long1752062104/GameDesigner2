#if UNITY_EDITOR
using Net.Share;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SyncVariable<>))]
public class SyncVariableDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        var valueProperty = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(position, valueProperty, label, true);
        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties(); //应用属性修改后触发属性修改事件才是最新的值
            var targetObject = property.serializedObject.targetObject;
            if (targetObject.GetType() == fieldInfo.DeclaringType)
            {
                var syncVariable = fieldInfo.GetValue(targetObject) as SyncVarInfo;
                syncVariable?.Set();
                if (!EditorApplication.isPlaying)
                    EditorUtility.SetDirty(targetObject);
            }
        }
    }
}
#endif