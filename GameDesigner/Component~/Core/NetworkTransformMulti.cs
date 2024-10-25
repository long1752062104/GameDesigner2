#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using Net.Component;
using Net.Share;
using Net.Client;
using Net.Unity;
using UnityEngine;

namespace Net.UnityComponent
{
    /// <summary>
    /// 网络Transform同步组件基类
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class NetworkTransformMulti : NetworkTransformBase
    {
        public ChildTransform[] childs;

        public override void Start()
        {
            base.Start();
            InitChilds(); //实例化后再赋值位置时再次初始化位置用到
        }

        public override void OnNetworkInitialize(in Operation opt)
        {
            if (!IsLocal)
                InitChilds(); //实例化后就要初始化子物体信息, 否则会出现子物体的大小变成0,0,0的问题
            base.OnNetworkInitialize(opt); //先初始化子物体id再初始化网络同步状态
        }

        private void InitChilds()
        {
            for (int i = 0; i < childs.Length; i++)
                childs[i].Init(this, i + 1);
        }

        public override void ForcedSynchronous()
        {
            InitChilds(); //要先初始化，否则子id是-1导致错误
            base.ForcedSynchronous();
            for (int i = 0; i < childs.Length; i++)
                childs[i].SyncTransformState(netObj.Identity, currMode, netObj.registerObjectIndex, NetComponentID);
        }

        public override void NetworkUpdate(bool isNetworkTick)
        {
            if (netObj.Identity == -1 | currMode == SyncMode.None)
                return;
            if (currMode == SyncMode.Synchronized)
            {
                SyncTransform();
                for (int i = 0; i < childs.Length; i++)
                    childs[i].SyncTransform(lerpSpeed);
            }
            else if (currControlTime > 0f & (currMode == SyncMode.Control | currMode == SyncMode.SynchronizedAll))
            {
                currControlTime -= Time.deltaTime; //NetworkTime.I.CanSentTime;
                SyncTransform();
                for (int i = 0; i < childs.Length; i++)
                    childs[i].SyncTransform(lerpSpeed);
            }
            else if (isNetworkTick)
            {
                NetworkSyncCheck();
                for (int i = 0; i < childs.Length; i++)
                    childs[i].NetworkSyncCheck(netObj.Identity, currMode, netObj.registerObjectIndex, NetComponentID);
            }
        }

        public override void OnNetworkOperationHandler(in Operation opt)
        {
            if (ClientBase.Instance.UID == opt.uid)
                return;
            if (opt.index2 == 0)
            {
                SetNetworkSyncState(opt);
                if (currMode == SyncMode.SynchronizedAll | currMode == SyncMode.Control)
                    SyncControlTransform();
                else if (currMode == SyncMode.None)
                    SetNetworkSyncMode(opt);
            }
            else
            {
                currControlTime = controlTime;
                var child = childs[opt.index2 - 1];
                child.netPosition = opt.position;
                child.netRotation = opt.rotation;
                child.netLocalScale = opt.direction;
                if (currMode == SyncMode.SynchronizedAll | currMode == SyncMode.Control)
                    child.SyncControlTransform();
            }
            ReadCount++;
        }
    }

    [Serializable]
    public class ChildTransform
    {
        public string name;
        public Transform transform;
        internal Vector3 netPosition;
        internal Quaternion netRotation;
        internal Vector3 netLocalScale;
        public AxisConstraints syncPosition;
        public Axis2Constraints syncRotation;
        public AxisConstraints syncScale;
        public float difference = 9.9999994E-11f;
        public int childId = -1;//自身id
        private NetworkTransformMulti networkParent;

        internal void Init(NetworkTransformMulti networkParent, int childId)
        {
            this.networkParent = networkParent;
            this.childId = childId;
            if (transform == null) //偶尔有需求动态更换子tranfsorm，所以可能有空
                return;
            netPosition = transform.localPosition;
            netRotation = transform.localRotation;
            netLocalScale = transform.localScale;
        }

        internal void NetworkSyncCheck(int identity, SyncMode mode, int registerObjectIndex, int componentId)
        {
            if (transform == null) //偶尔有需求动态更换子tranfsorm，所以可能有空
                return;
            var canSyncPosition = (transform.localPosition - netPosition).sqrMagnitude > difference;
            var canSyncRotation = (transform.localRotation - netRotation).LengthSquared() > difference;
            var canSyncScale = (transform.localScale - netLocalScale).sqrMagnitude > difference;
            if (canSyncPosition | canSyncRotation | canSyncScale)
                SyncTransformState(identity, mode, registerObjectIndex, componentId);
        }

        public void SyncTransformState(int identity, SyncMode mode, int registerObjectIndex, int componentId)
        {
            if (transform == null) //偶尔有需求动态更换子tranfsorm，所以可能有空
                return;
            netPosition = transform.localPosition; //必须在这里处理，在上面处理会有点问题
            netRotation = transform.localRotation;
            netLocalScale = transform.localScale;

            Vector3 position = default;
            Quaternion rotation = default;
            Vector3 localScale = default;
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

            NetworkSceneManager.Instance.AddOperation(new Operation(Command.Transform, identity, position, rotation, localScale)
            {
                cmd1 = (byte)mode,
                uid = ClientBase.Instance.UID,
                index = registerObjectIndex,
                index1 = componentId,
                index2 = childId
            });

            networkParent.WriteCount++;
        }

        public void SyncTransform(float lerpSpeed)
        {
            if (transform == null) //偶尔有需求动态更换子tranfsorm，所以可能有空
                return;
            SyncAxisConstraints(false, transform.localPosition, transform.localRotation, transform.localScale);
            transform.SetLocalPositionAndRotation(Vector3.Lerp(transform.localPosition, netPosition, lerpSpeed), Quaternion.Lerp(transform.localRotation, netRotation, lerpSpeed));
            transform.localScale = netLocalScale;
        }

        public void SyncControlTransform()
        {
            if (transform == null) //偶尔有需求动态更换子tranfsorm，所以可能有空
                return;
            SyncAxisConstraints(false, transform.localPosition, transform.localRotation, transform.localScale);
            transform.SetLocalPositionAndRotation(netPosition, netRotation);
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
    }
}
#endif