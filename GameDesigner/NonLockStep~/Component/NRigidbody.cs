#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;
using SoftFloat;
using System.Collections.Generic;
using Net.Unity;
#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Jitter2.Dynamics;
#else
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.Materials;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.PositionUpdating;
using BEPUphysics.CollisionRuleManagement;
#endif

namespace NonLockStep
{
    public class NRigidbody : MonoBehaviour
    {
        [SerializeField] protected float mass = 1.0f;
        [SerializeField] protected float angularDamping = .15f;
        [SerializeField] protected float linearDamping = .03f;
        [SerializeField] protected bool isKinematic;
#if !JITTER2_PHYSICS
        [SerializeField] protected PositionUpdateMode collisionDetection = PositionUpdateMode.Discrete;
        [Header("Material")]
        [SerializeField] protected float kineticFriction = (float)MaterialManager.DefaultKineticFriction;
        [SerializeField] protected float staticFriction = (float)MaterialManager.DefaultStaticFriction;
        [SerializeField] protected float bounciness = (float)MaterialManager.DefaultBounciness;
#else
        [Header("Material")]
        [SerializeField] protected float friction = 0.2f;
#endif
        [Header("Constraints")]
        [SerializeField] protected AxisConstraints fixedPosition;
        [SerializeField] protected AxisConstraints fixedRotation;
        [Header("layerOverrides")]
        [SerializeField] protected LayerMask includeLayers;
        [SerializeField] protected LayerMask excludeLayers;
        [Header("initialize")]
        [SerializeField] protected InitializeMode initializeMode = InitializeMode.Start;
        [SerializeField] protected DeinitializeMode deinitializeMode = DeinitializeMode.OnDestroy;
        [SerializeField] protected bool syncTransform = true;
#if !JITTER2_PHYSICS
        public IWorldObject physicsObject;
        protected Entity physicsEntity;
#else
        protected RigidBody physicsEntity;
        protected readonly List<RigidBodyShape> shapes = new List<RigidBodyShape>();
#endif
        protected Vector3 previousRenderPosition;
        protected Quaternion previousRenderRotation;
        protected NVector3 previousPhysicsPosition;
        protected NVector3 currentPhysicsPosition;
        protected NQuaternion previousPhysicsRotation = NQuaternion.Identity;
        protected NQuaternion currentPhysicsRotation = NQuaternion.Identity;
        protected NVector3 massCenterOffset;
        protected readonly List<ICollisionEnterListener> collisionEnterListeners = new();
        protected readonly List<ICollisionStayListener> collisionStayListeners = new();
        protected readonly List<ICollisionExitListener> collisionExitListeners = new();
        protected readonly List<ITriggerEnterListener> triggerEnterListeners = new();
        protected readonly List<ITriggerStayListener> triggerStayListeners = new();
        protected readonly List<ITriggerExitListener> triggerExitListeners = new();
        public object Tag; //存特别引用

        public sfloat Mass
        {
            get { return mass; }
            set
            {
                mass = value;
#if !JITTER2_PHYSICS
                if (physicsEntity == null)
                    return;
                physicsEntity.Mass = value;
#endif
            }
        }
        public bool IsKinematic
        {
            get { return isKinematic; }
            set
            {
                isKinematic = value;
                if (physicsEntity == null)
                    return;
#if !JITTER2_PHYSICS
                if (isKinematic)
                    physicsEntity.BecomeKinematic();
                else
                    physicsEntity.BecomeDynamic(mass);
#else
                physicsEntity.IsStatic = isKinematic;
#endif
            }
        }
#if !JITTER2_PHYSICS
        public Entity Entity => physicsEntity;
#else
        public RigidBody Entity => physicsEntity;
#endif
        public NVector3 Position
        {
            get => physicsEntity.Position;
            set
            {
                currentPhysicsPosition = value;
                physicsEntity.Position = value;
            }
        }
        public virtual NQuaternion Rotation
        {
            get => physicsEntity.Orientation;
            set
            {
                currentPhysicsRotation = value;
                physicsEntity.Orientation = value;
            }
        }
#if !JITTER2_PHYSICS
        public NVector3 Velocity { get => physicsEntity.LinearVelocity; set => physicsEntity.LinearVelocity = value; }
#else
        public NVector3 Velocity { get => physicsEntity.Velocity; set => physicsEntity.Velocity = value; }
#endif

        private void Awake()
        {
            if (initializeMode == InitializeMode.Awake)
                Initialize();
        }

