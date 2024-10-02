#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using SoftFloat;
using System;
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using System.Collections.Generic;
#else
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.Entities;
#endif

namespace NonLockStep
{
    [Flags]
    public enum Freeze
    {
        X = 1,
        Y = 2,
        Z = 4,
    }

    public enum InitMode
    {
        None = 0,
        Awake,
        Start,
        OnEnable,
    }

    public abstract class NCollider : MonoBehaviour
    {
        public bool isStatic;
        public bool isTriggered;
        public sfloat friction = 0.2f;
        public sfloat linear, angular;
        public Freeze freezeRot;
        public LayerMask includeLayers;
        public LayerMask excludeLayers;
        public Vector3 center;
#if JITTER2_PHYSICS
        public RigidBody rigidBody;
        internal List<RigidBodyShape> shapes;
#else
        public Entity rigidBody;
#endif
        private bool isInitialize;
        public InitMode initMode = InitMode.Start;
        public object Tag; //存特别引用

#if JITTER2_PHYSICS
        public Action<NCollider, Arbiter> onTriggerEnter;
        public Action<NCollider, Arbiter> onTriggerExit;
        public Action<NCollider, Arbiter> onCollisionEnter;
        public Action<NCollider, Arbiter> onCollisionExit;

        public NQuaternion Rotation { get => rigidBody.Orientation; set => rigidBody.Orientation = value; }
        public NVector3 Position { get => rigidBody.Position; set => rigidBody.Position = value; }
        public NVector3 Velocity { get => rigidBody.Velocity; set { rigidBody.SetActivationState(true); rigidBody.Velocity = value; } }
#else
        public NQuaternion Rotation { get => rigidBody.Orientation; set => rigidBody.Orientation = value; }
        public NVector3 Position { get => rigidBody.Position; set => rigidBody.Position = value; }
        public NVector3 Velocity { get => rigidBody.LinearVelocity; set { rigidBody.LinearVelocity = value; } }
#endif

        public virtual void Awake()
        {
            if (initMode == InitMode.Awake)
                Initialize();
        }

        public virtual void Start()
        {
            if (initMode == InitMode.Start)
                Initialize();
        }

        public virtual void OnEnable()
        {
            if (initMode == InitMode.OnEnable)
                Initialize();
        }

        public void Initialize()
        {
            if (isInitialize)
                return;
            isInitialize = true;
            InitRigidBody();
        }

        public void InitRigidBody()
        {
            var world = NPhysics.I.world;
#if JITTER2_PHYSICS
            rigidBody = world.CreateRigidBody();
            rigidBody.AddShape(shapes = OnCreateShape(), false);
            rigidBody.Position = transform.position.ToNVector3();
            rigidBody.Orientation = transform.rotation.ToQuaternion();
            rigidBody.IsStatic = isStatic;
            rigidBody.IsTriggered = isTriggered;
            rigidBody.Friction = friction;
            rigidBody.Damping = (linear, angular);
            rigidBody.BeginCollide = BeginCollide;
            rigidBody.EndCollide = EndCollide;
            rigidBody.Tag = this;
            rigidBody.IncludeLayers = includeLayers;
            rigidBody.ExcludeLayers = excludeLayers;
            if (freezeRot != 0)
            {
                var freeze = NVector3.Zero;
                if ((freezeRot & Freeze.X) == Freeze.X)
                    freeze.X = 1f;
                if ((freezeRot & Freeze.Y) == Freeze.Y)
                    freeze.Y = 1f;
                if ((freezeRot & Freeze.Z) == Freeze.Z)
                    freeze.Z = 1f;
                var ur = world.CreateConstraint<HingeAngle>(rigidBody, world.NullBody);
                ur.Initialize(freeze, AngularLimit.Full);
            }
#else
            var worldObject = OnCreateEntity(transform.position);
            rigidBody = worldObject as Entity;
            if (rigidBody != null)
            {
                rigidBody.Orientation = transform.rotation;
                rigidBody.Tag = this;
            }
            world.Add(worldObject);
#endif
        }

#if JITTER2_PHYSICS
        private void BeginCollide(Arbiter arbiter)
        {
            NCollider other;
            if (arbiter.Body1 == rigidBody)
                other = (NCollider)arbiter.Body2.Tag;
            else
                other = (NCollider)arbiter.Body1.Tag;
            if (arbiter.Body1.IsTriggered | arbiter.Body2.IsTriggered)
                onTriggerEnter?.Invoke(other, arbiter);
            else
                onCollisionEnter?.Invoke(other, arbiter);
        }

        private void EndCollide(Arbiter arbiter)
        {
            NCollider other;
            if (arbiter.Body1 == rigidBody)
                other = (NCollider)arbiter.Body2.Tag;
            else
                other = (NCollider)arbiter.Body1.Tag;
            if (arbiter.Body1.IsTriggered | arbiter.Body2.IsTriggered)
                onTriggerExit?.Invoke(other, arbiter);
            else
                onCollisionExit?.Invoke(other, arbiter);
        }
#endif

        private void Update()
        {
            if (rigidBody == null)
                return;
            transform.SetPositionAndRotation(rigidBody.Position, rigidBody.Orientation);
        }

#if JITTER2_PHYSICS
        public abstract List<RigidBodyShape> OnCreateShape();
#else
        public abstract IWorldObject OnCreateEntity(NVector3 position);
#endif

        public NVector3 TransformDirection(NVector3 direction)
        {
            return Rotation * direction;
        }

        public NVector3 TransformPoint(NVector3 direction)
        {
            var forwardOffset = Rotation * direction;
            return Position + forwardOffset;
        }

        public void Translate(NVector3 translation)
        {
            Translate(translation, Space.Self);
        }

        public void Translate(sfloat x, sfloat y, sfloat z, Space relativeTo = Space.Self)
        {
            Translate(new NVector3(x, y, z), relativeTo);
        }

        public void Translate(sfloat x, sfloat y, sfloat z)
        {
            Translate(new NVector3(x, y, z), Space.Self);
        }

        public void Translate(NVector3 translation, Space relativeTo = Space.Self)
        {
            if (relativeTo == Space.World)
                translation *= 30f;
            else
                translation = TransformDirection(translation) * 30f;
            var vel = Velocity;
            vel.X = translation.X;
            vel.Z = translation.Z;
            Velocity = vel;
        }

        public void LookAt(NVector3 targetPosition, NVector3 worldUp)
        {
            var direction = targetPosition - Position;
            rigidBody.Orientation = NQuaternion.LookRotation(direction, worldUp);
        }
    }
}
#endif