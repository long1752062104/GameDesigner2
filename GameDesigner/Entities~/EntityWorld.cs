using Net.Event;
using Net.System;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Net.Entities
{
    public class EntityWorld
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public FastList<Entity> EntityRoots { get; set; }
        private TimerTick TimerTick { get; set; }
        private Stopwatch Stopwatch { get; set; }

        public EntityWorld()
        {
            EntityRoots = new FastList<Entity>();
            TimerTick = new TimerTick();
            Stopwatch = Stopwatch.StartNew();
        }
        public EntityWorld(string name) : this()
        {
            this.Name = name;
        }

        public void Init()
        {
            CollectAttributes();
        }

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
            for (int i = 0; i < EntityRoots.Count; i++)
            {
                EntityRoots[i].Execute();
            }
            Stopwatch.Stop();
            //NDebug.Log(Stopwatch.Elapsed);
        }

        public Entity CreateEntity(string name, params Type[] components)
        {
            var entity = new Entity(name, components);
            entity.World = this;
            entity.Parent = null;
            return entity;
        }

        public Entity CreateEntity(string name, Entity parent, params Type[] components)
        {
            var entity = new Entity(name, components);
            entity.World = this;
            entity.Parent = parent;
            return entity;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
