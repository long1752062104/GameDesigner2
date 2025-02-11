#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using Net.Common;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;
using SoftFloat;
#if JITTER2_PHYSICS
using Jitter2;
using Jitter2.Dynamics;
using Jitter2.Collision;
#else
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Ray = BEPUutilities.Ray;
#endif

namespace NonLockStep
{
    public class NPhysics : SingletonMono<NPhysics>
    {
        [SerializeField] private float timeStep = 1f / 60f;
        [SerializeField] private float syncTransformLerp = 1f;
        [SerializeField] private int solverIterationLimit = 6;
        [SerializeField] private Vector3 gravity = Vector3.down * 9.81f;
        [SerializeField] private InitializeMode initializeMode = InitializeMode.Awake;
        [SerializeField] private SimulationMode simulationMode;

        private World world;
        private float timeSinceLastStep;
        private readonly List<NRigidbody> rigidbodies = new();
        private readonly Queue<NRigidbody> removeRigidbodies = new();
#if !JITTER2_PHYSICS
        private readonly List<CollisionGroup> collisionLayers = new();
#else
        private readonly List<CollisionLayer> collisionLayers = new();
#endif

        public float InterpolationTime => syncTransformLerp * Time.deltaTime;
        public World World => world;
#if !JITTER2_PHYSICS
        public List<CollisionGroup> CollisionLayers => collisionLayers;
#else
        public List<CollisionLayer> CollisionLayers => collisionLayers;
#endif

        protected override void Awake()
        {
            base.Awake();
            if (initializeMode == InitializeMode.Awake)
                Initialize();
        }

        private void OnEnable()
        {
            if (initializeMode == InitializeMode.OnEnable)
                Initialize();
        }

        private void Start()
        {
            if (initializeMode == InitializeMode.Start)
                Initialize();
        }

        private void Update()
        {
            if (simulationMode != SimulationMode.Update)
                return;
            if (Time.timeScale == 0f)
                return;
#if !JITTER2_PHYSICS
            timeSinceLastStep += Time.deltaTime;
            if (timeSinceLastStep > timeStep)
            {
                timeSinceLastStep = 0;
                Profiler.BeginSample("BEPUphysics Simulation Step");
                Simulate();
                Profiler.EndSample();
            }
#else
            timeSinceLastStep += Time.deltaTime;
            while (timeSinceLastStep > timeStep)
            {
                timeSinceLastStep = 0;
                Profiler.BeginSample("Jitter2 Simulation Step");
                Simulate();
                Profiler.EndSample();
            }
#endif
        }

        private void FixedUpdate()
        {
            if (simulationMode == SimulationMode.FixedUpdate)
                Simulate();
        }

        public void Simulate()
        {
#if JITTER2_PHYSICS
            world.Step(timeStep, false);
#else
            world.Update();
#endif
            NRigidbody rigidbody;
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                rigidbody = rigidbodies[i];
                rigidbody.PostPhysicsUpdate();
            }
            while (removeRigidbodies.Count > 0)
            {
                rigidbody = removeRigidbodies.Dequeue();
#if !JITTER2_PHYSICS
                world.Remove(rigidbody.physicsObject);
#else
                world.Remove(rigidbody.Entity);
#endif
            }
        }

