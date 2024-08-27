#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace AOIExample 
{
    using Net.Client;
    using Net.Component;
    using Net.Share;
    using Net.UnityComponent;
    using UnityEngine;

    public class SceneManager : NetworkSceneManager
    {
        public GameObject player;

        public override void OnConnected()
        {
            base.OnConnected();
            var player1 = Instantiate(player, new Vector3(Random.Range(-20, 20), 1, Random.Range(-20, 20)), Quaternion.identity);
            player1.AddComponent<PlayerControl>();
            player1.name = ClientBase.Instance.Identify.ToString();
            player1.GetComponent<NetworkObject>().Identity = ClientBase.Instance.UID;
            player1.GetComponent<AOIObject>().IsLocal = true;
            player1.GetComponent<PlayerControl>().moveSpeed = 20f;
            FindObjectOfType<ARPGcamera>().target = player1.transform;
        }
        public override void OnNetworkObjectCreate(in Operation opt, NetworkObject identity)
        {
            var rigidbody = identity.GetComponent<Rigidbody>();
            Destroy(rigidbody);
        }
        public override void OnPlayerExit(in Operation opt)
        {
            if (opt.identity == ClientBase.Instance.UID)//服务器延迟检测连接断开时,网络场景会将移除cmd插入同步队列, 当你再次进入如果uid是上次的uid, 则会发送下来,会删除刚生成的玩家对象
                return;
            base.OnPlayerExit(opt);
        }
        public override void OnOtherOperator(in Operation opt)
        {
            switch (opt.cmd)
            {
                case Command.EnterArea:
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
                    break;
                case Command.ExitArea:
                    {
                        if (identitys.TryGetValue(opt.identity, out var netObj))
                        {
                            netObj.gameObject.SetActive(false);
                        }
                    }
                    break;
                case Command.RobotUpdate:
                    {
                        if (identitys.TryGetValue(opt.identity, out var netObj))
                        {
                            netObj.transform.SetPositionAndRotation(opt.position, opt.rotation);
                        }
                    }
                    break;
            }
        }
    }
}
#endif