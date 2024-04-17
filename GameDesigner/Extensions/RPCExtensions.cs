using Net.Helper;
using Net.Share;
using System.Reflection;
using System.Collections.Generic;

internal static class RPCExtensions
{
    private static readonly Dictionary<uint, MethodInfo> FuncMap = new Dictionary<uint, MethodInfo>();

    static RPCExtensions()
    {
        var methods = AssemblyHelper.GetMethodAttributes(typeof(Rpc));
        foreach (var method in methods)
            FuncMap[method.Name.CRCU32()] = method;
    }

    internal static string GetFunc(uint protocol)
    {
        if (FuncMap.TryGetValue(protocol, out var method))
            return method.ToString();
        return protocol.ToString();
    }
}