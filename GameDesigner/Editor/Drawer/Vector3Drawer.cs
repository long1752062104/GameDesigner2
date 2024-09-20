#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Editor;

namespace Net.Editors
{
    [CustomPropertyDrawer(typeof(Vector3))]
    public class Vector3dDrawer : PropertyDrawerBase
    {
        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var vector3 = (Vector3)reference.GetValue();
            var newVal = EditorGUI.Vector3Field(position, label, vector3);
            reference.SetValue((Vector3)newVal);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18f;
        }
    }

    [CustomPropertyDrawer(typeof(Quaternion))]
    public class QuaternionDrawer : PropertyDrawerBase
    {
        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var quaternion = (Quaternion)reference.GetValue();
            var newVal = EditorGUI.Vector4Field(position, label, quaternion);
            reference.SetValue((Quaternion)newVal);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18f;
        }
    }
}
#endif