        public void Start()
        {
            if (initializeMode == InitializeMode.Start)
                Initialize();
        }

        private void OnEnable()
        {
            if (initializeMode == InitializeMode.OnEnable)
                Initialize();
        }

        private void OnDisable()
        {
            if (deinitializeMode == DeinitializeMode.OnDisable)
                Deinitialize();
        }

        private void OnDestroy()
        {
            if (deinitializeMode == DeinitializeMode.OnDestroy)
                Deinitialize();
        }

        public virtual void Initialize()
        {
            var colliders = GetComponents<Collider>();
#if !JITTER2_PHYSICS
            var shapeEntries = new List<CompoundShapeEntry>();
            var scale = (NVector3)transform.localScale;
            var isTrigger = false;
            foreach (var collider in colliders)
            {
                EntityCollidable collidable = null;
                if (collider is BoxCollider boxCollider)
                {
                    var boxSize = boxCollider.size * scale;
                    collidable = new ConvexCollidable<BoxShape>(new BoxShape(boxSize.X, boxSize.Y, boxSize.Z))
                    {
                        WorldTransform = new RigidTransform(boxCollider.center * scale, NQuaternion.Identity)
                    };
                }
                else if (collider is SphereCollider sphereCollider)
                {
                    var sphereRadius = (scale.X + scale.Y + scale.Z) / (sfloat)3.0f * (sfloat)sphereCollider.radius;
                    collidable = new ConvexCollidable<SphereShape>(new SphereShape(sphereRadius))
                    {
                        WorldTransform = new RigidTransform(sphereCollider.center * scale, NQuaternion.Identity)
                    };
                }
                else if (collider is CapsuleCollider capsuleCollider)
                {
                    var size = (scale.X + scale.Y + scale.Z) / (sfloat)3.0f;
                    var capsuleRadius = (sfloat)capsuleCollider.radius;
                    var offsetAxis = capsuleCollider.direction switch
                    {
                        0 => NVector3.Right * scale.X,
                        1 => NVector3.Up * scale.Y,
                        2 => NVector3.Forward * scale.Z,
                        _ => NVector3.Zero
                    };
                    var halfHeight = (sfloat)capsuleCollider.height * (sfloat)0.5f;
                    var halfHeightMinusRadius = libm.Max((sfloat)0, halfHeight - capsuleRadius);
                    var center = capsuleCollider.center * scale;
                    var start = -offsetAxis * halfHeightMinusRadius;
                    var end = offsetAxis * halfHeightMinusRadius;
                    Capsule.GetCapsuleInformation(ref start, ref end, out var orientation, out var length);
                    collidable = new ConvexCollidable<CapsuleShape>(new CapsuleShape(length, capsuleRadius * size))
                    {
                        WorldTransform = new RigidTransform(center, orientation)
                    };
                }
                else if (collider is MeshCollider meshCollider)
                {
                    var mesh = meshCollider.sharedMesh;
                    var vertices = mesh.vertices;
                    var staticTriangleIndices = mesh.GetIndices(0);
                    var staticTriangleVertices = new NVector3[vertices.Length];
                    for (int i = 0; i < vertices.Length; i++)
                        staticTriangleVertices[i] = vertices[i] * scale;
                    if (isKinematic)
                    {
                        physicsObject = new StaticMesh(staticTriangleVertices, staticTriangleIndices, new AffineTransform(transform.localScale, transform.rotation, transform.position))
                        {
                            Tag = this
                        };
                        NPhysics.Singleton.AddRigidbody(this);
                        return;
                    }
                    collidable = new ConvexCollidable<ConvexHullShape>(new ConvexHullShape(staticTriangleVertices, out var center))
                    {
                        WorldTransform = new RigidTransform(center, NQuaternion.Identity)
                    };
                }
                if (collidable == null)
                    continue;
                isTrigger = collider.isTrigger;
                shapeEntries.Add(new CompoundShapeEntry(collidable.Shape, collidable.WorldTransform));
            }
            physicsObject = physicsEntity = new Entity(new CompoundShape(shapeEntries, out massCenterOffset));
            physicsEntity.Tag = this;
            physicsEntity.CollisionInformation.Tag = this;
            physicsEntity.PositionUpdateMode = collisionDetection;
            physicsEntity.CollisionInformation.IsTrigger = isTrigger;
            physicsEntity.CollisionInformation.CollisionRules.Personal = isTrigger ? CollisionRule.NoSolver : CollisionRule.Defer;
            physicsEntity.CollisionInformation.CollisionRules.Group = NPhysics.Singleton.CollisionLayers[gameObject.layer];
            physicsEntity.CollisionInformation.CollisionRules.IncludeLayers = includeLayers;
            physicsEntity.CollisionInformation.CollisionRules.ExcludeLayers = excludeLayers;
            physicsEntity.Material = new BEPUphysics.Materials.Material
            {
                StaticFriction = staticFriction,
                KineticFriction = kineticFriction,
                Bounciness = bounciness
            };
            physicsEntity.AngularDamping = angularDamping;
            physicsEntity.LinearDamping = linearDamping;
            NPhysics.Singleton.AddRigidbody(this);
            Mass = mass;
            IsKinematic = isKinematic;
            RegisterEvents();
            InitializeTransform();
#else
            var scale = (NVector3)transform.localScale;
            var isTrigger = false;
            shapes.Clear();
            foreach (var collider in colliders)
            {
                RigidBodyShape collidable = null;
                if (collider is BoxCollider boxCollider)
                {
                    var boxSize = (NVector3)boxCollider.size;
                    collidable = new BoxShape(boxSize.X * scale.X, boxSize.Y * scale.Y, boxSize.Z * scale.Z)
                    {
                        CenterOffset = boxCollider.center,
                    };
                }
                else if (collider is SphereCollider sphereCollider)
                {
                    var sphereRadius = (-scale.X + scale.Y + scale.Z) / (sfloat)3.0f * (sfloat)sphereCollider.radius;
                    collidable = new SphereShape(sphereRadius)
                    {
                        CenterOffset = sphereCollider.center
                    };
                }
                else if (collider is CapsuleCollider capsuleCollider)
                {
                    var size = (-scale.X + scale.Y + scale.Z) / (sfloat)3.0f;
                    var capsuleRadius = (sfloat)capsuleCollider.radius;
                    var offsetAxis = capsuleCollider.direction switch
                    {
                        0 => NVector3.Right * scale.X,
                        1 => NVector3.Up * scale.Y,
                        2 => NVector3.Forward * scale.Z,
                        _ => NVector3.Zero
                    };
                    var halfHeight = (sfloat)capsuleCollider.height * (sfloat)0.5f;
                    var halfHeightMinusRadius = libm.Max((sfloat)0, halfHeight - capsuleRadius);
                    var center = capsuleCollider.center * scale;
                    var start = -offsetAxis * halfHeightMinusRadius;
                    var end = offsetAxis * halfHeightMinusRadius;
                    GetCapsuleInformation(ref start, ref end, out var orientation, out var length);
                    collidable = new CapsuleShape(capsuleRadius * size, length)
                    {
                        CenterOffset = capsuleCollider.center
                    };
                }
                else if (collider is MeshCollider meshCollider)
                {
                    var mesh = meshCollider.sharedMesh;
                    var localScale = transform.localScale;
                    var indices = mesh.GetIndices(0);
                    var vertices = mesh.vertices;
                    List<JTriangle> triangles = new();
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        NVector3 v1 = Conversion.ToNVector3(vertices[indices[i + 0]]);
                        NVector3 v2 = Conversion.ToNVector3(vertices[indices[i + 1]]);
                        NVector3 v3 = Conversion.ToNVector3(vertices[indices[i + 2]]);
                        v1 = new NVector3(v1.X * localScale.x, v1.Y * localScale.y, v1.Z * localScale.z);
                        v2 = new NVector3(v2.X * localScale.x, v2.Y * localScale.y, v2.Z * localScale.z);
                        v3 = new NVector3(v3.X * localScale.x, v3.Y * localScale.y, v3.Z * localScale.z);
                        triangles.Add(new JTriangle(v1, v2, v3));
                    }
                    var jtm = new TriangleMesh(triangles);
                    for (int i = 0; i < jtm.Indices.Length; i++)
                    {
                        var ts = new FatTriangleShape(jtm, i, 0.2f);
                        shapes.Add(ts);
                    }
                }
                if (collidable == null)
                    continue;
                isTrigger = collider.isTrigger;
                shapes.Add(collidable);
            }
            var world = NPhysics.Singleton.World;
            physicsEntity = world.CreateRigidBody();
            physicsEntity.AddShape(shapes, false);
            physicsEntity.IsStatic = isKinematic;
            physicsEntity.IsTriggered = isTrigger;
            physicsEntity.Friction = friction;
            physicsEntity.Damping = (linearDamping, angularDamping);
            physicsEntity.Tag = this;
            physicsEntity.Layer = NPhysics.Singleton.CollisionLayers[gameObject.layer];
            physicsEntity.Data.Layer.IncludeLayers = includeLayers;
            physicsEntity.Data.Layer.ExcludeLayers = excludeLayers;
            NPhysics.Singleton.AddRigidbody(this);
            IsKinematic = isKinematic;
            RegisterEvents();
            InitializeTransform();
#endif
        }

