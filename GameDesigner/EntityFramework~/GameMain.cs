using Net.Common;
using Net.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Net.EntityFramework
{
    public class GameMain : Singleton<GameMain>
    {
        public List<Scene> Scenes { get; set; }
        public int CurrActiveScene { get; set; }
        public Scene CurrentScene
        {
            get => Scenes[CurrActiveScene];
            set => Scenes[CurrActiveScene] = value;
        }

        public GameMain() 
        {
            if (instance == null)
                instance = this;
            Scenes = new List<Scene>();
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

        public void Run()
        {
            var scene = Scenes[CurrActiveScene];
            var nodes = scene.Roots;
            for (int i = 0; i < nodes.Count; i++)
            {
                var components = nodes[i].Components;
                for (int j = 0; j < components.Count; j++)
                {
                }
            }
        }
    }
}
