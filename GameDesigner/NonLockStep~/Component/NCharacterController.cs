#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Jitter2.Dynamics;
#else
using BEPUutilities;
using BEPUphysics.Character;
using BCharacterController = BEPUphysics.Character.CharacterController;
#endif

namespace NonLockStep
{
    public class NCharacterController : NRigidbody
    {
        [SerializeField] protected float height = 1.7f;
        [SerializeField] protected float crouchingHeight = 1.7f * .7f;
        [SerializeField] protected float proneHeight = 1.7f * 0.3f;
        [SerializeField] protected float radius = 0.6f;
        [SerializeField] protected float margin = 0.1f;
        [SerializeField] protected float maximumTractionSlope = 0.8f;
        [SerializeField] protected float maximumSupportSlope = 1.3f;
        [SerializeField] protected float standingSpeed = 8f;
        [SerializeField] protected float crouchingSpeed = 3f;
        [SerializeField] protected float proneSpeed = 1.5f;
        [SerializeField] protected float tractionForce = 1000;
        [SerializeField] protected float slidingSpeed = 6;
        [SerializeField] protected float slidingForce = 50;
        [SerializeField] protected float airSpeed = 1;
        [SerializeField] protected float airForce = 250;
        [SerializeField] protected float jumpSpeed = 4.5f;
        [SerializeField] protected float slidingJumpSpeed = 3;
        [SerializeField] protected float maximumGlueForce = 5000;

#if !JITTER2_PHYSICS
        public BCharacterController characterController;

        public NVector2 MovementDirection
        {
            get => characterController.HorizontalMotionConstraint.MovementDirection;
            set => characterController.HorizontalMotionConstraint.MovementDirection = value;
        }

        public Stance DesiredStance
        {
            get => characterController.StanceManager.DesiredStance;
            set => characterController.StanceManager.DesiredStance = value;
        }

        public NVector3 ViewDirection
        {
            get => characterController.ViewDirection;
            set => characterController.ViewDirection = value;
        }

        public override NQuaternion Rotation
        {
            get => base.Rotation;
            set
            {
                base.Rotation = value;
                characterController.ViewDirection = physicsEntity.WorldTransform.Backward;
            }
        }

        public override void Initialize()
        {
            var colliders = GetComponents<Collider>();
            var scale = (NVector3)transform.localScale;
            foreach (var collider in colliders)
            {
                if (collider is CapsuleCollider capsuleCollider)
                {
                    var center = capsuleCollider.center * scale;
                    massCenterOffset = center;
                }
                else if (collider is UnityEngine.CharacterController characterController)
                {
                    var center = characterController.center * scale;
                    massCenterOffset = center;
                }
            }
            physicsObject = characterController = new BCharacterController(transform.position, height, crouchingHeight, proneHeight, radius, margin, mass, maximumTractionSlope, maximumSupportSlope, standingSpeed, crouchingSpeed, proneSpeed, tractionForce, slidingSpeed, slidingForce, airSpeed, airForce, jumpSpeed, slidingJumpSpeed, maximumGlueForce);
            physicsObject.Tag = this;
            physicsEntity = characterController.Body;
            physicsEntity.Tag = this;
            physicsEntity.CollisionInformation.Tag = this;
            physicsEntity.CollisionInformation.CollisionRules.Group = NPhysics.Singleton.CollisionLayers[gameObject.layer];
            physicsEntity.CollisionInformation.CollisionRules.IncludeLayers = includeLayers;
            physicsEntity.CollisionInformation.CollisionRules.ExcludeLayers = excludeLayers;
            NPhysics.Singleton.AddRigidbody(this);
            RegisterEvents();
            InitializeTransform();
        }

        public void Jump()
        {
            characterController.Jump();
        }

        public override void Translate(NVector3 translation, Space relativeTo = Space.Self)
        {
            MovementDirection = new NVector2(translation.X, translation.Z);
        }
#endif
    }
}
#endif