        public static void GetCapsuleInformation(ref NVector3 start, ref NVector3 end, out NQuaternion orientation, out sfloat length)
        {
#if !JITTER2_PHYSICS
            NVector3.Subtract(ref end, ref start, out NVector3 segmentDirection);
#else
            NVector3.Subtract(end, start, out NVector3 segmentDirection);
#endif
            length = segmentDirection.Length();
            if (length > 0)
            {
                NVector3.Divide(ref segmentDirection, length, out segmentDirection);
                NVector3 v1 = NVector3.Up;
                NQuaternion.GetNQuaternionBetweenNormalizedVectors(ref v1, ref segmentDirection, out orientation);
            }
            else
                orientation = NQuaternion.Identity;
        }

        public void Deinitialize()
        {
            if (physicsEntity == null)
                return;
            DeregisterEvents();
            var physics = NPhysics.Singleton;
            if (physics != null)
                physics.RemoveRigidbody(this);
        }

        private void Update()
        {
            if (physicsEntity == null)
                return;
            if ((previousRenderPosition != transform.position || previousRenderRotation != transform.rotation) && syncTransform)
            {
                InitializeTransform();
            }
            else
            {
#if !JITTER2_PHYSICS
                var offset = NQuaternion.Transform(massCenterOffset, physicsEntity.orientation);
                transform.SetPositionAndRotation(Vector3.Lerp(previousPhysicsPosition - offset, currentPhysicsPosition - offset, NPhysics.Singleton.InterpolationTime), Quaternion.Lerp(previousPhysicsRotation, currentPhysicsRotation, NPhysics.Singleton.InterpolationTime));
#else
                transform.SetPositionAndRotation(Vector3.Lerp(previousPhysicsPosition, currentPhysicsPosition, NPhysics.Singleton.InterpolationTime), Quaternion.Lerp(previousPhysicsRotation, currentPhysicsRotation, NPhysics.Singleton.InterpolationTime));
#endif
            }
            previousRenderPosition = transform.position;
            previousRenderRotation = transform.rotation;
        }

