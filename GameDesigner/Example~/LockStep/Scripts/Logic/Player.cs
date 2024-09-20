#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Share;
using System;
using UnityEngine;

namespace LockStep.Client
{
    [Serializable]
    public class Player : Actor
    {
        public ObjectView objectView;
        public float moveSpeed = 6f;
        internal Operation opt;

        public void OnUpdate()
        {
            var dir = (Vector3)opt.direction * moveSpeed;
            if (jCollider != null)
            {
                var vel = jCollider.rigidBody.Velocity;
                if (dir == Net.Vector3.zero)
                {
                    objectView.anim.Play("soldierIdle");

                    dir.y = vel.Y;
                    jCollider.rigidBody.Velocity = dir;
                }
                else
                {
                    objectView.anim.Play("soldierRun");

                    dir.y = vel.Y;
                    jCollider.rigidBody.SetActivationState(true);
                    jCollider.rigidBody.Velocity = dir;
                    jCollider.rigidBody.Orientation = Quaternion.Lerp(jCollider.rigidBody.Orientation, Quaternion.LookRotation(opt.direction, Vector3.up), 0.5f);
                }
            }
            else
            {
                var vel = rigidBody.velocity;
                if (dir == Net.Vector3.zero)
                {
                    objectView.anim.Play("soldierIdle");

                    dir.y = vel.y;
                    rigidBody.velocity = dir;
                }
                else
                {
                    objectView.anim.Play("soldierRun");

                    dir.y = vel.y;
                    rigidBody.velocity = dir;
                    rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, Quaternion.LookRotation(opt.direction, Vector3.up), 0.5f);
                }
            }
        }

        public void Destroy()
        {
            UnityEngine.Object.DestroyImmediate(objectView.gameObject);
        }
    }
}
#endif