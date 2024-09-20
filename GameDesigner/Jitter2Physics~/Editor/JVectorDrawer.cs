#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Jitter2.LinearMath;
using Unity.Editor;
using FixedMath;

namespace GGPhysUnity.Editor
{
    [CustomPropertyDrawer(typeof(Fix64))]
    public class Fix64Drawer : PropertyDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var fix = (Fix64)reference.GetValue();
            var newVal = EditorGUI.FloatField(position, label, fix);
            reference.SetValue(new Fix64(newVal));
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18f;
        }
    }

    [CustomPropertyDrawer(typeof(JVector))]
    public class Vector3dDrawer : PropertyDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var vector3 = (JVector)reference.GetValue();
            var newVal = EditorGUI.Vector3Field(position, label, vector3.ToVector3());
            reference.SetValue(newVal.ToJVector());
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18f;
        }
    }

    [CustomPropertyDrawer(typeof(JQuaternion))]
    public class QuaternionDrawer : PropertyDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var quaternion = (JQuaternion)reference.GetValue();
            var newVal = EditorGUI.Vector4Field(position, label, quaternion.ToVector4());
            reference.SetValue(newVal.ToQuaternion());
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