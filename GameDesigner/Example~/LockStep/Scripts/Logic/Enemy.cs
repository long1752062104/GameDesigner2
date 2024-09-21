#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.MMORPG;
using UnityEngine;

namespace LockStep.Client
{
    public class Enemy : Actor
    {
        public RoamingPath roamingPath; // 巡逻点数组
        public float speed = 6f;          // 移动速度
        private int currentPatrolIndex;   // 当前巡逻点索引
        private Vector3 targetPosition;    // 目标位置

        public override void Update()
        {
            // 如果到达目标位置，则选择下一个巡逻点
            if (Vector3.Distance(jRigidBody.Position, targetPosition) < 1.5f)
            {
                currentPatrolIndex = LSRandom.Range(0, roamingPath.waypointsList.Count); // 随机选择下一个巡逻点
                targetPosition = roamingPath.waypointsList[currentPatrolIndex]; // 更新目标位置
            }

            // 移动到目标位置
            Vector3 direction = (targetPosition - jRigidBody.Position).normalized; // 计算方向
            var vel = jRigidBody.Velocity;
            var move = direction * speed; // 使用刚体移动
            move.y = vel.Y;
            jRigidBody.Velocity = move;
            jRigidBody.Rotation = Quaternion.Lerp(jRigidBody.Rotation, Quaternion.LookRotation(direction, Vector3.up), 0.5f);

            animation.Play("walk");
        }
    }
}
#endif