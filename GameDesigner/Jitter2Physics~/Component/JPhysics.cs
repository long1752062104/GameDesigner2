#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2;
using Jitter2.Collision;
using Jitter2.LinearMath;
using Net.Common;
using UnityEngine;

public class JPhysics : SingletonMono<JPhysics>
{
    public SimulationMode simulationMode;
    internal World world;
    public JVector gravity = new JVector(0, -9.81f, 0);
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
        world.Step(step, multiThread);
    }

    public static void ReBuild()
    {
        Instance.Build();
    }

    public void Build()
    {
        world = new World(64_000, 64_000, 32_000);
        world.DynamicTree.Filter = World.DefaultDynamicTreeFilter;
        world.BroadPhaseFilter = new TriggerCollisionDetection(world);
        world.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
        world.Gravity = gravity;
        world.NumberSubsteps = 1;
        world.SolverIterations = 12;
    }
}
#endif