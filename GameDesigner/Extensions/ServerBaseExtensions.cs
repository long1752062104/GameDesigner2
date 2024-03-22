using Net.Helper;
using Net.Server;
using Net.Share;
using System;

namespace Net.Server 
{
    public abstract partial class ServerBase<Player> : IServerHandle<Player> where Player : NetPlayer, new() 
    {
        #region 不异步时内置Token
        public virtual void Response(Player client, uint protocol, bool serialize, params object[] pars)
            => Call(client, NetCmd.CallRpc, protocol, true, serialize, client.Token, null, pars);
        public virtual void Response(Player client, uint protocol, params object[] pars)
            => Call(client, NetCmd.CallRpc, protocol, true, false, client.Token, null, pars);
        public virtual void Response(Player client, byte cmd, uint protocol, params object[] pars)
            => Call(client, cmd, protocol, true, false, client.Token, null, pars);

        public virtual void Response(Player client, string func, bool serialize, params object[] pars)
            => Call(client, NetCmd.CallRpc, func.CRCU32(), true, serialize, client.Token, null, pars);
        public virtual void Response(Player client, string func, params object[] pars)
            => Call(client, NetCmd.CallRpc, func.CRCU32(), true, false, client.Token, null, pars);
        public virtual void Response(Player client, byte cmd, string func, params object[] pars)
            => Call(client, cmd, func.CRCU32(), true, false, client.Token, null, pars);
        #endregion

        #region 提供枚举协议类型
        public virtual void Call(Player client, Enum protocol, params object[] pars)
            => Call(client, NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, false, 0, null, pars);
        public virtual void Call(Player client, byte cmd, Enum protocol, params object[] pars)
            => Call(client, cmd, (uint)protocol.GetHashCode(), true, false, 0, null, pars);
        public virtual void Response(Player client, Enum protocol, bool serialize, uint token, params object[] pars)
            => Call(client, NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, serialize, token, null, pars);
        public virtual void Response(Player client, Enum protocol, uint token, params object[] pars)
            => Call(client, NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, false, token, null, pars);
        public virtual void Response(Player client, byte cmd, Enum protocol, uint token, params object[] pars)
            => Call(client, cmd, (uint)protocol.GetHashCode(), true, false, token, null, pars);

        public virtual void Response(Player client, Enum protocol, bool serialize, params object[] pars)
            => Call(client, NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, serialize, client.Token, null, pars);
        public virtual void Response(Player client, Enum protocol, params object[] pars)
            => Call(client, NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, false, client.Token, null, pars);
        public virtual void Response(Player client, byte cmd, Enum protocol, params object[] pars)
            => Call(client, cmd, (uint)protocol.GetHashCode(), true, false, client.Token, null, pars);
        #endregion
    }
}