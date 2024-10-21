using Net.Event;
using Net.System;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Net.Entities
{
    [Serializable]
    public class World
    {
        public string Name;
        public string Description;
        public FastList<Entity> Roots;
        private readonly TimerTick TimerTick;
        private readonly Stopwatch Stopwatch;

        public World()
        {
            Roots = new FastList<Entity>();
            TimerTick = new TimerTick();
            Stopwatch = Stopwatch.StartNew();
        }

        public World(string name) : this()
        {
            Name = name;
        }

        public void Init(params object[] args)
        {
            CollectAttributes();
            OnInit(args);
        }

        public virtual void OnInit(params object[] args) { }

        public void CollectAttributes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblie in assemblies)
            {
                Type[] types = null;
                try
                {
                    types = assemblie.GetTypes().Where(t => t.IsClass).ToArray();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (Exception loadEx in ex.LoaderExceptions)
                    {
                        NDebug.LogError("Type load error: " + loadEx.Message);
                    }
                }
                if (types == null)
                    continue;
                foreach (var type in types)
                {
                    var staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var staticMethod in staticMethods)
                    {
                        var attribute = staticMethod.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>();
                        if (attribute == null)
                            continue;
                        if (attribute.loadType == RuntimeInitializeLoadType.AfterSceneLoad)
                            staticMethod.Invoke(null, null);
                    }
                }
            }
        }

        public void Simulate(uint dt = 17, bool sleep = false)
        {
            var tick = (uint)Environment.TickCount;
            if (TimerTick.CheckTimeout(tick, dt, sleep))
                Execute();
        }

        private void Execute()
        {
            Stopwatch.Restart();
            Time.Update();
            OnUpdate();
            for (int i = 0; i < Roots.Count; i++)
            {
                Roots[i].Execute();
            }
            OnLateUpdate();
            Stopwatch.Stop();
        }

        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }

        public Entity Create(string name, params Type[] components)
        {
            return Create(name, null, components);
        }

        public Entity Create(string name, Entity parent, params Type[] components)
        {
            var entity = new Entity(name, components)
            {
                World = this,
                Parent = parent
            };
            parent?.Childs.Add(entity);
            return entity;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