        protected void InitializeTransform()
        {
#if !JITTER2_PHYSICS
            physicsEntity.orientation = transform.rotation;
            currentPhysicsRotation = physicsEntity.orientation;
            previousPhysicsRotation = physicsEntity.orientation;
            var offset = NQuaternion.Transform(massCenterOffset, physicsEntity.orientation);
            physicsEntity.Position = (NVector3)transform.position + offset;
            currentPhysicsPosition = physicsEntity.Position;
            previousPhysicsPosition = physicsEntity.Position;
#else
            physicsEntity.Orientation = transform.rotation;
            currentPhysicsRotation = physicsEntity.Orientation;
            previousPhysicsRotation = physicsEntity.Orientation;
            physicsEntity.Position = (NVector3)transform.position;
            currentPhysicsPosition = physicsEntity.Position;
            previousPhysicsPosition = physicsEntity.Position;
#endif
        }

        protected void RegisterEvents()
        {
#if !JITTER2_PHYSICS
            physicsEntity.CollisionInformation.Events.InitialCollisionDetected += OnNCollisionEnter;
            physicsEntity.CollisionInformation.Events.PairTouched += OnNCollisionStay;
            physicsEntity.CollisionInformation.Events.CollisionEnded += OnNCollisionExit;
#else
            physicsEntity.BeginCollide += OnNCollisionEnter;
            physicsEntity.EndCollide += OnNCollisionExit;
#endif
            var listeners = GetComponents<IEntityListener>();
            collisionEnterListeners.Clear();
            collisionStayListeners.Clear();
            collisionExitListeners.Clear();
            triggerEnterListeners.Clear();
            triggerStayListeners.Clear();
            triggerExitListeners.Clear();
            for (int i = 0; i < listeners.Length; i++)
            {
                var listener = listeners[i];
                if (listener is ICollisionEnterListener collisionEnter)
                    collisionEnterListeners.Add(collisionEnter);
                if (listener is ICollisionStayListener collisionStay)
                    collisionStayListeners.Add(collisionStay);
                if (listener is ICollisionExitListener collisionExit)
                    collisionExitListeners.Add(collisionExit);
                if (listener is ITriggerEnterListener triggerEnter)
                    triggerEnterListeners.Add(triggerEnter);
                if (listener is ITriggerStayListener triggerStay)
                    triggerStayListeners.Add(triggerStay);
                if (listener is ITriggerExitListener triggerExit)
                    triggerExitListeners.Add(triggerExit);
            }
        }

