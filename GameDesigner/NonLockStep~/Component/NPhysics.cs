#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Common;
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2;
using Jitter2.Collision;
using Jitter2.LinearMath;
#else
using BEPUutilities;
using BEPUphysics;
#endif

namespace NonLockStep
{
    public class NPhysics : SingletonMono<NPhysics>
    {
        public SimulationMode simulationMode;
        public World world;
        public NVector3 gravity = new NVector3(0, -9.81f, 0);
        public float step = 1f / 60f;
        public bool multiThread;

        protected override void Awake()
        {
            base.Awake();
            Build();
        }

        private void Update()
        {
            if (simulationMode == SimulationMode.Update)
                Simulate();
        }

        private void FixedUpdate()
        {
            if (simulationMode == SimulationMode.FixedUpdate)
                Simulate();
        }

        public void Simulate()
        {
#if JITTER2_PHYSICS
            world.Step(step, multiThread);
#else
            world.Update(step);
#endif
        }

        public static void ReBuild()
        {
            Instance.Build();
        }

        public void Build()
        {
#if JITTER2_PHYSICS
            world = new World(64_000, 64_000, 32_000);
            world.DynamicTree.Filter = World.DefaultDynamicTreeFilter;
            world.BroadPhaseFilter = new TriggerCollisionDetection(world);
            world.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
            world.Gravity = gravity;
            world.NumberSubsteps = 1;
            world.SolverIterations = 12;
#else
            world = new World();
            world.ForceUpdater.Gravity = gravity;
#endif
        }
    }
}
#endif