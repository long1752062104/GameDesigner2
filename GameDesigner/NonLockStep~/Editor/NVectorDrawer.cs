#if UNITY_EDITOR
using SoftFloat;
using UnityEngine;
using UnityEditor;
using Unity.Editor;
#if JITTER2_PHYSICS
using Jitter2.LinearMath;
#else
using BEPUutilities;
#endif

namespace NonLockStep.Editor
{
    [CustomPropertyDrawer(typeof(sfloat))]
    public class SFloatDrawer : PropertyDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var fix = (sfloat)reference.GetValue();
            var newVal = EditorGUI.FloatField(position, label, fix);
            reference.SetValue((sfloat)newVal);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }

    [CustomPropertyDrawer(typeof(NVector3))]
    public class Vector3Drawer : PropertyDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var vector3 = (NVector3)reference.GetValue();
            var newVal = EditorGUI.Vector3Field(position, label, vector3);
            reference.SetValue((NVector3)newVal);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }

    [CustomPropertyDrawer(typeof(NQuaternion))]
    public class QuaternionDrawer : PropertyDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = GetFieldReference(property);
            EditorGUI.BeginChangeCheck();
            var quaternion = (NQuaternion)reference.GetValue();
            var newVal = EditorGUI.Vector4Field(position, label, quaternion);
            reference.SetValue((NQuaternion)newVal);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }
}
#endif