        private void DeregisterEvents()
        {
#if !JITTER2_PHYSICS
            physicsEntity.CollisionInformation.Events.InitialCollisionDetected -= OnNCollisionEnter;
            physicsEntity.CollisionInformation.Events.PairTouched -= OnNCollisionStay;
            physicsEntity.CollisionInformation.Events.CollisionEnded -= OnNCollisionExit;
#else
            physicsEntity.BeginCollide -= OnNCollisionEnter;
            physicsEntity.EndCollide -= OnNCollisionExit;
#endif
        }

        public void AddListener(IEntityListener listener)
        {
            if (listener is ICollisionEnterListener collisionEnter)
                collisionEnterListeners.Add(collisionEnter);
            if (listener is ICollisionStayListener collisionStay)
                collisionStayListeners.Add(collisionStay);
            if (listener is ICollisionExitListener collisionExit)
                collisionExitListeners.Add(collisionExit);
            if (listener is ITriggerEnterListener triggerEnter)
                triggerEnterListeners.Add(triggerEnter);
            if (listener is ITriggerStayListener triggerStay)
                triggerStayListeners.Add(triggerStay);
            if (listener is ITriggerExitListener triggerExit)
                triggerExitListeners.Add(triggerExit);
        }

        public void RemoveListener(IEntityListener listener)
        {
            if (listener is ICollisionEnterListener collisionEnter)
                collisionEnterListeners.Remove(collisionEnter);
            if (listener is ICollisionStayListener collisionStay)
                collisionStayListeners.Remove(collisionStay);
            if (listener is ICollisionExitListener collisionExit)
                collisionExitListeners.Remove(collisionExit);
            if (listener is ITriggerEnterListener triggerEnter)
                triggerEnterListeners.Remove(triggerEnter);
            if (listener is ITriggerStayListener triggerStay)
                triggerStayListeners.Remove(triggerStay);
            if (listener is ITriggerExitListener triggerExit)
                triggerExitListeners.Remove(triggerExit);
        }

#if !JITTER2_PHYSICS
        private void OnNCollisionEnter(EntityCollidable self, Collidable other, CollidablePairHandler pair)
        {
            var rigidbody = other.Tag as NRigidbody;
            var isTrigger = self.IsTrigger | other.IsTrigger;
            if (isTrigger)
            {
                foreach (var listener in triggerEnterListeners)
                    listener.OnNTriggerEnter(rigidbody, pair);
            }
            else
            {
                foreach (var listener in collisionEnterListeners)
                    listener.OnNCollisionEnter(rigidbody, pair);
            }
        }

        private void OnNCollisionStay(EntityCollidable self, Collidable other, CollidablePairHandler pair)
        {
            var rigidbody = other.Tag as NRigidbody;
            var isTrigger = self.IsTrigger | other.IsTrigger;
            if (isTrigger)
            {
                foreach (var listener in triggerStayListeners)
                    listener.OnNTriggerStay(rigidbody, pair);
            }
            else
            {
                foreach (var listener in collisionStayListeners)
                    listener.OnNCollisionStay(rigidbody, pair);
            }
        }

        private void OnNCollisionExit(EntityCollidable self, Collidable other, CollidablePairHandler pair)
        {
            var rigidbody = other.Tag as NRigidbody;
            var isTrigger = self.IsTrigger | other.IsTrigger;
            if (isTrigger)
            {
                foreach (var listener in triggerExitListeners)
                    listener.OnNTriggerExit(rigidbody, pair);
            }
            else
            {
                foreach (var listener in collisionExitListeners)
                    listener.OnNCollisionExit(rigidbody, pair);
            }
        }
#else
        private void OnNCollisionEnter(Arbiter arbiter)
        {
            if (arbiter.Handle.IsZero)
            {
                Debug.Log("bug!!!");
            }
            NRigidbody rigidbody;
            if (arbiter.Body1 == physicsEntity)
                rigidbody = arbiter.Body2.Tag as NRigidbody;
            else
                rigidbody = arbiter.Body1.Tag as NRigidbody;
            var isTrigger = arbiter.Body1.IsTriggered | arbiter.Body2.IsTriggered;
            if (isTrigger)
            {
                foreach (var listener in triggerEnterListeners)
                    listener.OnNTriggerEnter(rigidbody, arbiter); //传arbiter丢失Handle指针
            }
            else
            {
                foreach (var listener in collisionEnterListeners)
                    listener.OnNCollisionEnter(rigidbody, arbiter);
            }
        }

