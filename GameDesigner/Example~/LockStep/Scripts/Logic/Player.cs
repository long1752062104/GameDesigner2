#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using UnityEngine;

namespace LockStep.Client
{
    [Serializable]
    public class Player : Actor
    {
        public Animation anim;
        public float moveSpeed = 6f;

        public override void Update()
        {
            var dir = (Vector3)operation.direction * moveSpeed;
            if (jCollider != null)
            {
                var vel = jCollider.Velocity;
                if (dir == Net.Vector3.zero)
                {
                    anim.Play("soldierIdle");

                    dir.y = vel.Y;
                    jCollider.Velocity = dir;
                }
                else
                {
                    anim.Play("soldierRun");

                    dir.y = vel.Y;
                    jCollider.Velocity = dir;
                    jCollider.Rotation = Quaternion.Lerp(jCollider.Rotation, Quaternion.LookRotation(operation.direction, Vector3.up), 0.5f);
                }
            }
            else
            {
                var vel = rigidBody.velocity;
                if (dir == Net.Vector3.zero)
                {
                    anim.Play("soldierIdle");

                    dir.y = vel.y;
                    rigidBody.velocity = dir;
                }
                else
                {
                    anim.Play("soldierRun");

                    dir.y = vel.y;
                    rigidBody.velocity = dir;
                    rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, Quaternion.LookRotation(operation.direction, Vector3.up), 0.5f);
                }
            }
        }
    }
}
#endif