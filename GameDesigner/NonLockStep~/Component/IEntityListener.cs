#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
#if JITTER2_PHYSICS
using Jitter2.Dynamics;
using Jitter2.LinearMath;
#else
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities;
#endif

namespace NonLockStep
{
    public interface IEntityListener
    {
    }

    public interface ICollisionEnterListener : IEntityListener
    {
        /// <summary>
        /// 当进入碰撞
        /// </summary>
        /// <param name="other">碰撞到的刚体组件</param>
        /// <param name="collisionInfo">碰撞信息，如果使用，请使用<see cref="NCollision.GetCollision(object)"/>获取碰撞信息</param>
        void OnNCollisionEnter(NRigidbody other, object collisionInfo);
    }

    public interface ICollisionStayListener : IEntityListener
    {
        /// <summary>
        /// 当逗留碰撞
        /// </summary>
        /// <param name="other">碰撞到的刚体组件</param>
        /// <param name="collisionInfo">碰撞信息，如果使用，请使用<see cref="NCollision.GetCollision(object)"/>获取碰撞信息</param>
        void OnNCollisionStay(NRigidbody other, object collisionInfo);
    }

    public interface ICollisionExitListener : IEntityListener
    {
        /// <summary>
        /// 当退出碰撞
        /// </summary>
        /// <param name="other">碰撞到的刚体组件</param>
        /// <param name="collisionInfo">碰撞信息，如果使用，请使用<see cref="NCollision.GetCollision(object)"/>获取碰撞信息</param>
        void OnNCollisionExit(NRigidbody other, object collisionInfo);
    }

    public interface ITriggerEnterListener : IEntityListener
    {
        /// <summary>
        /// 当进入触发器
        /// </summary>
        /// <param name="other">碰撞到的刚体组件</param>
        /// <param name="collisionInfo">碰撞信息，如果使用，请使用<see cref="NCollision.GetCollision(object)"/>获取碰撞信息</param>
        void OnNTriggerEnter(NRigidbody other, object collisionInfo);
    }

    public interface ITriggerStayListener : IEntityListener
    {
        /// <summary>
        /// 当逗留触发器
        /// </summary>
        /// <param name="other">碰撞到的刚体组件</param>
        /// <param name="collisionInfo">碰撞信息，如果使用，请使用<see cref="NCollision.GetCollision(object)"/>获取碰撞信息</param>
        void OnNTriggerStay(NRigidbody other, object collisionInfo);
    }

    public interface ITriggerExitListener : IEntityListener
    {
        /// <summary>
        /// 当退出触发器
        /// </summary>
        /// <param name="other">碰撞到的刚体组件</param>
        /// <param name="collisionInfo">碰撞信息，如果使用，请使用<see cref="NCollision.GetCollision(object)"/>获取碰撞信息</param>
        void OnNTriggerExit(NRigidbody other, object collisionInfo);
    }

    public interface ICollisionListener : ICollisionEnterListener, ICollisionStayListener, ICollisionExitListener
    {
    }

    public interface ITriggerListener : ITriggerEnterListener, ITriggerStayListener, ITriggerExitListener
    {
    }

    public class NCollision
    {
        public NContactPoint[] contacts;

        public static NCollision GetCollision(object pairObj)
        {
#if !JITTER2_PHYSICS
            var pair = (CollidablePairHandler)pairObj;
            var collision = new NCollision
            {
                contacts = new NContactPoint[pair.Contacts.Count]
            };
            for (int i = 0; i < pair.Contacts.Count; i++)
            {
                var contact = pair.Contacts[i].Contact;
                collision.contacts[i] = new NContactPoint()
                {
                    Point = contact.Position,
                    Normal = contact.Normal,
                };
            }
#else
            var arbiter = (Arbiter)pairObj;
            ref var cq = ref arbiter.Handle.Data;
            NContactPoint contact = default;
            void DrawContact(in ContactData cq, in ContactData.Contact c)
            {
                var v1 = c.RelativePos1 + cq.Body1.Data.Position;
                var v2 = c.RelativePos2 + cq.Body2.Data.Position;
                contact.Point = v1;
                contact.Normal = c.Normal;
            }
            if ((cq.UsageMask & ContactData.MaskContact0) != 0) DrawContact(cq, cq.Contact0);
            if ((cq.UsageMask & ContactData.MaskContact1) != 0) DrawContact(cq, cq.Contact1);
            if ((cq.UsageMask & ContactData.MaskContact2) != 0) DrawContact(cq, cq.Contact2);
            if ((cq.UsageMask & ContactData.MaskContact3) != 0) DrawContact(cq, cq.Contact3);
            var collision = new NCollision
            {
                contacts = new NContactPoint[] { contact }
            };
#endif
            return collision;
        }
    }

    public struct NContactPoint
    {
        public NVector3 Point;
        public NVector3 Normal;
    }
}
#endif