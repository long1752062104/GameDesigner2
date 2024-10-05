#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Client;
using Net.Component;
using Net.MMORPG;
using Net.Share;
using Net.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.LinearMath;
#else
using BEPUutilities;
#endif

namespace NonLockStep.Client
{
    public class GameWorld : SingleCase<GameWorld>
    {
        internal int frame;
        private readonly List<OperationList> snapshots = new();
        private int logicFrame;
        public uint delay;
        public GameObject player;
        public GameObject enemyObj;
        public GameObject boxPrefab;
        public GameObject bullet;
        public GameObject hitEffect;
        private Operation operation;
        public List<Actor> actors = new();
        public Dictionary<int, Actor> actorDic = new();
        private bool playback;
        public int frameRate = 30;
        public int frameLoopMax = 1;
        private float time;
        public NRigidbody[] colliders;
        public RoamingPath[] roamings;
        private readonly StringBuilder frameLog = new();
        private NVector3 playerPos;

        // Use this for initialization
        async void Start()
        {
            while (ClientBase.Instance == null)
                await Task.Yield();
            while (!ClientBase.Instance.Connected)
                await Task.Yield();
            ClientBase.Instance.OnOperationSync += OnOperationSync;
            ClientBase.Instance.AddRpcAuto(this, this);
            Physics.simulationMode = SimulationMode.Script;
            Physics.autoSyncTransforms = false;
            ThreadManager.Invoke(string.Empty, 1f, () =>
            {
                ClientBase.Instance?.Ping();
                return ClientBase.Instance != null;
            });
            ClientBase.Instance.OnPingCallback += (delay) =>
            {
                this.delay = delay;
            };
        }

        [Rpc]
        void StartGameSync()
        {
            foreach (var actor in actors)
                actor.Destroy();
            actorDic.Clear();
            actors.Clear();
            frame = 0;
            logicFrame = 0;
            playback = false;
            snapshots.Clear();
            frameLog.Clear();
            operation.cmd = Command.Input;
            operation.identity = ClientBase.Instance.UID;
        }

        [Rpc]
        void ExitBattle(int uid)
        {
            if (actorDic.TryGetValue(uid, out var actor))
            {
                actors.Remove(actor);
                actorDic.Remove(uid);
                actor.Destroy();
            }
            frame = 0;
            logicFrame = 0;
            Debug.Log("退出战斗");
        }

        [Rpc]
        void Playback()
        {
            foreach (var actor in actors)
                actor.Destroy();
            actorDic.Clear();
            actors.Clear();
            playback = true;
            logicFrame = 0;
            frameLog.Clear();
        }

        private void OnOperationSync(in OperationList list)
        {
            if (frame != list.frame)
            {
                Debug.Log($"帧错误:{frame} - {list.frame}");
                return;
            }
            frame++;
            snapshots.Add(list);
            ClientBase.Instance.AddOperation(operation);
            operation.cmd1 = 0;
        }

        // Update is called once per frame
        void Update()
        {
            int forLogic;
            if (!playback)
            {
                operation.direction = Transform3Dir(Camera.main.transform, Direction);
                if (Input.GetKeyDown(KeyCode.Q))
                    operation.cmd1 = 1;
                if (Input.GetKeyDown(KeyCode.Space))
                    operation.cmd1 = 2;
                forLogic = frame - logicFrame;
                forLogic = forLogic > frameLoopMax ? frameLoopMax : forLogic;
            }
            else
            {
                if (Time.time < time)
                    return;
                time = Time.time + (1f / frameRate);
                forLogic = frameLoopMax;
            }
            for (int i = 0; i < forLogic; i++)
            {
                if (logicFrame >= snapshots.Count - 2)
                    return;
                var list = snapshots[logicFrame];
                logicFrame++;
                GameUpdate(list);
            }
        }

        private void GameUpdate(OperationList list)
        {
            if (logicFrame == 1)
            {
                NTime.Init();
                NRandom.InitSeed(1752062104);
                NEvent.Init();
                NPhysics.Singleton.Initialize();
                playerPos.Z = -20f;
                for (int i = 0; i < colliders.Length; i++)
                    colliders[i].Initialize();
                for (int i = 0; i < roamings.Length; i++)
                {
                    for (int n = 0; n < 5; n++)
                    {
                        CreateEnemyActor(roamings[i]);
                    }
                }
                //BuildJenga(new NVector3(0, 1, 10f), 5);
            }
            for (int i = 0; i < list.operations.Length; i++)
            {
                var opt = list.operations[i];
                switch (opt.cmd)
                {
                    case Command.Input:
                        if (!actorDic.TryGetValue(opt.identity, out var actor))
                        {
                            actor = CreatePlayerActor(opt);
                            actor.name = opt.identity.ToString();
                            actorDic.Add(opt.identity, actor);
                        }
                        actor.operation = opt;
                        break;
                    case NetCmd.QuitGame:
                        if (actorDic.TryGetValue(opt.identity, out var actor1))
                        {
                            actors.Remove(actor1);
                            actorDic.Remove(opt.identity);
                            actor1.Destroy();
                        }
                        break;
                }
            }
            for (int i = 0; i < actors.Count; i++)
                actors[i].Update();
            NPhysics.Singleton.Simulate();
            NTime.Update();
            NEvent.Update();
            frameLog.AppendLine($"frame:{logicFrame}");
            for (int i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                frameLog.AppendLine($"actorId: {i} pos: ({actor.rigidBody.Position}) rot: ({actor.rigidBody.Rotation}) vel: ({actor.rigidBody.Velocity})");
            }
        }

