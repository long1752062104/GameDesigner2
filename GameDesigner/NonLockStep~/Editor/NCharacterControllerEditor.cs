#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NonLockStep.Editor
{
    [CustomEditor(typeof(NCharacterController))]
    [CanEditMultipleObjects]
    public class NCharacterControllerEditor : UnityEditor.Editor
    {
        private NCharacterController self;

        private void OnEnable()
        {
            self = (NCharacterController)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("height"), new GUIContent("height", "站立时角色身体的高度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchingHeight"), new GUIContent("crouchingHeight", "角色蹲下时的身体高度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("proneHeight"), new GUIContent("proneHeight", "角色趴下时的身体高度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"), new GUIContent("radius", "角色身体的半径"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("margin"), new GUIContent("margin", "应用于圆柱体的“圆角”半径, 更高的值使圆柱的边缘更加圆滑, 边距包含在圆柱的高度和半径内，因此不能超过圆柱的半径或高度, 要在之后更改碰撞边距, 请使用 CharacterController.CollisionMargin 属性"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mass"), new GUIContent("mass", "角色身体的质量"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumTractionSlope"), new GUIContent("maximumTractionSlope", "角色可以保持牵引力的最陡坡度（以弧度为单位）"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSupportSlope"), new GUIContent("maximumSupportSlope", "角色可以视为支撑的最陡坡度（以弧度为单位）"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standingSpeed"), new GUIContent("standingSpeed", "角色在有提供牵引力的支撑下蹲下时尝试移动的速度, 相对速度较大的将被减速"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchingSpeed"), new GUIContent("crouchingSpeed", "角色在有提供牵引力的支撑下蹲下时尝试移动的速度, 相对速度较大的将被减速"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("proneSpeed"), new GUIContent("proneSpeed", "角色在有提供牵引力的支撑下趴下时尝试移动的速度, 相对速度较大的将被减速"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tractionForce"), new GUIContent("tractionForce", "角色在提供牵引力的支撑上可以施加的最大力量"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slidingSpeed"), new GUIContent("slidingSpeed", "角色在没有提供牵引力的支撑上尝试移动的速度, 相对速度较大的将被减速"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slidingForce"), new GUIContent("slidingForce", "角色在没有提供牵引力的支撑上可以施加的最大力量"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airSpeed"), new GUIContent("airSpeed", "角色在没有支撑的情况下尝试移动的速度, 角色在空中时不会被减速"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airForce"), new GUIContent("airForce", "角色在没有支撑的情况下可以施加的最大力量"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpSpeed"), new GUIContent("jumpSpeed", "角色跳跃时离开地面的速度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slidingJumpSpeed"), new GUIContent("slidingJumpSpeed", "角色在没有牵引力的情况下跳跃时离开地面的速度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumGlueForce"), new GUIContent("maximumGlueForce", "垂直运动约束允许施加的最大力量，以试图将角色保持在地面上"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("includeLayers"), new GUIContent("includeLayers", "该属性用于设置参与碰撞检测的层。通过指定不同的层，您可以控制实体在碰撞时与哪些对象进行交互。这在复杂的物理场景中非常有用，可以优化性能并避免不必要的碰撞检测"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("excludeLayers"), new GUIContent("excludeLayers", "该属性用于设置在碰撞检测中排除的层。通过指定不同的层，您可以控制实体在碰撞时忽略哪些对象。这在需要避免特定对象之间的碰撞时非常有用，例如在复杂的场景中优化物理计算"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initializeMode"), new GUIContent("initializeMode", "该属性用于设置实体在物理系统中的初始化模式。不同的初始化模式可能会影响实体的行为和与其他物体的交互方式"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deinitializeMode"), new GUIContent("deinitializeMode", "该属性用于设置实体在物理系统中去初始化的模式。不同的去初始化模式可能会影响实体在被移除时的行为"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("syncTransform"), new GUIContent("syncTransform", "该属性指示实体的变换（位置和旋转）是否应与物理系统同步。当其值为 true 时，实体的变换将根据物理模拟进行更新。这通常用于确保物理对象在场景中的位置和方向与其物理状态保持一致"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif