#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Unity;
using UnityEngine;

namespace Net.AI
{
    public class AgentUnity : MonoBehaviour
    {
        public AgentEntity entity = new AgentEntity();
        public AxisConstraints axisConstraints = new AxisConstraints();
        public bool isPatrol;
        private float time;
        private Transform thisTransform;

        // Start is called before the first frame update
        void Start()
        {
            thisTransform = transform;
            entity.System = NavmeshSystemUnity.Instance.System;
            entity.SetPositionAndRotation(transform.position, transform.rotation);
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                entity.SetPositionAndRotation(transform.position, transform.rotation);
                return;
            }
#endif
            entity.OnUpdate(Time.deltaTime);
            var entityPos = entity.transform.Position;
            var thisPos = thisTransform.position;
            if (axisConstraints.X) entityPos.x = thisPos.x;
            if (axisConstraints.Y) entityPos.y = thisPos.y;
            if (axisConstraints.Z) entityPos.z = thisPos.z;
            thisTransform.SetPositionAndRotation(entityPos, entity.transform.Rotation);
            if (Time.time > time & isPatrol)
            {
                time = Time.time + Random.Range(1f, 10f);
                entity.SetDestination(new Vector3(Random.Range(-50f, 50f), 1f, Random.Range(-50f, 50f)));
            }
        }

        void OnDrawGizmos()
        {
            entity.OnDrawGizmos((begin, end) => Gizmos.DrawLine(begin, end));
        }
    }
}
#endif