        private Player CreatePlayerActor(Operation opt)
        {
            var gameObject = Instantiate(player);
            var actor = new Player
            {
                name = opt.identity.ToString(),
                gameObject = gameObject,
                animation = gameObject.GetComponent<Animation>(),
                rigidBody = gameObject.GetComponent<NRigidbody>(),
                Health = 1000,
            };
            gameObject.transform.position = playerPos;
            playerPos.Z -= 5f;
            actor.rigidBody.Tag = actor;
            actor.rigidBody.Initialize();
            if (opt.identity == ClientBase.Instance.UID)
                FindObjectOfType<ARPGcamera>().target = gameObject.transform;
            actors.Add(actor);
            actor.Start();
            return actor;
        }

        private Actor CreateEnemyActor(RoamingPath roamingPath)
        {
            var gameObject = Instantiate(enemyObj);
            var actor = new Enemy
            {
                name = "enemy",
                gameObject = gameObject,
                animation = gameObject.GetComponent<Animation>(),
                rigidBody = gameObject.GetComponent<NRigidbody>(),
                roamingPath = roamingPath,
            };
            gameObject.transform.position = roamingPath.waypointsList[NRandom.Range(0, roamingPath.waypointsList.Count)];
            actor.rigidBody.Tag = actor;
            actor.rigidBody.Initialize();
            actors.Add(actor);
            actor.Start();
            return actor;
        }

        public Actor CreateBulletActor(Actor self, Vector3 position, Vector3 direction)
        {
            var gameObject = Instantiate(bullet);
            var actor = new Bullet
            {
                name = "bullet",
                gameObject = gameObject,
                animation = gameObject.GetComponent<Animation>(),
                rigidBody = gameObject.GetComponent<NRigidbody>(),
                direction = direction,
                This = self,
                explosionPrefab = hitEffect,
            };
            gameObject.transform.position = position;
            actor.rigidBody.Tag = actor;
            actor.rigidBody.Initialize();
            actor.Init();
            actors.Add(actor);
            actor.Start();
            NEvent.AddEvent(5f, () => RemoveActor(actor));
            return actor;
        }

        public void RemoveActor(Actor actor)
        {
            actors.Remove(actor);
            actor.Destroy();
        }

        public Vector3 Direction => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        public Vector3 Transform3Dir(Transform t, Vector3 dir)
        {
            var f = Mathf.Deg2Rad * (-t.rotation.eulerAngles.y);
            dir.Normalize();
            var ret = new Vector3(dir.x * Mathf.Cos(f) - dir.z * Mathf.Sin(f), 0, dir.x * Mathf.Sin(f) + dir.z * Mathf.Cos(f));
            return ret;
        }

        private void OnDestroy()
        {
            if (ClientBase.Instance != null)
                ClientBase.Instance.OnOperationSync -= OnOperationSync;
        }

        public void BuildJenga(NVector3 position, int size = 20)
        {
            position += new NVector3(0, 0.5f, 0);
            for (int i = 0; i < size; i++)
            {
                for (int e = 0; e < 3; e++)
                {
                    var gameObject = Instantiate(boxPrefab);
                    if (i % 2 == 0)
                    {
                        gameObject.transform.localScale = new Vector3(3, 1, 1);
                        gameObject.transform.position = position + new NVector3(0, i, -1 + e);
                    }
                    else
                    {
                        gameObject.transform.localScale = new Vector3(1, 1, 3);
                        gameObject.transform.position = position + new NVector3(-1 + e, i, 0);
                    }
                    var actor = new Box()
                    {
                        gameObject = gameObject,
                        rigidBody = gameObject.GetComponent<NRigidbody>()
                    };
                    if (actor.rigidBody != null)
                        actor.rigidBody.Initialize();
                    actors.Add(actor);
                }
            }
        }

        internal void SaveData()
        {
            var now = DateTime.Now;
#if UNITY_ANDROID && !UNITY_EDITOR
            //雷电模拟器共享路径
            var file = "/mnt/shared/Pictures/" + $"FRAME{now.Year}{now.Month:00}{now.Day:00}{now.Hour:00}{now.Minute:00}{now.Second:00}.txt";
#else
            var file = Application.persistentDataPath + "/" + $"FRAME{now.Year}{now.Month:00}{now.Day:00}{now.Hour:00}{now.Minute:00}{now.Second:00}.txt";
#endif
            File.WriteAllText(file, frameLog.ToString());
            Debug.Log($"数据保存成功:{file}");
        }
    }
}
#endif