#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NonLockStep.Editor
{
    [CustomEditor(typeof(NRigidbody))]
    [CanEditMultipleObjects]
    public class NRigidbodyEditor : UnityEditor.Editor
    {
        private NRigidbody self;

        private void OnEnable()
        {
            self = (NRigidbody)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mass"), new GUIContent("mass", "获取或设置实体的质量。将其设置为无效值，例如非正数、NaN 或无穷大，会使实体变为运动学（kinematic）, 将其设置为有效的正数也会缩放惯性张量（inertia tensor），如果它已经是动态的，或者强制计算新的惯性张量, 如果它之前是运动学的"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("angularDamping"), new GUIContent("angularDamping", "获取或设置实体的角阻尼（angular damping）, 值的范围从 0 到 1，对应于在单位时间内从实体中移除的角动量的比例"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("linearDamping"), new GUIContent("linearDamping", "获取或设置实体的线性阻尼（linear damping）, 值的范围从 0 到 1，对应于在单位时间内从实体中移除的线动量的比例"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isKinematic"), new GUIContent("isKinematic", "该属性指示实体是否为运动学（kinematic）状态。当其值为 true 时，实体不会受到物理力的影响，只能通过直接设置其位置来移动"));
#if !JITTER2_PHYSICS
            EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionDetection"), new GUIContent("collisionDetection", "该属性用于设置实体的碰撞检测模式。它决定了如何检测和处理碰撞。常见的碰撞检测模式包括：\r\nDiscrete: 离散碰撞检测，适用于大多数情况。\r\nContinuous: 连续碰撞检测，适用于快速移动的物体，以减少穿透现象。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("kineticFriction"), new GUIContent("kineticFriction", "该属性用于设置实体的动摩擦系数。它决定了当实体处于运动状态时施加的摩擦力大小。较高的值会导致更大的摩擦力，从而减缓实体的运动"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("staticFriction"), new GUIContent("staticFriction", "该属性用于设置实体的静摩擦系数。它决定了当实体处于静止状态时施加的摩擦力大小。较高的值可以防止实体在未施加足够的外力时开始移动"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bounciness"), new GUIContent("bounciness", "该属性用于设置实体的弹性系数。它决定了实体在与其他物体碰撞时的反弹程度。值通常在 0 到 1 之间，0 表示没有反弹（完全吸收碰撞能量），1 表示完全反弹（不损失能量）"));
#else
            EditorGUILayout.PropertyField(serializedObject.FindProperty("friction"), new GUIContent("friction", "该属性用于设置实体的摩擦系数。它决定了在运动过程中施加于实体的整体摩擦力。较高的摩擦值会导致实体减速更快，而较低的摩擦值则允许实体在表面上滑动得更远"));
#endif
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedPosition"), new GUIContent("fixedPosition", "该属性指示实体的位置是否是固定的。当其值为 true 时，实体的位置无法被物理引擎或其他力改变。这通常用于需要保持在特定位置的对象"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedRotation"), new GUIContent("fixedRotation", "该属性指示实体的旋转是否是固定的。当其值为 true 时，实体的旋转无法被物理引擎或其他力改变。这通常用于需要保持特定方向的对象，例如某些游戏角色或静态物体"));

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