using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
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

public abstract class JCollider : MonoBehaviour
{
    public bool isStatic;
    public float friction = 0.2f;
    public float linear, angular;
    public Freeze freezeRot;
    public Vector3 center;
    public RigidBody rigidBody;
    internal List<RigidBodyShape> shapes;
    private bool isInitialize;

    public virtual void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialize)
            return;
        isInitialize = true;
        var world = JPhysics.I.world;
        rigidBody = world.CreateRigidBody();
        rigidBody.AddShape(shapes = OnCreateShape(), false);
        rigidBody.Position = transform.position.ToJVector();
        rigidBody.Orientation = transform.rotation.ToQuaternion();
        rigidBody.IsStatic = isStatic;
        rigidBody.Friction = friction;
        rigidBody.Damping = (linear, angular);
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

    private void Update()
    {
        transform.SetPositionAndRotation(rigidBody.Position.ToVector3(), rigidBody.Orientation.ToQuaternion());
    }

    public abstract List<RigidBodyShape> OnCreateShape();
}
