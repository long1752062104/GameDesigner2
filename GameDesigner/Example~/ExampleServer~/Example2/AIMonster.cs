using ECS;
using Net;
using Net.Component;
using Net.MMORPG;
using Net.Share;
using Net.System;

namespace Example2
{
    public class AIMonster : Component, IUpdate
    {
        internal NTransform transform;
        internal PatrolPath patrolPath;
        internal Scene scene;
        internal byte state;
        internal byte state1;
        private float idleTime;
        private int pointIndex;
        public float walkSpeed = 3f;
        public float moveSpeed = 6f;
        internal int id;
        internal int mid;
        internal bool isDeath;
        internal int health = 100;
        internal int targetID;
        
        public void OnUpdate()
        {
            if (isDeath)
                return;
            switch (state)
            {
                case 0:
                    Patrol();
                    break;
                case 1:
                    Authorize();
                    break;
            }
        }

        void Authorize() 
        {
            scene.AddOperation(new Operation(Command.EnemySync, id, transform.position, transform.rotation)
            {
                cmd1 = state,
                cmd2 = state1,
                index1 = health,
                index2 = targetID,
                buffer = new byte[] { (byte)mid }
            });
            if (targetID == 0)
            {
                state = 0;
                state1 = 0;
            }
        }

        void Patrol()
        {
            switch (state1)
            {
                case 0:
                    if (Time.time > idleTime)
                    {
                        state1 = (byte)RandomHelper.Range(0, 2);
                        idleTime = Time.time + RandomHelper.Range(0f, 2f);
                    }
                    break;
                case 1:
                    var dis = Vector3.Distance(transform.position, patrolPath.waypoints[pointIndex]);
                    if (dis < 0.1f)
                    {
                        pointIndex = RandomHelper.Range(0, patrolPath.waypoints.Count);
                        state1 = 0;
                        idleTime = Time.time + RandomHelper.Range(0f, 2f);
                    }
                    transform.LookAt(patrolPath.waypoints[pointIndex]);
                    transform.Translate(0, 0, walkSpeed * Time.deltaTime);
                    break;
            }
            PatrolCall();
        }

        internal void PatrolCall()
        {
            scene.AddOperation(new Operation(Command.AIMonster, id, transform.position, transform.rotation)
            {
                cmd1 = state,
                cmd2 = state1,
                index1 = health,
                buffer = new byte[] { (byte)mid }
            });
        }

        internal void OnDamage(int damage)
        {
            if (isDeath)
                return;
            health -= damage;
            if (health <= 0)
            {
                isDeath = true;
                health = 0;
                state1 = 4;
                ThreadManager.Event.AddEvent(10f, () =>
                {
                    health = 100;
                    isDeath = false;
                    state = 0;
                    state1 = 0;
                    targetID = 0;
                });
            }
            else 
            {
                state = 1;
            }
        }
    }
}
