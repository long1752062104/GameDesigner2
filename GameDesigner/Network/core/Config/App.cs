using Net.Share;
using System;
using System.Linq;
using System.Reflection;

namespace Net.Config
{
    /// <summary>
    /// gdnet应用程序入口
    /// </summary>
    public static class App
    {
        /// <summary>
        /// 初始化GDNet环境
        /// </summary>
        public static void Setup() => Init();

        /// <summary>
        /// 初始化GDNet环境
        /// </summary>
        public static void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblie in assemblies)
            {
                foreach (var type in assemblie.GetTypes().Where(t => !t.IsInterface))
                {
                    var members = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (var member in members)
                    {
                        var runtimeInitialize = member.GetCustomAttribute<RuntimeInitializeOnLoadMethod>();
                        if (runtimeInitialize == null)
                            continue;
                        member.Invoke(null, null);
                    }
                }
            }
        }
    }
}
