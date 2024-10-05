#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using SoftFloat;
using Net.MMORPG;
using GameDesigner;
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using Jitter2.LinearMath;
#else
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Entities.Prefabs;
#endif

namespace NonLockStep.Client
{
    [Serializable]
    public class Enemy : Actor
    {
        public RoamingPath roamingPath; // 巡逻点数组
        public sfloat moveSpeed = 6f;          // 移动速度
        internal int currentPatrolIndex;   // 当前巡逻点索引
        internal NVector3 targetPosition;    // 目标位置
        public Actor target;
        public StateMachineCore fsm = new();

        public override void Start()
        {
            fsm.Handler = new TimeStateMachine();
            fsm.Init();
            fsm.AddState("idle", new EnemyIdle(this)).animLoop = true;
            fsm.AddState("walk", new EnemyWalk(this)).animLoop = true;
            fsm.AddState("run", new EnemyRun(this)).animLoop = true;
            var fireState = fsm.AddState("attack");
            fireState.Action.AddComponent(new EnemyAttack(this));
            fireState.AddTransition(0);
            fireState.isExitState = true;
            fireState.DstStateID = 0;
            fireState.actionSystem = true;
            fireState.animSpeed = 2f;
            fsm.AddState("die", new EnemyDie(this));
        }

        public override void Update()
        {
            fsm.Execute(StateMachineUpdateMode.Update);
        }

        public override void OnDamage(Actor attack)
        {
            if (IsDeath)
                return;
            Health -= attack.Damage;
            if (Health <= 0)
            {
                fsm.ChangeState(4, 0, true);
                NEvent.AddEvent(5f, () => GameWorld.I.RemoveActor(this));
            }
            else
            {
                target = attack;
            }
        }
    }

    public class EnemyIdle : StateBehaviour
    {
        private readonly Enemy self;

        public EnemyIdle(Enemy player)
        {
            self = player;
        }

        public override void OnEnter()
        {
            self.animation.Play("idle");
            self.rigidBody.Translate(0, 0, 0);
        }

        public override void OnUpdate()
        {
            if (self.target == null)
            {
                ChangeState(1);
            }
            else
            {
                if (self.target.IsDeath)
                {
                    self.target = null;
                    return;
                }
                var dis = NVector3.Distance(self.target.Position, self.Position);
                if (dis < 3f)
                {
                    ChangeState(3);
                }
                else if (dis < 15f)
                {
                    ChangeState(2);
                }
                else
                {
                    self.target = null;
                }
            }
        }
    }

    public class EnemyWalk : StateBehaviour
    {
        private readonly Enemy self;

        public EnemyWalk(Enemy player)
        {
            self = player;
        }

        public override void OnEnter()
        {
            self.animation.Play("walk");
            self.rigidBody.Translate(0, 0, 0);
        }

        public override void OnUpdate()
        {
            if (self.target == null)
            {
                if (NVector3.Distance(self.Position, self.targetPosition) < 1.5f)
                {
                    self.currentPatrolIndex = NRandom.Range(0, self.roamingPath.waypointsList.Count); // 随机选择下一个巡逻点
                    self.targetPosition = (Vector3)self.roamingPath.waypointsList[self.currentPatrolIndex]; // 更新目标位置
                }
                var direction = (self.targetPosition - self.Position).Normalized; // 计算方向
                self.Rotation = NQuaternion.Lerp(self.Rotation, NQuaternion.LookRotation(direction, NVector3.UnitY), 0.5f);
                self.rigidBody.Translate(0, 0, self.moveSpeed * NTime.deltaTime);
            }
            else
            {
                if (self.target.IsDeath)
                {
                    self.target = null;
                    return;
                }
                var dis = NVector3.Distance(self.target.Position, self.Position);
                if (dis < 3f)
                {
                    ChangeState(3);
                }
                else if (dis < 15f)
                {
                    ChangeState(2);
                }
                else
                {
                    self.target = null;
                }
            }
        }
    }

    public class EnemyRun : StateBehaviour
    {
        private readonly Enemy self;

        public EnemyRun(Enemy player)
        {
            self = player;
        }

        public override void OnEnter()
        {
            self.animation.Play("run");
            self.rigidBody.Translate(0, 0, 0);
        }

        public override void OnUpdate()
        {
            if (self.target == null)
            {
                ChangeState(0);
            }
            else
            {
                if (self.target.IsDeath)
                {
                    self.target = null;
                    return;
                }
                var dis = NVector3.Distance(self.target.Position, self.Position);
                if (dis > 15f)
                {
                    self.target = null;
                    return;
                }
                if (dis < 3f)
                {
                    ChangeState(3);
                }
                else
                {
                    var direction = (self.target.Position - self.Position).Normalized; // 计算方向
                    self.Rotation = NQuaternion.Lerp(self.Rotation, NQuaternion.LookRotation(direction, NVector3.UnitY), 0.5f);
                    self.rigidBody.Translate(0, 0, self.moveSpeed * NTime.deltaTime);
                }
            }
        }
    }

    public class EnemyAttack : ActionCoreBase
    {
        private readonly Enemy self;

        public EnemyAttack(Enemy player)
        {
            self = player;
        }

        public override void OnEnter(StateAction action)
        {
            self.animation.Play("charge_end");
            self.rigidBody.Translate(0, 0, 0);
            self.rigidBody.LookAt(self.target.Position, NVector3.UnitY);
        }

        public override void OnAnimationEvent(StateAction action)
        {
            self.target?.OnDamage(self);
        }
    }

    public class EnemyDie : StateBehaviour
    {
        private readonly Enemy self;

        public EnemyDie(Enemy player)
        {
            self = player;
        }

        public override void OnEnter()
        {
            self.animation.Play("die");
            self.rigidBody.Translate(0, 0, 0);
            //self.rigidBody.rigidBody.Friction = 1f;
        }
    }
}
#endif