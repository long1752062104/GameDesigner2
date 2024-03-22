using Net.Helper;
using Net.Server;
using Net.Share;
using System;

namespace Net.Server 
{
    public partial class NetPlayer
    {
        #region 不异步时内置Token
        public virtual void Response(uint protocol, bool serialize, params object[] pars)
            => Call(NetCmd.CallRpc, protocol, true, serialize, Token, null, pars);
        public virtual void Response(uint protocol, params object[] pars)
            => Call(NetCmd.CallRpc, protocol, true, false, Token, null, pars);
        public virtual void Response(byte cmd, uint protocol, params object[] pars)
            => Call(cmd, protocol, true, false, Token, null, pars);

        public virtual void Response(string func, bool serialize, params object[] pars)
            => Call(NetCmd.CallRpc, func.CRCU32(), true, serialize, Token, null, pars);
        public virtual void Response(string func, params object[] pars)
            => Call(NetCmd.CallRpc, func.CRCU32(), true, false, Token, null, pars);
        public virtual void Response(byte cmd, string func, params object[] pars)
            => Call(cmd, func.CRCU32(), true, false, Token, null, pars);
        #endregion

        #region 提供枚举协议类型
        public virtual void Call(Enum protocol, params object[] pars)
            => Call(NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, false, 0, null, pars);
        public virtual void Call(byte cmd, Enum protocol, params object[] pars)
            => Call(cmd, (uint)protocol.GetHashCode(), true, false, 0, null, pars);
        public virtual void Response(Enum protocol, bool serialize, uint token, params object[] pars)
            => Call(NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, serialize, token, null, pars);
        public virtual void Response(Enum protocol, uint token, params object[] pars)
            => Call(NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, false, token, null, pars);
        public virtual void Response(byte cmd, Enum protocol, uint token, params object[] pars)
            => Call(cmd, (uint)protocol.GetHashCode(), true, false, token, null, pars);

        public virtual void Response(Enum protocol, bool serialize, params object[] pars)
            => Call(NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, serialize, Token, null, pars);
        public virtual void Response(Enum protocol, params object[] pars)
            => Call(NetCmd.CallRpc, (uint)protocol.GetHashCode(), true, false, Token, null, pars);
        public virtual void Response(byte cmd, Enum protocol, params object[] pars)
            => Call(cmd, (uint)protocol.GetHashCode(), true, false, Token, null, pars);
        #endregion
    }
}