using System.Text;
using Net.Event;
using System.Reflection;
using System.Collections.Generic;
#if CORE
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
#else
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;
#endif

namespace Net.Helper
{
    public class AssemblyHelper
    {
        private static readonly Dictionary<string, Type> TypeDict = new Dictionary<string, Type>();
        private static readonly HashSet<string> NotTypes = new HashSet<string>();

        /// <summary>
        /// 添加后续要查找的类型
        /// </summary>
        /// <param name="type"></param>
        public static void AddFindType(Type type)
        {
            TypeDict[type.ToString()] = type;
        }

        /// <summary>
        /// 获取类型， 如果类型已经获取过一次则直接取，否则或查找所有程序集获取类型，如果查找到则会添加到缓存字典中，下次不需要再遍历所有程序集
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            if (TypeDict.TryGetValue(typeName, out var type))
                return type;
            if (NotTypes.Contains(typeName))
                goto JMP;
            type = Type.GetType(typeName);
            if (type != null)
            {
                TypeDict.Add(typeName, type);
                return type;
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    TypeDict[typeName] = type;
                    return type;
                }
            }
            if (typeName.IndexOf("[") >= 0) //虽然支持了泛型和字典检测, 但是不要搞的太复杂花里胡哨的会影响性能和产生bug, 这里还有一个问题, 就是当编译il2cpp后可能也会获取不到
            {
                var index = typeName.IndexOf("[");
                var itemTypeName = typeName.Substring(0, index);
                var genericType = GetType(itemTypeName);
                if (genericType == null)
                    goto JMP;
                itemTypeName = typeName.Remove(0, index + 1);
                index = StringHelper.FindHitCount(itemTypeName, '[');
                StringHelper.RemoveHit(ref itemTypeName, ']', index);
                var itemTypeList = new List<Type>();
                if (genericType.GetGenericArguments().Length == 1)
                {
                    var itemType = GetType(itemTypeName);
                    if (itemType == null)
                        goto JMP;
                    itemTypeList.Add(itemType);
                }
                else
                {
                    var itemTypeNames = itemTypeName.Split(',');
                    foreach (var itemTypeNameN in itemTypeNames)
                    {
                        var itemType = GetType(itemTypeNameN);
                        if (itemType == null)
                            goto JMP;
                        itemTypeList.Add(itemType);
                    }
                }
                type = genericType.MakeGenericType(itemTypeList.ToArray());
                TypeDict[typeName] = type;
                return type;
            }
            NotTypes.Add(typeName);
        JMP: Event.NDebug.LogError($"找不到类型:{typeName}, 类型太复杂时需要使用 AssemblyHelper.AddFindType(type) 标记后面要查找的类");
            return null;
        }

        public static Type GetTypeNotOptimized(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// 获取代码形式的类型名称, 包括泛型,数组
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTypeName(Type type,
            string baseBegin = "", string baseEnd = "",
            string normalBegin = "", string normalEnd = "",
            string baseArrayBegin = "", string baseArrayEnd = "",
            string arrayBegin = "", string arrayEnd = "",
            string baseGenericBegin = "", string baseGenericEnd = "",
            string genericBegin = "", string genericEnd = ""
            )
        {
            string typeName;
            if (type.IsArray)
            {
                var type1 = type.GetArrayItemType();
                var typecode = Type.GetTypeCode(type1);
                if (typecode == TypeCode.Object)
                {
                    var typeName1 = GetTypeName(type1, baseBegin, baseEnd, normalBegin, normalEnd, baseArrayBegin, baseArrayEnd, arrayBegin, arrayEnd, baseGenericBegin, baseGenericEnd, genericBegin, genericEnd);
                    typeName = arrayBegin + $"{typeName1}[]" + arrayEnd;
                }
                else
                {
                    typeName = baseArrayBegin + $"{type1}[]" + baseArrayEnd;
                }
            }
            else if (type.IsGenericType)
            {
                typeName = type.ToString();
                var index = typeName.IndexOf("`");
                var count = typeName.IndexOf("[");
                typeName = typeName.Remove(index, count + 1 - index);
                typeName = typeName.Insert(index, "<");
                typeName = typeName.Substring(0, index + 1);
                var genericTypes = type.GenericTypeArguments;
                foreach (var item in genericTypes)
                {
                    var typecode = Type.GetTypeCode(item);
                    if (typecode == TypeCode.Object)
                    {
                        var typeName1 = GetTypeName(item, baseBegin, baseEnd, normalBegin, normalEnd, baseArrayBegin, baseArrayEnd, arrayBegin, arrayEnd, baseGenericBegin, baseGenericEnd, genericBegin, genericEnd);
                        typeName += genericBegin + $"{typeName1}" + genericEnd + ",";
                    }
                    else
                    {
                        var typeName1 = item.ToString();
                        typeName += baseGenericBegin + $"{typeName1}" + baseGenericEnd + ",";
                    }
                }
                typeName = typeName.TrimEnd(',') + ">";
            }
            else
            {
                typeName = type.ToString();
                var typecode = Type.GetTypeCode(type);
                if (typecode == TypeCode.Object)
                {
                    typeName = normalBegin + typeName.Replace("+", ".") + normalEnd;
                }
                else
                {
                    typeName = baseBegin + typeName.Replace("+", ".") + baseEnd;
                }
            }
            return typeName;
        }

        /// <summary>
        /// 获取泛型类型ToString成代码形式返回
        /// </summary>
        /// <param name="fullName">必须是泛型.ToString() 而不是泛型.FullName</param>
        /// <returns></returns>
        public static string GetCodeTypeName(string fullName)
        {
            var sb = new StringBuilder(fullName);
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '`')
                {
                    sb[i++] = '<';
                    for (int n = i; n < sb.Length; n++)
                    {
                        if (sb[n] != '[')
                            continue;
                        if (n + 1 >= sb.Length)
                            continue;
                        if (sb[n + 1] != ']')
                        {
                            sb.Remove(i, n - i + 1);
                            break;
                        }
                    }
                    for (int n = i; n < sb.Length; n++) //兼容dnlib库Type.FullName
                    {
                        if (sb[n] != '<')
                            continue;
                        sb.Remove(i, n - i + 1);
                        break;
                    }
                    for (int n = sb.Length - 1; n >= 0; n--)
                    {
                        if (sb[n] != ']')
                            continue;
                        if (n - 1 < 0)
                            continue;
                        if (sb[n - 1] != '[')
                        {
                            sb[n] = '>';
                            break;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public static Assembly GetRunAssembly(string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name == assemblyName)
                    return assembly;
            }
            return null;
        }

        public static List<Type> GetInterfaceTypes(Type interfaceType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var interfaceTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(t => t.IsInterfaceType(interfaceType)).ToList();
                if (types == null)
                    continue;
                if (types.Count > 0)
                    interfaceTypes.AddRange(types);
            }
            return interfaceTypes;
        }

        public static List<object> GetInterfaceInstances(Type interfaceType)
        {
            var types = GetInterfaceTypes(interfaceType);
            var objs = new List<object>();
            foreach (var type in types)
            {
                var obj = Activator.CreateInstance(type);
                objs.Add(obj);
            }
            return objs;
        }

        public static List<T> GetInterfaceInstances<T>()
        {
            var types = GetInterfaceTypes(typeof(T));
            var objs = new List<T>();
            foreach (var type in types)
            {
                var obj = (T)Activator.CreateInstance(type);
                objs.Add(obj);
            }
            return objs;
        }

        public static Assembly DynamicBuild(Dictionary<string, string> codes)
        {
            var includeDllPaths = new HashSet<string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblie in assemblies)
            {
                if (assemblie.IsDynamic)
                    continue;
                var path = assemblie.Location;
                var name = Path.GetFileName(path);
                //if (name.Contains("Editor"))
                //    continue;
                if (name.Contains("mscorlib"))
                    continue;
                if (!File.Exists(path))
                    continue;
                //if (path.Contains("PackageCache"))
                //    continue;
                includeDllPaths.Add(path);
            }
            return DynamicBuild(codes, includeDllPaths);
        }

        public static Assembly DynamicBuild(Dictionary<string, string> codes, HashSet<string> includeDllPaths)
        {
            Assembly assembly = null;
#if !CORE
            var provider = new CSharpCodeProvider();
            var param = new CompilerParameters();
            param.ReferencedAssemblies.AddRange(includeDllPaths.ToArray());
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;
            param.CompilerOptions = "/optimize+ /platform:x64 /target:library /unsafe /langversion:default";
            var codeFiles = new List<string>();
            foreach (var code in codes)
            {
                var tempFile = $"{Path.GetTempPath()}{code.Key}";
                File.WriteAllText(tempFile, code.Value);
                codeFiles.Add(tempFile);
            }
            var cr = provider.CompileAssemblyFromFile(param, codeFiles.ToArray());
            if (cr.Errors.HasErrors)
            {
                NDebug.LogError("编译错误：");
                foreach (CompilerError err in cr.Errors)
                    NDebug.LogError(err.ErrorText);
                return assembly;
            }
#else
            var metadataReferences = new List<MetadataReference>();
            foreach (var dllPath in includeDllPaths)
                metadataReferences.Add(MetadataReference.CreateFromFile(dllPath));
            var references = metadataReferences.ToArray();
            var assemblyName = Path.GetRandomFileName();
            var syntaxTrees = new List<SyntaxTree>();
            foreach (var code in codes)
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(code.Value));
            var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, allowUnsafe: true));
            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                    foreach (Diagnostic diagnostic in failures)
                        NDebug.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    return assembly;
                }
                else
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    assembly = Assembly.Load(stream.ToArray());
                }
            }
#endif
            NDebug.Log("动态编译完成!");
            return assembly;
        }

        /// <summary>
        /// 热更新加载, 可以在运行时植入dll执行一段代码
        /// </summary>
        /// <param name="dllPath">dll文件路径</param>
        /// <param name="entryTypeName">入口类名称</param>
        /// <param name="invokeMethodName">调用方法名称, 必须是静态的</param>
        public static void HotfixLoad(string dllPath, string entryTypeName, string invokeMethodName)
        {
            var bytes = File.ReadAllBytes(dllPath);
#if CORE
            var context = new AssemblyLoadContext("Hotfix", true);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var assembly = context.LoadFromStream(stream);
                var entryType = assembly.GetType(entryTypeName);
                if (entryType != null)
                {
                    var method = entryType.GetMethod(invokeMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    method?.Invoke(null, null);
                }
            }
            context.Unload();
#else
            var assembly = Assembly.Load(bytes);
            var entryType = assembly.GetType(entryTypeName);
            if (entryType != null)
            {
                var method = entryType.GetMethod(invokeMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                method?.Invoke(null, null);
            }
            //这里无法卸载程序集, 可能会导致加载的程序集越来越多, 内存可能会涨点, 性能影响应该不大
#endif
        }
    }
}