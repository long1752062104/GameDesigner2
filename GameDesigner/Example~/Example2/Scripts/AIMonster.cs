﻿namespace Example2
{
    using GameDesigner;
    using Net.Client;
    using Net.Component;
    using Net.Share;
    using UnityEngine;

    /// <summary>
    /// 怪物组件, 此类的核心处理是: 当怪物被攻击后, 怪物的行为由攻击这个怪物的玩家客户端进行同步这个怪物的所有行为, 追击, 攻击, 位置同步等
    /// </summary>
    public class AIMonster : Actor
    {
        internal byte state;
        internal byte state1;
        public float walkSpeed = 3f;
        public Player target;
        public int targetID;//怪物攻击的玩家id

        void Awake()
        {
            preHealth = health;

            headBloodBar = Instantiate(GameManager.I.headBloodBar, GameManager.I.UIRoot);
            headBloodBar.target = transform;
            headBloodBar.offset = headBarOffset;
            headBloodBar.text.text = $"{health:f0}/{healthMax:f0}";
            headBloodBar.image.fillAmount = health / healthMax;
        }

        private void Update()
        {
            if (target != null & targetID == ClientBase.Instance.UID)
            {
                if (NetworkTime.CanSent)
                    ClientBase.Instance.AddOperation(new Operation(Command.EnemySync, id, transform.position, transform.rotation));
            }
            else if (state == 1 & targetID == ClientBase.Instance.UID)
            {
                if (NetworkTime.CanSent)
                    ClientBase.Instance.AddOperation(new Operation(Command.EnemySwitchState, id) { cmd1 = 0, cmd2 = 0 });
            }
        }

        internal void StatusEntry()
        {
            sm.StatusEntry(state1);
        }
    }

    public class MonsterIdle : StateBehaviour
    {
        private AIMonster self;
        private int number;

        public override void OnInit()
        {
            self = transform.GetComponent<AIMonster>();
        }

        public override void OnEnter()
        {
            number = 0;
        }

        public override void OnUpdate()
        {
            if (self.target != null & self.targetID == ClientBase.Instance.UID & number == 0)
            {
                var pos = self.target.transform.position;
                transform.LookAt(new Vector3(pos.x, transform.position.y, pos.z));
                var dis = Vector3.Distance(transform.position, self.target.transform.position);
                byte state1;
                if (dis > 15f)
                {
                    state1 = 0;
                    self.target = null;
                }
                else if (dis < 1.5f)
                {
                    state1 = 3;
                }
                else
                {
                    state1 = 2;
                }
                ClientBase.Instance.AddOperation(new Operation(Command.EnemySwitchState, self.id) { cmd1 = 1, cmd2 = state1 });
                number++;
            }
        }
    }

    public class MonsterRun : StateBehaviour
    {
        private AIMonster self;
        private int number;

        public override void OnInit()
        {
            self = transform.GetComponent<AIMonster>();
        }

        public override void OnEnter()
        {
            number = 0;
        }

        public override void OnUpdate()
        {
            if (self.target != null & self.targetID == ClientBase.Instance.UID & number == 0)
            {
                var pos = self.target.transform.position;
                transform.LookAt(new Vector3(pos.x, transform.position.y, pos.z));
                var dis = Vector3.Distance(transform.position, self.target.transform.position);
                byte state1;
                if (dis > 15f)
                {
                    state1 = 0;
                    ClientBase.Instance.AddOperation(new Operation(Command.EnemySwitchState, self.id) { cmd1 = 1, cmd2 = state1 });
                    number++;
                }
                else if (dis < 1.5f)
                {
                    state1 = 3;
                    ClientBase.Instance.AddOperation(new Operation(Command.EnemySwitchState, self.id) { cmd1 = 1, cmd2 = state1 });
                    number++;
                }
                else
                {
                    transform.Translate(0, 0, self.moveSpeed * Time.deltaTime);
                }
            }
        }
    }

    public class MonsterAttack : ActionCore//ActionBehaviour
    {
        private AIMonster self;
        public float distance = 3f;
        public float range = 90f;
        public float damage = 30f;
        public GameObject damageEffect;
        public override void OnInit()
        {
            self = transform.GetComponent<AIMonster>();
        }
        public override void OnAnimationEvent(StateAction action)
        {
            foreach (var p in GameManager.I.players)
            {
                var targetDir = p.transform.position - transform.position; //必须是 攻击对象 - 自身对象
                if (targetDir.magnitude > distance)
                    continue;
                var forward = transform.forward;
                var angle = Vector3.Angle(targetDir, forward); //最高是180
                if (angle < range & !p.isDead)//只能攻击本机玩家
                {
                    var effect = Object.Instantiate(damageEffect, p.transform.position, p.transform.rotation);
                    Object.Destroy(effect, 1f);
                    if(self.targetID == ClientBase.Instance.UID)
                        ClientBase.Instance.AddOperation(new Operation(Command.AIAttack, (int)damage));
                }
            }
            if (self.target != null)
            {
                if (self.target.isDead)
                    self.target = null;
            }
        }
    }

    public class MonsterDie : StateBehaviour
    {
        private AIMonster self;

        public override void OnInit()
        {
            self = transform.GetComponent<AIMonster>();
        }

        public override void OnEnter()
        {
            self.GetComponent<Collider>().enabled = false;
            self.GetComponent<Rigidbody>().isKinematic = true;
            self.target = null;
        }

        public override void OnExit()
        {
            self.GetComponent<Collider>().enabled = true;
            self.GetComponent<Rigidbody>().isKinematic = false;
            self.target = null;
        }
    }
}