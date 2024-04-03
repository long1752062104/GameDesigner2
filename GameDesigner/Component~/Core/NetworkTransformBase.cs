﻿#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace Net.UnityComponent
{
    using Net.Client;
    using Net.Component;
    using Net.Share;
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
        protected Net.Vector3 position;
        protected Net.Quaternion rotation;
        protected Net.Vector3 localScale;
        protected Net.Vector3 netPosition;
        protected Net.Quaternion netRotation;
        protected Net.Vector3 netLocalScale;
        public SyncMode syncMode = SyncMode.Local;
        public bool syncPosition = true;
        public bool syncRotation = true;
        public bool syncScale = false;
        [HideInInspector] public SyncMode currMode = SyncMode.None;
        public float controlTime = 0.5f;
        public float lerpSpeed = 0.3f;
        public bool fixedSync = false;
        public float fixedSendTime = 1f;//固定发送时间
        internal float fixedTime;
        [HideInInspector] public float currControlTime;

        public override void Start()
        {
            base.Start();
            netObj.CanDestroy = false;
            netPosition = position = transform.position;
            netRotation = rotation = transform.rotation;
            netLocalScale = localScale = transform.localScale;
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
            if (transform.position != position | transform.rotation != rotation | transform.localScale != localScale | (Time.time > fixedTime & fixedSync))
                SyncTransformState();
        }

        public virtual void SyncTransformState()
        {
            position = transform.position; //必须在这里处理，在上面处理会有点问题
            rotation = transform.rotation;
            localScale = transform.localScale;
            fixedTime = Time.time + fixedSendTime;
            NetworkSceneManager.Instance.AddOperation(new Operation(Command.Transform, netObj.Identity, syncScale ? localScale : Net.Vector3.zero, syncPosition ? position : Net.Vector3.zero, syncRotation ? rotation : Net.Quaternion.zero)
            {
                cmd1 = (byte)currMode,
                index = netObj.registerObjectIndex,
                index1 = NetComponentID,
                uid = ClientBase.Instance.UID
            });
        }

        public virtual void ForcedSynchronous()
        {
            SyncTransformState();
        }

        public virtual void SyncTransform()
        {
            if (syncPosition)
                transform.position = Vector3.Lerp(transform.position, netPosition, lerpSpeed);
            if (syncRotation)
                if (netRotation != Net.Quaternion.identity)
                    transform.rotation = Quaternion.Lerp(transform.rotation, netRotation, lerpSpeed);
            if (syncScale)
                transform.localScale = netLocalScale;
        }

        public virtual void SyncControlTransform()
        {
            if (syncPosition)
            {
                position = netPosition;//位置要归位,要不然就会发送数据
                transform.position = netPosition;
            }
            if (syncRotation)
            {
                rotation = netRotation;
                transform.rotation = netRotation;
            }
            if (syncScale)
            {
                localScale = netLocalScale;
                transform.localScale = netLocalScale;
            }
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
            if (opt.cmd == Command.Transform)
                SetNetworkSyncMode(opt);
            netPosition = opt.position;
            netRotation = opt.rotation;
            netLocalScale = opt.direction;
            SyncControlTransform();
        }

        public override void OnInitialSynchronization(in Operation opt)
        {
            if (ClientBase.Instance.UID == opt.uid)
                return;
            SetNetworkSyncState(opt);
            SetNetworkSyncMode(opt);
            SyncControlTransform();
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
        }

        protected void SetNetworkSyncState(in Operation opt)
        {
            currControlTime = controlTime;
            netPosition = opt.position;
            netRotation = opt.rotation;
            netLocalScale = opt.direction;
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