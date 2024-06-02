using Net.Helper;
using Net.Share;
using System.Reflection;
using System.Collections.Generic;

internal static class RPCExtensions
{
    private static readonly Dictionary<uint, MethodInfo> FuncMap = new Dictionary<uint, MethodInfo>();

    static RPCExtensions()
    {
        var methods = AssemblyHelper.GetMethodAttributes(typeof(RPC), "mscorlib", "System", "Unity");
        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes(typeof(RPC), true);
            foreach (var attribute in attributes)
            {
                RPC rpc = (RPC)attribute;
                if (!string.IsNullOrEmpty(rpc.func))
                    FuncMap[rpc.func.CRCU32()] = method;
                else
                    FuncMap[method.Name.CRCU32()] = method;
                if (rpc.hash != 0)
                    FuncMap[rpc.hash] = method;
            }
        }
    }

    internal static string GetFunc(uint protocol)
    {
        if (FuncMap.TryGetValue(protocol, out var method))
            return method.ToString();
        return protocol.ToString();
    }
}