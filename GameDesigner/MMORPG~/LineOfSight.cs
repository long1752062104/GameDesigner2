#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace Net.MMORPG
{
    using UnityEngine;

    /// <summary>
    /// 攻击范围可视化组件
    /// </summary>
    public class LineOfSight : MonoBehaviour
    {
        /// <summary>
        /// 攻击半径
        /// </summary>
        public float viewAngle = 120f;
        /// <summary>
        /// 检测半径长度
        /// </summary>
        public float detectionRadius = 3f;
        /// <summary>
        /// 视图偏移
        /// </summary>
        public Vector3 offset = Vector3.up;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = new Color(0.8f, 0.7f, 0.2f, 0.3f);
            UnityEditor.Handles.DrawSolidArc(transform.position + offset, transform.up, transform.forward, viewAngle, detectionRadius);
            UnityEditor.Handles.DrawSolidArc(transform.position + offset, transform.up, transform.forward, -viewAngle, detectionRadius);
        }
#endif
    }
}
#endif