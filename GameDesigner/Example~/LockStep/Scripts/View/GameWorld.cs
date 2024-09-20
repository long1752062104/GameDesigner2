#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.LinearMath;
using Net.Client;
using Net.Component;
using Net.Event;
using Net.Share;
using Net.System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LockStep.Client
{
    public class GameWorld : SingleCase<GameWorld>
    {
        private int frame;
        private readonly List<OperationList> snapshots = new List<OperationList>();
        private int logicFrame, frame1;
        public int frame2;
        public uint delay;
        public GameObject player;
        public GameObject enemyObj;
        public GameObject boxPrefab;
        public Net.Vector3 direction;

        public List<Player> actors = new List<Player>();
        public List<GameObject> boxs = new List<GameObject>();
        public Dictionary<int, Player> actorDic = new Dictionary<int, Player>();
        private bool playback;
        public int frameRate = 30;
        public int frameFor = 1;
        private float time;
        public JCollider[] colliders;
        public TimerEvent Event;

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
                frame2 = frame - frame1;
                frame1 = frame;
                ClientBase.Instance?.Ping();
                return ClientBase.Instance != null;
            });
            ClientBase.Instance.OnPingCallback += (delay) =>
            {
                this.delay = delay;
            };
        }

        [Rpc]
        void ExitBattle(int uid)
        {
            if (actorDic.TryGetValue(uid, out Player actor1))
            {
                actors.Remove(actor1);
                actorDic.Remove(uid);
                actor1.Destroy();
            }
            frame = 0;
            logicFrame = 0;
            //snapshots.Clear();
            Debug.Log("退出战斗");
        }

        [Rpc]
        void Playback()
        {
            foreach (var player in actorDic.Values)
            {
                player.Destroy();
            }
            actorDic.Clear();
            actors.Clear();
            playback = true;
            logicFrame = 0;
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
                {
                    colliders[i].InitRigidBody();
                }
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
                        if (!actorDic.TryGetValue(opt.identity, out Player actor))
                        {
                            actor = CreateActor(opt);
                            actor.name = opt.identity.ToString();
                            actorDic.Add(opt.identity, actor);
                            actors.Add(actor);
                        }
                        actor.opt = opt;
                        break;
                    case NetCmd.QuitGame:
                        if (actorDic.TryGetValue(opt.identity, out Player actor1))
                        {
                            actors.Remove(actor1);
                            actorDic.Remove(opt.identity);
                            actor1.Destroy();
                        }
                        break;
                }
            }
            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].OnUpdate();
            }
            Physics.Simulate(0.01666667f);
            JPhysics.Singleton.Simulate();
            Event.UpdateEvent();
        }

        private Player CreateActor(Operation opt)
        {
            var actor = new Player
            {
                name = opt.identity.ToString(),
                gameObject = Instantiate(player)
            };
            actor.objectView = actor.gameObject.GetComponent<ObjectView>();
            actor.objectView.actor = actor;
            actor.objectView.anim = actor.gameObject.GetComponent<Animation>();
            actor.rigidBody = actor.gameObject.GetComponent<Rigidbody>();
            actor.jCollider = actor.gameObject.GetComponent<JCapsuleCollider>();
            actor.jCollider?.Initialize();
            if (opt.identity == ClientBase.Instance.UID)
                FindObjectOfType<ARPGcamera>().target = actor.gameObject.transform;
            return actor;
        }

        public Vector3 Direction
        {
            get { return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); }
        }

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
            boxs.ClearObjects();

            position += new JVector(0, 0.5f, 0);

            for (int i = 0; i < size; i++)
            {
                for (int e = 0; e < 3; e++)
                {
                    var box = Instantiate(boxPrefab);
                    if (i % 2 == 0)
                    {
                        //body.AddShape(new BoxShape(3, 1, 1));
                        box.transform.position = position + new JVector(0, i, -1 + e);
                    }
                    else
                    {
                        //body.AddShape(new BoxShape(1, 1, 3));
                        box.transform.position = position + new JVector(-1 + e, i, 0);
                    }
                    if (box.TryGetComponent<JBoxCollider>(out var boxCollider))
                    {
                        boxCollider.InitRigidBody();
                    }
                    boxs.Add(box);
                }
            }
        }
    }
}
#endif