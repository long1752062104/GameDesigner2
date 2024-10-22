#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace Net.UnityComponent
{
    using Net.Client;
    using Net.Component;
    using Net.Share;
    using Net.Unity;
    using UnityEngine;

    public enum SyncMode
    {
        /// <summary>
        /// 自身同步, 只有自身才能控制, 同步给其他客户端, 其他客户端无法控制这个物体的移动
        /// </summary>
        Local,
        /// <summary>
        /// 完全控制, 所有客户端都可以移动这个物体, 并且其他客户端都会被同步
        /// 同步条件是哪个先移动这个物体会有<see cref="NetworkTransformBase.controlTime"/>秒完全控制,
        /// 其他客户端无法控制,如果先移动的客户端一直移动这个物体,则其他客户端无法移动,只有先移动的客户端停止操作,下个客户端才能同步这个物体
        /// </summary>
        Control,
        /// <summary>
        /// 无效
        /// </summary>
        Authorize,
        /// <summary>
        /// 自身同步在其他客户端显示的状态
        /// </summary>
        Synchronized,
        /// <summary>
        /// 完全控制在其他客户端显示的状态
        /// </summary>
        SynchronizedAll,
        /// <summary>
        /// 空同步
        /// </summary>
        None,
    }

    /// <summary>
    /// 网络Transform同步组件基类
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public abstract class NetworkTransformBase : NetworkBehaviour
    {
        protected Net.Vector3 netPosition;
        protected Net.Quaternion netRotation;
        protected Net.Vector3 netLocalScale;
        public SyncMode syncMode = SyncMode.Local;
        public AxisConstraints syncPosition;
        public Axis2Constraints syncRotation;
        public AxisConstraints syncScale;
        [HideInInspector] public SyncMode currMode = SyncMode.None;
        public float controlTime = 0.5f;
        public float lerpSpeed = 0.3f;
        [HideInInspector] public float currControlTime;
        [Tooltip("允许位置微小变动的范围, 如果当前位置移动超过difference距离,才会发生同步")]
        public float difference = 9.9999994E-11f;

        public override void Start()
        {
            base.Start();
            netObj.CanDestroy = false;
            netPosition = transform.position;
            netRotation = transform.rotation;
            netLocalScale = transform.localScale;
        }

        public override void NetworkUpdate()
        {
            if (netObj.Identity == -1 | currMode == SyncMode.None)
                return;
            if (currMode == SyncMode.Synchronized)
            {
                SyncTransform();
            }
            else if (currControlTime > 0f & (currMode == SyncMode.Control | currMode == SyncMode.SynchronizedAll))
            {
                currControlTime -= NetworkTime.I.CanSentTime;
                SyncTransform();
            }
            else
            {
                NetworkSyncCheck();
            }
        }

        public virtual void NetworkSyncCheck()
        {
            SyncAxisConstraints(false, transform.position, transform.rotation, transform.localScale); //禁言某个轴后值是0,导致一直同步
            var canSyncPosition = (transform.position - netPosition).sqrMagnitude > difference;
            var canSyncRotation = (transform.rotation - netRotation).LengthSquared() > difference;
            var canSyncScale = (transform.localScale - netLocalScale).sqrMagnitude > difference;
            if (canSyncPosition | canSyncRotation | canSyncScale)
                SyncTransformState();
        }

        public virtual void SyncTransformState()
        {
            netPosition = transform.position; //必须在这里处理，在上面处理会有点问题
            netRotation = transform.rotation;
            netLocalScale = transform.localScale;

            Net.Vector3 position = default;
            Net.Quaternion rotation = default;
            Net.Vector3 localScale = default;
            if (syncPosition.X)
                position.x = netPosition.x;
            if (syncPosition.Y)
                position.y = netPosition.y;
            if (syncPosition.Z)
                position.z = netPosition.z;

            if (syncRotation.X)
                rotation.x = netRotation.x;
            if (syncRotation.Y)
                rotation.y = netRotation.y;
            if (syncRotation.Z)
                rotation.z = netRotation.z;
            if (syncRotation.W)
                rotation.w = netRotation.w;

            if (syncScale.X)
                localScale.x = netLocalScale.x;
            if (syncScale.Y)
                localScale.y = netLocalScale.y;
            if (syncScale.Z)
                localScale.z = netLocalScale.z;

            NetworkSceneManager.Instance.AddOperation(new Operation(Command.Transform, netObj.Identity, position, rotation, localScale)
            {
                cmd1 = (byte)currMode,
                index = netObj.registerObjectIndex,
                index1 = NetComponentID,
                uid = ClientBase.Instance.UID
            });

            WriteCount++;
        }

        public virtual void ForcedSynchronous()
        {
            SyncTransformState();
        }

        public virtual void SyncTransform()
        {
            SyncAxisConstraints(false, transform.position, transform.rotation, transform.localScale);
            var canSyncPosition = (transform.position - netPosition).sqrMagnitude > difference;
            var canSyncRotation = (transform.rotation - netRotation).LengthSquared() > difference;
            var canSyncScale = (transform.localScale - netLocalScale).sqrMagnitude > difference;
            if (canSyncPosition)
                transform.position = Vector3.Lerp(transform.position, netPosition, lerpSpeed);
            if (canSyncRotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, netRotation, lerpSpeed);
            if (canSyncScale)
                transform.localScale = netLocalScale;
        }

        public virtual void SyncControlTransform()
        {
            SyncAxisConstraints(false, transform.position, transform.rotation, transform.localScale);
            var canSyncPosition = (transform.position - netPosition).sqrMagnitude > difference;
            var canSyncRotation = (transform.rotation - netRotation).LengthSquared() > difference;
            var canSyncScale = (transform.localScale - netLocalScale).sqrMagnitude > difference;
            if (canSyncPosition)
                transform.position = netPosition;
            if (canSyncRotation)
                transform.rotation = netRotation;
            if (canSyncScale)
                transform.localScale = netLocalScale;
        }

        public virtual void SyncAxisConstraints(bool state, in Net.Vector3 position, in Net.Quaternion rotation, in Net.Vector3 localScale)
        {
            if (syncPosition.X == state)
                netPosition.x = position.x;
            if (syncPosition.Y == state)
                netPosition.y = position.y;
            if (syncPosition.Z == state)
                netPosition.z = position.z;

            if (syncRotation.X == state)
                netRotation.x = rotation.x;
            if (syncRotation.Y == state)
                netRotation.y = rotation.y;
            if (syncRotation.Z == state)
                netRotation.z = rotation.z;
            if (syncRotation.W == state)
                netRotation.w = rotation.w;

            if (syncScale.X == state)
                netLocalScale.x = localScale.x;
            if (syncScale.Y == state)
                netLocalScale.y = localScale.y;
            if (syncScale.Z == state)
                netLocalScale.z = localScale.z;
        }

        public override void OnNetworkObjectInit(int identity)
        {
            currMode = syncMode;
            if (syncMode != SyncMode.Control && syncMode != SyncMode.SynchronizedAll) //如果是公共网络物体则不能初始化的时候通知，因为可能前面有玩家了，如果发起同步会导致前面的玩家被拉回原来位置
                ForcedSynchronous(); //发起一次同步，让对方显示物体
            else
                currControlTime = controlTime; //如果是公共网络物体则开始时先处于被控制状态，这样就不会发送同步数据
        }

        public override void OnNetworkObjectCreate(in Operation opt)
        {
            if (opt.cmd == Command.Transform) //同步Transform命令时才能设置位置,否则会出现位置在0,0,0的问题, 然后看到瞬移
            {
                SetNetworkSyncMode(opt);
                SyncAxisConstraints(true, opt.position, opt.rotation, opt.direction);
                SyncControlTransform();
            }
        }

        public override void OnInitialSynchronization(in Operation opt)
        {
            if (ClientBase.Instance.UID == opt.uid)
                return;
            //当一个物体挂有多个网络组件时, 比如NetworAnimator先发生了同步数据, 这里就会导致位置变成0,0,0 所以我们判断当不是NetworkTransform的同步数据, 则不能修改位置和旋转,缩放
            if (opt.cmd == Command.Transform)
            {
                SetNetworkSyncMode(opt);
                SetNetworkSyncState(opt);
                SyncControlTransform();
            }
        }

        public override void OnNetworkOperationHandler(in Operation opt)
        {
            if (ClientBase.Instance.UID == opt.uid)
                return;
            SetNetworkSyncState(opt);
            if (currMode == SyncMode.SynchronizedAll | currMode == SyncMode.Control)
                SyncControlTransform();
            else if (currMode == SyncMode.None)
                SetNetworkSyncMode(opt);
            ReadCount++;
        }

        protected void SetNetworkSyncState(in Operation opt)
        {
            currControlTime = controlTime;
            SyncAxisConstraints(true, opt.position, opt.rotation, opt.direction);
        }

        protected void SetNetworkSyncMode(in Operation opt)
        {
            var mode1 = (SyncMode)opt.cmd1;
            if (mode1 == SyncMode.Control | mode1 == SyncMode.SynchronizedAll)
                currMode = SyncMode.SynchronizedAll;
            else
                currMode = SyncMode.Synchronized;
        }

        public void SetNetworkPosition(Net.Vector3 position)
        {
            netPosition = position;
        }

        public void SetNetworkRotation(Net.Quaternion rotation)
        {
            netRotation = rotation;
        }

        public void SetNetworkPositionAndRotation(Net.Vector3 position, Net.Quaternion rotation)
        {
            netPosition = position;
            netRotation = rotation;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (ClientBase.Instance == null)
                return;
            if (!ClientBase.Instance.Connected)
                return;
            if (netObj.Identity == -1)
                return;
            //如果在退出游戏或者退出场景后不让物体被销毁，则需要查找netobj组件设置Identity等于-1，或者查找此组件设置currMode等于None，或者在点击处理的时候调用ClientBase.Instance.Close方法
            if (currMode == SyncMode.SynchronizedAll | currMode == SyncMode.Control | currMode == SyncMode.Local)
                netObj.SendDestroyCommand();
        }
    }
}
#endif