using Net;
using Net.AI;
using Net.MMORPG;
using Net.Share;
using Net.System;
using Net.AOI;

namespace Example2
{
    public class AIMonster : IGridBody, IGridActor
    {
        public int ID { get; set; }
        public int Identity { get; set; }
        public Vector3 Position { get; set; }
        public Grid Grid { get; set; }
        public int Hair { get; set; }
        public int Head { get; set; }
        public int Jacket { get; set; }
        public int Belt { get; set; }
        public int Pants { get; set; }
        public int Shoe { get; set; }
        public int Weapon { get; set; }
        public int ActorID { get; set; }
        public bool MainRole { get; set; }

        public AgentEntity Agent;

        public Player target; //攻击对象

        public ListSafe<Operation> currOpers = new ListSafe<Operation>();
        public Operation[] preOpers;
        public uint frame;

        internal Vector3 pointCenter;
        internal Vector3 destination;
        private float idleTime;
        internal bool isDeath;
        internal int health = 100;

        public int roleInCount; //有主角在这个区域才能添加操作, 否则会堆积很多数据

        public void OnStart()
        {
        }

        public void OnBodyUpdate()
        {
            Position = Agent.transform.Position;
            if (isDeath)
                return;
            if (target == null)
            {
                if (Time.time > idleTime)
                {
                    idleTime = Time.time + RandomHelper.Range(0f, 10f);
                    destination = new Vector3(RandomHelper.Range(pointCenter.x - 50f, pointCenter.x + 50f), 0f, RandomHelper.Range(pointCenter.z - 50f, pointCenter.z + 50f));
                    if (Agent.SetDestination(destination))
                    {
                        destination = Agent.Destination; //获取目的地位置, 这里才包含y的值
                        if (roleInCount > 0)
                        {
                            currOpers.Add(new Operation(Command.EnterArea, Identity, Position, Agent.Rotation)
                            {
                                index = 2,
                                index1 = ActorID,
                                index2 = health,
                                direction = destination, //怪物下一个位置, 客户端收到后会自动寻路到目的地
                            });
                        }
                    }
                }
            }
            else
            {
                var d = Vector3.Distance(Position, target.Position);
                if (d < 1.5f)
                {
                    //TODO 攻击玩家
                }
                else if (d < 10f)
                {
                    //TODO 追击玩家
                    Agent.SetDestination(target.Position);
                }
                else
                {
                    target = null;
                }
            }
            Agent.OnUpdate(Time.deltaTime);
        }

        public void OnEnter(IGridBody body)
        {
        }

        public void OnExit(IGridBody body)
        {
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
                EventManager.Event.AddEvent(10f, () =>
                {
                    health = 100;
                    isDeath = false;
                    target = null;
                });
                //TODO 死亡通知
            }
        }

        public void OnInit()
        {
        }
    }
}
