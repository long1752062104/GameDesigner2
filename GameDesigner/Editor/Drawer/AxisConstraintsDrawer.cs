#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Net.Unity
{
    [CustomPropertyDrawer(typeof(AxisConstraints))]
    public class AxisConstraintsDrawer : PropertyDrawer
    {
        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, GUIContent label)
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

    [CustomPropertyDrawer(typeof(Axis2Constraints))]
    public class Axis2ConstraintsDrawer : PropertyDrawer
    {
        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, GUIContent label)
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
            var wRect = new Rect(position.x + 90, position.y, 15, position.height);
            var wLabelRect = new Rect(position.x + 105, position.y, 15, position.height);

            EditorGUI.LabelField(xLabelRect, "X");
            EditorGUI.LabelField(yLabelRect, "Y");
            EditorGUI.LabelField(zLabelRect, "Z");
            EditorGUI.LabelField(wLabelRect, "W");

            EditorGUI.PropertyField(xRect, property.FindPropertyRelative("X"), GUIContent.none);
            EditorGUI.PropertyField(yRect, property.FindPropertyRelative("Y"), GUIContent.none);
            EditorGUI.PropertyField(zRect, property.FindPropertyRelative("Z"), GUIContent.none);
            EditorGUI.PropertyField(wRect, property.FindPropertyRelative("W"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
#endif