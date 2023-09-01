using System;
using System.Collections.Generic;
using Net.Common;

namespace Net.AI
{
    [Serializable]
    public class AgentEntity
    {
        public float speed = 5f;
        public float agentHeight = 1f;
        public TransformEntity transform = new TransformEntity();
        private readonly List<Vector3> pathPoints = new List<Vector3>();
        //private bool isLookAtOptimize = true;
        public FindPathMode findPathMode;

        public void Init(Vector3 position, Quaternion rotation)
        {
            transform.Position = position;
            transform.Rotation = rotation;
        }

        public void OnUpdate(float dt)
        {
            if (pathPoints.Count > 0)
            {
                int index = pathPoints.Count - 1;
                var nextPos = pathPoints[index];
                //nextPos.y -= agentHeight / 2f;
                //if (isLookAtOptimize)
                //{
                //    transform.LookAt(new Vector3(nextPos.x, transform.Position.y, nextPos.z));
                //    isLookAtOptimize = false;
                //}
                transform.Rotation = Quaternion.Lerp(transform.Rotation, Quaternion.LookRotation(new Vector3(nextPos.x, transform.Position.y, nextPos.z) - transform.Position, Vector3.up), 0.1f);
                transform.Position = Vector3.MoveTowards(transform.Position, nextPos, speed * dt);
                if (Vector3.Distance(transform.Position, nextPos) < 0.1f)
                {
                    pathPoints.RemoveAt(index);
                    //isLookAtOptimize = true;
                }
            }
        }

        public bool SetDestination(Vector3 target)
        {
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
            Pathfinding.I.GetPath(transform.Position, target, pathPoints, agentHeight, findPathMode);
            pathPoints.Reverse(); //反转是因为后面每一步会进行移除, 移除时数组不会进行倒塌操作
            return pathPoints.Count > 0;
#else
            return false;
#endif
        }

        public void OnDrawGizmos(Action<Vector3, Vector3> onDrawLine)
        {
            if (pathPoints.Count > 0)
            {
                int index = pathPoints.Count - 1;
                onDrawLine(transform.Position, pathPoints[index]);
                for (int i = index; i > 0; i--)
                    onDrawLine(pathPoints[i], pathPoints[i - 1]);
            }
        }
    }
}