        public void Initialize()
        {
            rigidbodies.Clear();
            removeRigidbodies.Clear();
#if JITTER2_PHYSICS
            world = new World(64_000, 64_000, 32_000);
            world.DynamicTree.Filter = World.DefaultDynamicTreeFilter;
            world.BroadPhaseFilter = new TriggerCollisionDetection(world);
            world.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
            world.Gravity = gravity;
            world.NumberSubsteps = 1;
            world.SolverIterations = 12;
            CollisionLayerRules.CollisionLayers.Clear();
            collisionLayers.Clear();
            var layerCount = 32;
            for (int i = 0; i < layerCount; i++)
                collisionLayers.Add(new CollisionLayer() { Layer = i });
            for (int x = 0; x < layerCount; x++)
            {
                var layerName = LayerMask.LayerToName(x);
                if (string.IsNullOrEmpty(layerName))
                    continue;
                for (int y = x; y < layerCount; y++)
                {
                    var otherLayerName = LayerMask.LayerToName(y);
                    if (string.IsNullOrEmpty(otherLayerName))
                        continue;
                    var isIgnoring = Physics.GetIgnoreLayerCollision(x, y);
                    CollisionLayerRules.CollisionLayers.Add(x + y * 100, isIgnoring);
                }
            }
#else
            world = new World();
            world.Solver.IterationLimit = solverIterationLimit;
            world.TimeStepSettings = new TimeStepSettings()
            {
                MaximumTimeStepsPerFrame = 1,
                TimeStepDuration = timeStep
            };
            world.ForceUpdater.Gravity = gravity;
            CollisionRules.CollisionGroupRules.Clear();
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(CollisionRules.DefaultKinematicCollisionGroup, CollisionRules.DefaultKinematicCollisionGroup), CollisionRule.NoBroadPhase);
            collisionLayers.Clear();
            var layerCount = 32;
            for (int i = 0; i < layerCount; i++)
                collisionLayers.Add(new CollisionGroup(i));
            for (int x = 0; x < layerCount; x++)
            {
                var layerName = LayerMask.LayerToName(x);
                if (string.IsNullOrEmpty(layerName))
                    continue;
                var firstStackGroup = collisionLayers[x];
                for (int y = x; y < layerCount; y++)
                {
                    var otherLayerName = LayerMask.LayerToName(y);
                    if (string.IsNullOrEmpty(otherLayerName))
                        continue;
                    var isIgnoring = Physics.GetIgnoreLayerCollision(x, y);
                    var secondStackGroup = collisionLayers[y];
                    var groupPair = new CollisionGroupPair(firstStackGroup, secondStackGroup);
                    CollisionRules.CollisionGroupRules.Add(groupPair, isIgnoring ? CollisionRule.NoBroadPhase : CollisionRule.Defer);
                }
            }
#endif
        }

        public void AddRigidbody(NRigidbody rigidbody)
        {
            rigidbodies.Add(rigidbody);
#if !JITTER2_PHYSICS
            world.Add(rigidbody.physicsObject);
#endif
        }

        public void RemoveRigidbody(NRigidbody rigidbody)
        {
            rigidbodies.Remove(rigidbody);
            removeRigidbodies.Enqueue(rigidbody); //要延迟移除，直接在当碰撞时移除刚体会导致内部报错，因为内部还有其他关联的数据要处理
        }

#if !JITTER2_PHYSICS
        public static bool RayCast(Ray ray, out RayCastResult result)
        {
            return Singleton.world.RayCast(ray, out result);
        }

        public static bool RayCast(Ray ray, Func<BroadPhaseEntry, bool> filter, out RayCastResult result)
        {
            return Singleton.world.RayCast(ray, filter, out result);
        }

        public static bool RayCast(Ray ray, sfloat maximumLength, out RayCastResult result)
        {
            return Singleton.world.RayCast(ray, maximumLength, out result);
        }

        public static bool RayCast(Ray ray, sfloat maximumLength, Func<BroadPhaseEntry, bool> filter, out RayCastResult result)
        {
            return Singleton.world.RayCast(ray, maximumLength, filter, out result);
        }

        public static bool RayCast(Ray ray, sfloat maximumLength, IList<RayCastResult> outputRayCastResults)
        {
            return Singleton.world.RayCast(ray, maximumLength, outputRayCastResults);
        }

        public static bool RayCast(Ray ray, sfloat maximumLength, Func<BroadPhaseEntry, bool> filter, IList<RayCastResult> outputRayCastResults)
        {
            return Singleton.world.RayCast(ray, maximumLength, filter, outputRayCastResults);
        }

        public static bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref NVector3 sweep, out RayCastResult castResult)
        {
            return Singleton.world.ConvexCast(castShape, ref startingTransform, ref sweep, out castResult);
        }

        public static bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref NVector3 sweep, Func<BroadPhaseEntry, bool> filter, out RayCastResult castResult)
        {
            return Singleton.world.ConvexCast(castShape, ref startingTransform, ref sweep, filter, out castResult);
        }

        public static bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref NVector3 sweep, IList<RayCastResult> outputCastResults)
        {
            return Singleton.world.ConvexCast(castShape, ref startingTransform, ref sweep, outputCastResults);
        }

        public static bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref NVector3 sweep, Func<BroadPhaseEntry, bool> filter, IList<RayCastResult> outputCastResults)
        {
            return Singleton.world.ConvexCast(castShape, ref startingTransform, ref sweep, filter, outputCastResults);
        }
#endif
    }
}
#endif