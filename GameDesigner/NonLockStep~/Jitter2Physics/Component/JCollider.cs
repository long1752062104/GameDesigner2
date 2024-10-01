#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using SoftFloat;
using System;
using System.Collections.Generic;
using UnityEngine;

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

public abstract class JCollider : MonoBehaviour
{
    public bool isStatic;
    public bool isTriggered;
    public sfloat friction = 0.2f;
    public sfloat linear, angular;
    public Freeze freezeRot;
    public LayerMask includeLayers;
    public LayerMask excludeLayers;
    public Vector3 center;
    public RigidBody rigidBody;
    internal List<RigidBodyShape> shapes;
    private bool isInitialize;
    public InitMode initMode = InitMode.Start;
    public object Tag; //存特别引用
    
    public Action<JCollider, Arbiter> onTriggerEnter;
    public Action<JCollider, Arbiter> onTriggerExit;
    public Action<JCollider, Arbiter> onCollisionEnter;
    public Action<JCollider, Arbiter> onCollisionExit;

    public JQuaternion Rotation { get => rigidBody.Orientation; set => rigidBody.Orientation = value; }
    public JVector Position { get => rigidBody.Position; set => rigidBody.Position = value; }
    public JVector Velocity { get => rigidBody.Velocity; set { rigidBody.SetActivationState(true); rigidBody.Velocity = value; } }

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
        var world = JPhysics.I.world;
        rigidBody = world.CreateRigidBody();
        rigidBody.AddShape(shapes = OnCreateShape(), false);
        rigidBody.Position = transform.position.ToJVector();
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
            var freeze = JVector.Zero;
            if ((freezeRot & Freeze.X) == Freeze.X)
                freeze.X = 1f;
            if ((freezeRot & Freeze.Y) == Freeze.Y)
                freeze.Y = 1f;
            if ((freezeRot & Freeze.Z) == Freeze.Z)
                freeze.Z = 1f;
            var ur = world.CreateConstraint<HingeAngle>(rigidBody, world.NullBody);
            ur.Initialize(freeze, AngularLimit.Full);
        }
    }

    private void BeginCollide(Arbiter arbiter)
    {
        JCollider other;
        if (arbiter.Body1 == rigidBody)
            other = (JCollider)arbiter.Body2.Tag;
        else
            other = (JCollider)arbiter.Body1.Tag;
        if (arbiter.Body1.IsTriggered | arbiter.Body2.IsTriggered)
            onTriggerEnter?.Invoke(other, arbiter);
        else
            onCollisionEnter?.Invoke(other, arbiter);
    }

    private void EndCollide(Arbiter arbiter)
    {
        JCollider other;
        if (arbiter.Body1 == rigidBody)
            other = (JCollider)arbiter.Body2.Tag;
        else
            other = (JCollider)arbiter.Body1.Tag;
        if (arbiter.Body1.IsTriggered | arbiter.Body2.IsTriggered)
            onTriggerExit?.Invoke(other, arbiter);
        else
            onCollisionExit?.Invoke(other, arbiter);
    }

    private void Update()
    {
        if (rigidBody == null)
            return;
        transform.SetPositionAndRotation(rigidBody.Position.ToVector3(), rigidBody.Orientation.ToQuaternion());
    }

    public abstract List<RigidBodyShape> OnCreateShape();

    public JVector TransformDirection(JVector direction)
    {
        return Rotation * direction;
    }

    public JVector TransformPoint(JVector direction)
    {
        var forwardOffset = Rotation * direction;
        return Position + forwardOffset;
    }

    public void Translate(JVector translation)
    {
        Translate(translation, Space.Self);
    }

    public void Translate(sfloat x, sfloat y, sfloat z, Space relativeTo = Space.Self)
    {
        Translate(new JVector(x, y, z), relativeTo);
    }

    public void Translate(sfloat x, sfloat y, sfloat z)
    {
        Translate(new JVector(x, y, z), Space.Self);
    }

    public void Translate(JVector translation, Space relativeTo = Space.Self)
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

    public void LookAt(JVector targetPosition, JVector worldUp)
    {
        var direction = targetPosition - Position;
        rigidBody.Orientation = JQuaternion.LookRotation(direction, worldUp);
    }
}
#endif