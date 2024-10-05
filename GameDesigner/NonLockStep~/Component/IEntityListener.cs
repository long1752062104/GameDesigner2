#if !JITTER2_PHYSICS
using BEPUphysics.NarrowPhaseSystems.Pairs;
#else
using Jitter2.Dynamics;
#endif

namespace NonLockStep
{
    public interface IEntityListener
    {
    }

    public interface ICollisionListener : IEntityListener
    {
        void OnNCollisionEnter(NRigidbody other);
        void OnNCollisionStay(NRigidbody other);
        void OnNCollisionExit(NRigidbody other);
    }

    public interface ITriggerListener : IEntityListener
    {
        void OnNTriggerEnter(NRigidbody other);
        void OnNTriggerStay(NRigidbody other);
        void OnNTriggerExit(NRigidbody other);
    }
}