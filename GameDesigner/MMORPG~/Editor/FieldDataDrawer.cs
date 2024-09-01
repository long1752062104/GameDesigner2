using UnityEditor;
using UnityEngine;

namespace Net.MMORPG 
{
    [CustomPropertyDrawer(typeof(FieldData))]
    public class FieldDataDrawer : PropertyDrawer
    {
        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}