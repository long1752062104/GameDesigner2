#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace Example2
{
    using Example2.Model;
    using Net.AI;
    using Net.Share;
    using Net.System;
    using Net.UnityComponent;

    public class SceneManager : NetworkSceneManager
    {
        public MonsterView[] monsters;
        internal MyDictionary<int, MonsterView> monsterDics = new MyDictionary<int, MonsterView>();

        public override void OnNetworkObjectCreate(Operation opt, NetworkObject identity)
        {
            var p = identity.GetComponent<Player>();
            if (p != null)
            {
                p.id = opt.identity;
                GameManager.I.players.Add(p);
            }
        }

        public override void OnOtherDestroy(NetworkObject identity)
        {
            var p = identity.GetComponent<Player>();
            if (p != null)
            {
                Destroy(p.headBloodBar.gameObject);
                Destroy(p.gameObject);
                GameManager.I.players.Remove(p);
            }
        }

        public MonsterView CheckMonster(Operation opt)
        {
            if (!monsterDics.TryGetValue(opt.identity, out MonsterView monster))
            {
                var monsterObj = monsters[opt.index1];
                monster = Instantiate(monsterObj, opt.position, opt.rotation);
                monster.Self = new Monster();
                monster.Self.Agent = new AgentEntity(NavmeshSystemUnity.I.System) { agentHeight = 0f, findPathMode = FindPathMode.FindPathStraight };
                monster.Self.Agent.SetPositionAndRotation(opt.position, opt.rotation);
                monsterDics.Add(opt.identity, monster);
            }
            return monster;
        }

        public override void OnOtherOperator(Operation opt)
        {
            switch (opt.cmd)
            {
                case Command.EnterArea:
                    {
                        if (opt.index == 1) //玩家
                        {
                            var identity = OnCheckIdentity(opt);
                            if (identity == null)
                                return;
                            if (identity.IsDispose)
                                return;
                            identity.name = opt.identity.ToString();
                            if (identity.TryGetComponent<NetworkTransform>(out var nt))
                                nt.SetNetworkPositionAndRotation(opt.position, opt.rotation);
                            identity.transform.SetPositionAndRotation(opt.position, opt.rotation);
                            identity.gameObject.SetActive(true);
                        }
                        else if (opt.index == 2) //怪物
                        {
                            var identity = CheckMonster(opt);
                            if (identity == null)
                                return;
                            identity.name = opt.identity.ToString();
                            identity.Self.Agent.SetPositionAndRotation(opt.position, opt.rotation);
                            identity.Self.Agent.SetDestination(opt.direction);
                            identity.transform.SetPositionAndRotation(opt.position, opt.rotation); //视图的位置和旋转也要立马刷新
                            identity.gameObject.SetActive(true);
                        }
                    }
                    break;
                case Command.ExitArea:
                    {
                        if (opt.index == 1) //玩家
                        {
                            if (identitys.TryGetValue(opt.identity, out var netObj))
                                netObj.gameObject.SetActive(false);
                        }
                        else if (opt.index == 2) //怪物
                        {
                            if (monsterDics.TryGetValue(opt.identity, out MonsterView monster))
                                monster.gameObject.SetActive(false);
                        }

                    }
                    break;
                case Command.Fire:
                    {
                        if (identitys.TryGetValue(opt.identity, out var t))
                        {
                            var p = t.GetComponent<Player>();
                            p.Fire();
                        }
                    }
                    break;
                case Command.AIMonster:
                    //var monster = CheckMonster(opt);
                    //monster.state = opt.cmd1;
                    //monster.state1 = opt.cmd2;
                    //monster.health = opt.index1;
                    //monster.StatusEntry();
                    //monster.transform.position = opt.position;
                    //monster.transform.rotation = opt.rotation;
                    //if (monster.health != monster.preHealth) 
                    //{
                    //    monster.BeAttacked(null, monster.health - monster.preHealth);
                    //    monster.preHealth = monster.health;
                    //}
                    break;
                case Command.EnemySync:
                    //var monster1 = CheckMonster(opt);
                    //monster1.state = opt.cmd1;
                    //monster1.state1 = opt.cmd2;
                    //monster1.health = opt.index1;
                    //monster1.targetID = opt.index2;
                    //if (monster1.targetID != ClientBase.Instance.UID)
                    //{
                    //    monster1.transform.position = opt.position;
                    //    monster1.transform.rotation = opt.rotation;
                    //}
                    //if (monster1.health != monster1.preHealth)
                    //{
                    //    monster1.BeAttacked(null, monster1.health - monster1.preHealth);
                    //    monster1.preHealth = monster1.health;
                    //}
                    break;
                case Command.EnemySwitchState:
                    //var monster2 = CheckMonster(opt);
                    //monster2.state = opt.cmd1;
                    //monster2.state1 = opt.cmd2;
                    //monster2.StatusEntry();
                    break;
                case Command.PlayerState:
                    {
                        if (identitys.TryGetValue(opt.identity, out var t))
                        {
                            var p = t.GetComponent<Player>();
                            p.health = opt.index1;
                            if (p.health != p.preHealth)
                            {
                                p.BeAttacked(null, p.health - p.preHealth);
                                p.preHealth = p.health;
                            }
                            p.Check();
                        }
                    }
                    break;
                case Command.Resurrection:
                    {
                        if (identitys.TryGetValue(opt.identity, out var t))
                        {
                            var p = t.GetComponent<Player>();
                            p.Resurrection();
                        }
                    }
                    break;
            }
        }
    }
}
#endif