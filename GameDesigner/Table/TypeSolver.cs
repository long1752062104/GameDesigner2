using System;
using System.Collections.Generic;
using System.Linq;
using Net.Event;
using Net.Helper;
using Net.Table.Solvers;

namespace Net.Table
{
    public class TypeSolver
    {
        public static Dictionary<string, ITypeSolver> SolverTypes = new();

        public static bool TryGetValue(string columnType, out ITypeSolver typeSolver)
        {
            if (SolverTypes.TryGetValue(columnType, out typeSolver))
                return true;
            var type = AssemblyHelper.GetTypeNotOptimized(columnType);
            if (type != null && type.IsEnum)
            {
                typeSolver = Activator.CreateInstance(typeof(EnumTypeSolver<>).MakeGenericType(type)) as ITypeSolver;
                SolverTypes.Add(columnType, typeSolver);
                return true;
            }
            NDebug.LogError($"表类型:{columnType}没有类型求解器, 请实现!");
            return false;
        }

        static TypeSolver()
        {
            if (SolverTypes.Count > 0)
                return;
            InitTypeSolverCollectors();
        }

        public static void InitTypeSolverCollectors()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var collectors = assemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => typeof(ITypeSolverCollector).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
            foreach (var collector in collectors)
            {
                var collector1 = (ITypeSolverCollector)Activator.CreateInstance(collector);
                collector1.Initialize();
            }
        }
    }
}