        private void OnNCollisionExit(Arbiter arbiter)
        {
            NRigidbody rigidbody;
            if (arbiter.Body1 == physicsEntity)
                rigidbody = arbiter.Body2.Tag as NRigidbody;
            else
                rigidbody = arbiter.Body1.Tag as NRigidbody;
            var isTrigger = arbiter.Body1.IsTriggered | arbiter.Body2.IsTriggered;
            if (isTrigger)
            {
                foreach (var listener in triggerExitListeners)
                    listener.OnNTriggerExit(rigidbody, arbiter);
            }
            else
            {
                foreach (var listener in collisionExitListeners)
                    listener.OnNCollisionExit(rigidbody, arbiter);
            }
        }
#endif

        public virtual void PostPhysicsUpdate()
        {
            if (physicsEntity == null)
                return;
            if (fixedPosition.X)
            {
#if !JITTER2_PHYSICS
                physicsEntity.position.X = currentPhysicsPosition.X;
                physicsEntity.linearVelocity.X = 0;
#else
                physicsEntity.Handle.Data.Position.X = currentPhysicsPosition.X;
                physicsEntity.Handle.Data.Velocity.X = 0;
#endif
            }
            if (fixedPosition.Y)
            {
#if !JITTER2_PHYSICS
                physicsEntity.position.Y = currentPhysicsPosition.Y;
                physicsEntity.linearVelocity.Y = 0;
#else
                physicsEntity.Handle.Data.Position.Y = currentPhysicsPosition.Y;
                physicsEntity.Handle.Data.Velocity.Y = 0;
#endif
            }
            if (fixedPosition.Z)
            {
#if !JITTER2_PHYSICS
                physicsEntity.position.Z = currentPhysicsPosition.Z;
                physicsEntity.linearVelocity.Z = 0;
#else
                physicsEntity.Handle.Data.Position.Z = currentPhysicsPosition.Z;
                physicsEntity.Handle.Data.Velocity.Z = 0;
#endif
            }

            if (fixedRotation.Any)
            {
                var rotationDelta = NQuaternion.Inverse(currentPhysicsRotation) * physicsEntity.Orientation;
                NQuaternion.GetAxisAngleFromQuaternion(ref rotationDelta, out var axis, out var angle);

                if (fixedRotation.X)
                {
                    axis.X = 0;
#if !JITTER2_PHYSICS
                    physicsEntity.angularVelocity.X = 0;
#else
                    physicsEntity.Handle.Data.AngularVelocity.X = 0;
#endif
                }
                if (fixedRotation.Y)
                {
                    axis.Y = 0;
#if !JITTER2_PHYSICS
                    physicsEntity.angularVelocity.Y = 0;
#else
                    physicsEntity.Handle.Data.AngularVelocity.Y = 0;
#endif
                }
                if (fixedRotation.Z)
                {
                    axis.Z = 0;
#if !JITTER2_PHYSICS
                    physicsEntity.angularVelocity.Z = 0;
#else
                    physicsEntity.Handle.Data.AngularVelocity.Z = 0;
#endif
                }
                rotationDelta = NQuaternion.Identity;
                var axisLength = axis.Length();
                if (axisLength > 0)
                {
                    angle *= axisLength;
                    axis.Normalize();
                    rotationDelta = NQuaternion.CreateFromAxisAngle(axis, angle);
                }
                physicsEntity.Orientation = rotationDelta * currentPhysicsRotation;
            }

            previousPhysicsPosition = currentPhysicsPosition;
            previousPhysicsRotation = currentPhysicsRotation;
            currentPhysicsPosition = physicsEntity.Position;
            currentPhysicsRotation = physicsEntity.Orientation;
        }

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

        public virtual void Translate(NVector3 translation, Space relativeTo = Space.Self)
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
            physicsEntity.Orientation = NQuaternion.LookRotation(direction, worldUp);
        }
    }
}
#endif