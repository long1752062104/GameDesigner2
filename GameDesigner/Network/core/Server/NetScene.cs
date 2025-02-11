﻿namespace Net.Server
{
    using global::System;
    using global::System.Collections.Generic;
    using Net.Common;
    using Net.Helper;
    using Net.Share;
    using Net.System;

    /// <summary>
    /// 网络场景
    /// </summary>
    public class NetScene<Player> where Player : NetPlayer
    {
        /// <summary>
        /// 场景名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 场景容纳人数
        /// </summary>
        public int sceneCapacity;
        /// <summary>
        /// 当前网络场景的玩家, 此字段不要使用Add, Remove进行调用
        /// </summary>
        public FastList<Player> Players { get; private set; } = new FastList<Player>();
        public FastList<Player> Clients => Players;
        /// <summary>
        /// 当前帧
        /// </summary>
        public uint frame;
        /// <summary>
        /// 备用操作, 当玩家被移除后速度比update更新要快而没有地方收集操作指令, 所以在玩家即将被移除时, 可以访问这个变量进行添加操作同步数据
        /// </summary>
        protected FastList<Operation> operations = new FastList<Operation>();
        public SerializeOptDelegate onSerializeOpt;
        public SerializeRpcDelegate onSerializeRpc;
        /// <summary>
        /// 获取场景当前人数
        /// </summary>
        public int SceneNumber
        {
            get { return Players.Count; }
        }
        /// <summary>
        /// 获取场景当前人数
        /// </summary>
        public int Count
        {
            get { return Players.Count; }
        }
        /// <summary>
        /// 获取场景容纳人数
        /// </summary>
        public int Capacity
        {
            get { return sceneCapacity; }
            set { sceneCapacity = value; }
        }
        /// <summary>
        /// 场景(房间)人数是否已满？
        /// </summary>
        public bool IsFull
        {
            get { return Players.Count >= sceneCapacity; }
        }
        /// <summary>
        /// 操作列表分段值, 当operations.Count的长度大于Split值时, 就会裁剪为多段数据发送 默认为500长度分段
        /// </summary>
        public int Split { get; set; } = 500;
        private readonly int hash;
        internal int preFps, currFps;
        /// <summary>
        /// 场景帧数
        /// </summary>
        public int FPS { get => preFps; }
        private readonly FastLocking Locking = new FastLocking();

        /// <summary>
        /// 构造网络场景
        /// </summary>
        public NetScene()
        {
            hash = GetHashCode();
        }

        /// <summary>
        /// 添加网络主场景并增加主场景最大容纳人数
        /// </summary>
        /// <param name="number">主场景最大容纳人数</param>
        public NetScene(int number)
        {
            sceneCapacity = number;
        }

        /// <summary>
        /// 添加网络场景并增加当前场景人数
        /// </summary>
        /// <param name="client">网络玩家</param>
        /// <param name="number">创建场景容纳人数</param>
        public NetScene(Player client, int number)
        {
            sceneCapacity = number;
            AddPlayer(client);
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="client"></param>
        public virtual void AddPlayer(Player client)
        {
            Locking.Enter();
            try
            {
                AddPlayerInternal(client);
            }
            finally
            {
                Locking.Exit();
            }
        }

        private void AddPlayerInternal(Player client)
        {
            if (client.SceneHash == hash)
                return;
            //如果已经在场景里面, 必须要先退出, 否则会发生一个玩家在多个场景的重大问题, 当玩家在多个场景后, 客户端被移除就找不到这个玩家进行移除,就会导致内存泄露问题
            if (client.Scene is NetScene<Player> preScene)
                preScene.Remove(client);
            client.SceneName = Name;
            client.Scene = this;
            Players.Add(client);
            OnEnter(client);
            client.OnEnter();
            client.SceneHash = hash;
        }

        /// <summary>
        /// 当进入场景的玩家
        /// </summary>
        /// <param name="client"></param>
        public virtual void OnEnter(Player client) { }

        /// <summary>
        /// 当开始退出场景，当调用此方法时client还在Clients属性里面
        /// </summary>
        /// <param name="client"></param>
        public virtual void OnBeginExit(Player client) { }

        /// <summary>
        /// 当退出场景的玩家, 当调用此方法后client已经被移出Clients属性
        /// </summary>
        /// <param name="client"></param>
        public virtual void OnExit(Player client) { }

        /// <summary>
        /// 当场景被移除
        /// </summary>
        public virtual void OnRemove() { }

        /// <summary>
        /// 当接收到客户端使用Client.AddOperation方法发送的请求时调用
        /// </summary>
        public virtual void OnOperationSync(Player client, OperationList list)
        {
            AddOperations(list.operations);
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="collection"></param>
        public virtual void AddPlayers(IEnumerable<Player> collection)
        {
            Locking.Enter();
            try
            {
                foreach (var client in collection)
                    AddPlayerInternal(client);
            }
            finally
            {
                Locking.Exit();
            }
        }

        public void UpdateLock(IServerSendHandle<Player> handle, byte cmd)
        {
            Locking.Enter(); //解决多线程问题
            try
            {
                Update(handle, cmd);
            }
            finally
            {
                Locking.Exit();
            }
        }

        /// <summary>
        /// 网络帧同步, 状态同步更新, 帧时间根据服务器主类的SyncSceneTime属性来调整速率
        /// </summary>
        public virtual void Update(IServerSendHandle<Player> handle, byte cmd)
        {
            var players = Players;
            int playerCount = players.Count;
            if (playerCount <= 0)
                return;
            for (int i = 0; i < playerCount; i++)
                players[i].OnUpdate();
            if (operations.Count > 0)
            {
                frame++;
                SendOperitions(handle, cmd, operations);
            }
        }

        /// <summary>
        /// 发送操作, 如果超过Split属性则会进行拆分发送
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cmd"></param>
        /// <param name="operations"></param>
        public void SendOperitions(IServerSendHandle<Player> handle, byte cmd, FastList<Operation> operations)
        {
            int count = operations.Count;
            while (count > Split)
            {
                OnPacket(handle, cmd, Split, Players, operations);
                count -= Split;
            }
            if (count > 0) OnPacket(handle, cmd, count, Players, operations);
        }

        /// <summary>
        /// 发送操作, 如果超过Split属性则会进行拆分发送
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cmd"></param>
        /// <param name="client"></param>
        /// <param name="operations"></param>
        public void SendOperitions(IServerSendHandle<Player> handle, byte cmd, Player client, FastList<Operation> operations)
        {
            int count = operations.Count;
            while (count > Split)
            {
                OnPacket(handle, cmd, Split, client, operations);
                count -= Split;
            }
            if (count > 0) OnPacket(handle, cmd, count, client, operations);
        }

        /// <summary>
        /// 当封包数据时调用
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cmd"></param>
        /// <param name="count"></param>
        public virtual void OnPacket(IServerSendHandle<Player> handle, byte cmd, int count)
        {
            OnPacket(handle, cmd, count, Players, operations);
        }

        /// <summary>
        /// 当封包数据时调用
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cmd"></param>
        /// <param name="count"></param>
        /// <param name="players"></param>
        /// <param name="operations"></param>
        public virtual void OnPacket(IServerSendHandle<Player> handle, byte cmd, int count, FastList<Player> players, FastList<Operation> operations)
        {
            var opts = operations.GetRemoveRange(0, count);
            var operList = new OperationList(frame, opts);
            var buffer = onSerializeOpt(operList);
            handle.Multicast(players, cmd, buffer, false, false);
        }

        /// <summary>
        /// 当封包数据时调用
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cmd"></param>
        /// <param name="count"></param>
        /// <param name="client"></param>
        /// <param name="operations"></param>
        public virtual void OnPacket(IServerSendHandle<Player> handle, byte cmd, int count, Player client, FastList<Operation> operations)
        {
            var opts = operations.GetRemoveRange(0, count);
            var operList = new OperationList(frame, opts);
            var buffer = onSerializeOpt(operList);
            handle.Call(client, cmd, buffer, false, false);
        }

        /// <summary>
        /// 添加操作帧, 等待帧时间同步发送
        /// </summary>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public virtual void AddOperation(string func, params object[] pars)
        {
            AddOperation(NetCmd.CallRpc, func, pars);
        }

        /// <summary>
        /// 添加操作帧, 等待帧时间同步发送
        /// </summary>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public virtual void AddOperation(ushort func, params object[] pars)
        {
            AddOperation(NetCmd.CallRpc, func, pars);
        }

        /// <summary>
        /// 添加操作帧, 等待帧时间同步发送
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public virtual void AddOperation(byte cmd, string func, params object[] pars)
        {
            var segment = BufferPool.Take();
            onSerializeRpc(segment, new RPCModel(cmd: 0, kernel: true, protocol: func.CRCU32(), pars: pars));
            var opt = new Operation(cmd, segment.ToArray(true));
            AddOperation(opt);
        }

        /// <summary>
        /// 添加操作帧, 等待帧时间同步发送
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="func"></param>
        /// <param name="pars"></param>
        [Obsolete("此方法尽量少用,此方法有可能产生较大的数据，不要频繁发送!", false)]
        public virtual void AddOperation(byte cmd, uint func, params object[] pars)
        {
            var segment = BufferPool.Take();
            onSerializeRpc(segment, new RPCModel(cmd: 0, kernel: true, protocol: func, pars: pars));
            var opt = new Operation(cmd, segment.ToArray(true));
            AddOperation(opt);
        }

        /// <summary>
        /// 添加操作帧, 等待帧时间同步发送
        /// </summary>
        /// <param name="opt"></param>
        public virtual void AddOperation(in Operation opt)
        {
            Locking.Enter();
            operations.Add(opt);
            Locking.Exit();
        }

        /// <summary>
        /// 添加操作帧, 等待帧时间同步发送
        /// </summary>
        /// <param name="opts"></param>
        public virtual void AddOperations(List<Operation> opts)
        {
            Locking.Enter();
            operations.AddRange(opts);
            Locking.Exit();
        }

        /// <summary>
        /// 添加操作帧, 等待帧时间同步发送
        /// </summary>
        /// <param name="opts"></param>
        public virtual void AddOperations(Operation[] opts)
        {
            Locking.Enter();
            operations.AddRange(opts);
            Locking.Exit();
        }

        /// <summary>
        /// 场景对象转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}:{Players.Count}/{sceneCapacity} opts:{operations.Count}";
        }

        /// <summary>
        /// 移除玩家
        /// </summary>
        /// <param name="client"></param>
        public void Remove(Player client)
        {
            Locking.Enter();
            try
            {
                if (client.SceneHash != hash)
                    return;
                OnBeginExit(client);
                Players.Remove(client);
                OnExit(client);
                client.OnExit();
                client.Scene = null;
                client.SceneName = string.Empty;
                client.SceneHash = -1;
            }
            finally
            {
                Locking.Exit();
            }
        }

        /// <summary>
        /// 移除所有玩家
        /// </summary>
        public void RemoveAll()
        {
            Locking.Enter();
            try
            {
                while (Players.Count > 0) //解决移除不全的问题
                    Remove(Players[0]);
                Players.Clear();
            }
            finally
            {
                Locking.Exit();
            }
        }

        /// <summary>
        /// 执行移除场景
        /// </summary>
        public void RemoveScene()
        {
            RemoveAll();
            OnRemove();
        }

        /// <summary>
        /// 移除场景所有玩家操作
        /// </summary>
        public void RemoveOperations()
        {
            Locking.Enter();
            operations.Clear();
            Locking.Exit();
        }

        ~NetScene()
        {
            onSerializeOpt = null;
        }
    }

    /// <summary>
    /// 默认网络场景，当不需要场景时直接继承
    /// </summary>
    public class DefaultScene : NetScene<NetPlayer>
    {
    }
}