using UnityEngine;
using UnityEditor;

namespace Framework
{
    [CustomPropertyDrawer(typeof(InterfaceAttribute))]
    public class InterfaceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "InterfaceType Attribute can only be used with MonoBehaviour Components.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var old = property.objectReferenceValue;

            Component currentComponent = EditorGUI.ObjectField(position, label, old, typeof(Component), true) as Component;
            
            EditorGUI.EndProperty();
        }
    }
}