using Net;
using Net.AI;
using Net.AOI;
using Net.Component;
using Net.MMORPG;
using Net.Server;
using Net.Share;
using Net.System;

namespace Example2
{
    /// <summary>
    /// 场景管理器, 状态同步, 帧同步 固定帧发送当前场景的所有玩家操作
    /// </summary>
    public class Scene : NetScene<Player>
    {
        internal MapData mapData = new MapData();
        internal readonly MyDictionary<int, AIMonster> monsters = new MyDictionary<int, AIMonster>();
        internal GridWorld gridWorld = new GridWorld();
        internal NavmeshSystem navmeshSystem = new NavmeshSystem();

        public void Init()
        {
            var ad = mapData.aoiData;
            gridWorld.Init(ad.xPos, ad.zPos, ad.xMax, ad.zMax, ad.width, ad.height);
            navmeshSystem.Init(mapData.navmeshPath);
            int id = 1;
            foreach (var item in mapData.monsterPoints)
            {
                if (item.monsters == null)
                    continue;
                var patrolPath = item.patrolPath;
                for (int i = 0; i < item.monsters.Length; i++)
                {
                    var position = patrolPath.waypoints[0];
                    var monster = new AIMonster();
                    monster.Agent = new AgentEntity(navmeshSystem) { agentHeight = 0f };
                    monster.Agent.SetPositionAndRotation(position, Quaternion.identity);
                    monster.Position = position;
                    monster.pointCenter = position;
                    monster.health = item.monsters[i].health;
                    monster.Identity = id;
                    monster.ActorID = 0;
                    gridWorld.Insert(monster);
                    monsters.Add(id++, monster);
                }
            }
        }

        public override void OnEnter(Player client)
        {
            client.Identity = client.UserID;
            client.MainRole = true;
            gridWorld.Insert(client);
        }
        public override void OnExit(Player client)
        {
            foreach (var monster in monsters.Values)
            {
                if (monster.target == client)
                {
                    monster.target = null;
                }
            }
            if (Count <= 0) //如果没人时要清除操作数据，不然下次进来会直接发送Command.OnPlayerExit指令给客户端，导致客户端的对象被销毁
                operations.Clear();
            else
                AddOperation(new Operation(Command.OnPlayerExit, client.UserID));
            gridWorld.Remove(client);
        }

        /// <summary>
        /// 网络帧同步, 状态同步更新
        /// </summary>
        public override void Update(IServerSendHandle<Player> handle, byte cmd = 19)
        {
            var players = Clients;
            int playerCount = players.Count;
            if (playerCount <= 0)
                return;
            frame++;
            int count;
            var areaOpers = new FastList<Operation>(); //九宫格区域操作帧数据
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null)
                    continue;
                player.OnUpdate();
                var grid = players[i].Grid;
                if (grid == null)
                    continue;
                areaOpers.AddRange(player.selfOpers.GetRemoveRange(0, player.selfOpers.Count));
                foreach (var body in grid)
                {
                    if (body is Player player1)
                    {
                        if (player1.frame != frame) //每帧只需要赋值一次
                        {
                            player1.preOpers = player1.currOpers.GetRemoveRange(0, player1.currOpers.Count);
                            player1.frame = frame;
                        }
                        areaOpers.AddRange(player1.preOpers);
                    }
                    else if (body is AIMonster monster)
                    {
                        if (monster.frame != frame) //每帧只需要赋值一次
                        {
                            monster.preOpers = monster.currOpers.GetRemoveRange(0, monster.currOpers.Count);
                            monster.frame = frame;
                        }
                        areaOpers.AddRange(monster.preOpers);
                    }
                }
                SendOperitions(handle, cmd, player, areaOpers);
            }
            SendOperitions(handle, cmd, this.operations); //不管aoi, 整个场景的同步在这里, 如玩家退出操作
            gridWorld.UpdateHandler();
        }

        public override void OnOperationSync(Player client, OperationList list)
        {
            for (int i = 0; i < list.operations.Length; i++)
            {
                var opt = list.operations[i];
                switch (opt.cmd)
                {
                    case Command.Attack:
                        if (monsters.TryGetValue(opt.identity, out AIMonster monster))
                        {
                            monster.target = client;
                            monster.OnDamage(opt.index1);
                        }
                        break;
                    case Command.AttackPlayer:
                        var players = Clients;
                        for (int n = 0; n < players.Count; n++)
                        {
                            if (players[n].UserID == opt.identity) 
                            {
                                players[n].BeAttacked(opt.index1);
                                break;
                            }
                        }
                        break;
                    case Command.Resurrection:
                        client.Resurrection();
                        client.currOpers.Add(opt);
                        break;
                    case Command.Transform:
                        client.Position = opt.position; //设置aoi位置
                        client.currOpers.Add(opt);
                        break;
                    default:
                        client.currOpers.Add(opt); //你的数据不需要向场景所有人转发, 只需要向九宫格内的玩家进行转发
                        break;
                }
            }
        }
    }
}