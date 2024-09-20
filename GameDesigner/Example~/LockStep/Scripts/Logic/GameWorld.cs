#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.LinearMath;
using Net.Client;
using Net.Component;
using Net.Event;
using Net.Share;
using Net.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LockStep.Client
{
    public class GameWorld : SingleCase<GameWorld>
    {
        internal int frame;
        private readonly List<OperationList> snapshots = new List<OperationList>();
        private int logicFrame;
        public uint delay;
        public GameObject player;
        public GameObject enemyObj;
        public GameObject boxPrefab;
        private Net.Vector3 direction;

        public List<Actor> actors = new List<Actor>();
        public Dictionary<int, Actor> actorDic = new Dictionary<int, Actor>();
        private bool playback;
        public int frameRate = 30;
        public int frameFor = 1;
        private float time;
        public JCollider[] colliders;
        public TimerEvent Event;
        private StringBuilder frameLog = new StringBuilder();

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
            ClientBase.Instance.AddOperation(new Operation(Command.Input, ClientBase.Instance.UID, direction));
        }

        // Update is called once per frame
        void Update()
        {
            int forLogic;
            if (!playback)
            {
                direction = Transform3Dir(Camera.main.transform, Direction);
                forLogic = frame - logicFrame;
            }
            else
            {
                if (Time.time < time)
                    return;
                time = Time.time + (1f / frameRate);
                forLogic = frameFor;
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
                Event = new TimerEvent();
                JPhysics.ReBuild();
                for (int i = 0; i < colliders.Length; i++)
                    colliders[i].InitRigidBody();
                BuildJenga(new JVector(0, 0, 10f));
            }
            if (list.operations == null)
                return;
            LSTime.time += LSTime.deltaTime;//最先执行的时间,逻辑时间
            for (int i = 0; i < list.operations.Length; i++)
            {
                var opt = list.operations[i];
                switch (opt.cmd)
                {
                    case Command.Input:
                        if (!actorDic.TryGetValue(opt.identity, out var actor))
                        {
                            actor = CreateActor(opt);
                            actor.name = opt.identity.ToString();
                            actorDic.Add(opt.identity, actor);
                            actors.Add(actor);
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
            Physics.Simulate(0.01666667f);
            JPhysics.Singleton.Simulate();
            Event.UpdateEvent();
            frameLog.AppendLine($"frame:{logicFrame}");
            for (int i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                frameLog.AppendLine($"actorId: {i} pos: ({actor.jCollider.Position}) rot: ({actor.jCollider.Rotation})");
            }
        }

        private Player CreateActor(Operation opt)
        {
            var gameObject = Instantiate(player);
            var actor = new Player
            {
                name = opt.identity.ToString(),
                gameObject = gameObject,
                anim = gameObject.GetComponent<Animation>(),
                rigidBody = gameObject.GetComponent<Rigidbody>(),
                jCollider = gameObject.GetComponent<JCollider>()
            };
            if (actor.jCollider != null)
                actor.jCollider.Initialize();
            if (opt.identity == ClientBase.Instance.UID)
                FindObjectOfType<ARPGcamera>().target = gameObject.transform;
            return actor;
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

        public void BuildJenga(JVector position, int size = 20)
        {
            position += new JVector(0, 0.5f, 0);
            for (int i = 0; i < size; i++)
            {
                for (int e = 0; e < 3; e++)
                {
                    var gameObject = Instantiate(boxPrefab);
                    if (i % 2 == 0)
                    {
                        gameObject.transform.localScale = new Vector3(3, 1, 1);
                        gameObject.transform.position = position + new JVector(0, i, -1 + e);
                    }
                    else
                    {
                        gameObject.transform.localScale = new Vector3(1, 1, 3);
                        gameObject.transform.position = position + new JVector(-1 + e, i, 0);
                    }
                    var actor = new Box()
                    {
                        gameObject = gameObject,
                        rigidBody = gameObject.GetComponent<Rigidbody>(),
                        jCollider = gameObject.GetComponent<JCollider>()
                    };
                    if (actor.jCollider != null)
                        actor.jCollider.Initialize();
                    actors.Add(actor);
                }
            }
        }

        internal void SaveData()
        {
            var now = DateTime.Now;
            var file = Application.streamingAssetsPath + "/" + $"{now.Year}{now.Month:00}{now.Day:00}{now.Hour:00}{now.Minute:00}{now.Second:00}.txt";
            File.WriteAllText(file, frameLog.ToString());
            Debug.Log($"数据保存成功:{file}");
        }
    }
}
#endif