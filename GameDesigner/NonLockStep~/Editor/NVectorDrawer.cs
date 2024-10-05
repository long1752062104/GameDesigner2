#if UNITY_EDITOR
using SoftFloat;
using UnityEngine;
using UnityEditor;
using Unity.Editor;
#if JITTER2_PHYSICS
using Jitter2.LinearMath;
#else
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Entities.Prefabs;
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

    [CustomPropertyDrawer(typeof(NPhysicsAxisConstraints))]
    public class NPhysicsAxisConstraintsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var xRect = new Rect(position.x + 0, position.y, 15, position.height);
            var xLabelRect = new Rect(position.x + 15, position.y, 15, position.height);
            var yRect = new Rect(position.x + 30, position.y, 15, position.height);
            var yLabelRect = new Rect(position.x + 45, position.y, 15, position.height);
            var zRect = new Rect(position.x + 60, position.y, 15, position.height);
            var zLabelRect = new Rect(position.x + 75, position.y, 15, position.height);

            EditorGUI.LabelField(xLabelRect, "X");
            EditorGUI.LabelField(yLabelRect, "Y");
            EditorGUI.LabelField(zLabelRect, "Z");

            EditorGUI.PropertyField(xRect, property.FindPropertyRelative("X"), GUIContent.none);
            EditorGUI.PropertyField(yRect, property.FindPropertyRelative("Y"), GUIContent.none);
            EditorGUI.PropertyField(zRect, property.FindPropertyRelative("Z"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
#endif