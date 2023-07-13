using Net.Server;
using System;

namespace Net.Share
{
    public class RpcWorkParameter
    {
        public NetPlayer client;
        public IRPCMethod method;
        public object[] pars;

        public RpcWorkParameter(NetPlayer client, IRPCMethod method, object[] pars)
        {
            this.client = client;
            this.method = method;
            this.pars = pars;
        }

        public void RpcWorkCallback(object state)
        {
            Invoke();
        }

        public void Invoke()
        {
            var len = pars.Length;
            var array = new object[len + 1];
            array[0] = client;
            Array.Copy(pars, 0, array, 1, len);
            method.Invoke(array);
        }
    }

    public class CallWorkParameter
    {
        public NetPlayer client;
        public IRPCMethod method;
        public object[] pars;
        public uint callId;

        public CallWorkParameter(NetPlayer client, IRPCMethod method, object[] pars, uint callId)
        {
            this.client = client;
            this.method = method;
            this.pars = pars;
            this.callId = callId;
        }

        public void RpcWorkCallback(object state)
        {
            Invoke();
        }

        public void Invoke()
        {
            var len = pars.Length;
            var array = new object[len + 2];
            array[0] = client;
            array[1] = callId;
            Array.Copy(pars, 0, array, 2, len);
            method.Invoke(array);
        }
    }
}
