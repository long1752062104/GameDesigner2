using Net.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Net.Entities
{
    public unsafe class EntityWorld
    {
        public List<Entity> EntityList { get; set; }
        public MyDictionary<Type, FastList<IComponent>> Components { get; set; }
        private MyDictionary<Type, IComponent> DefaultComponents { get; set; }

        public EntityWorld()
        {
            EntityList = new List<Entity>();
            Components = new MyDictionary<Type, FastList<IComponent>>();
            DefaultComponents = new MyDictionary<Type, IComponent>();
        }

        public Entity CreateEntity()
        {
            var entity = new Entity(this);
            return entity;
        }

        public Entity CreateEntity(params Type[] components)
        {
            var entity = new Entity(this);
            foreach (var component in components)
                entity.AddComponent(component);
            return entity;
        }

        public ref T GetEntityComponent<T>(Entity entity) where T : IComponent, new()
        {
            var componentType = typeof(T);
            if (Components.TryGetValue(componentType, out var components))
            {
                foreach (var componentId in entity.componentIds)
                {
                    if (components[componentId].Entity == entity)
                    {
                        T component1 = (T)components._items[componentId];
                        return ref component1;
                    }
                }
            }
            T component = TryGetDefaultComponent<T>();
            return ref component;
        }

        private T TryGetDefaultComponent<T>() where T : IComponent, new()
        {
            var componentType = typeof(T);
            if (!DefaultComponents.TryGetValue(componentType, out var component))
            {
                component = new T();
                DefaultComponents.Add(componentType, component);
            }
            return (T)component;
        }

        public ref IComponent AddEntityComponent(Entity entity, Type componentType)
        {
            if (!Components.TryGetValue(componentType, out var components))
                Components.Add(componentType, components = new FastList<IComponent>());
            var component = (IComponent)Activator.CreateInstance(componentType);
            component.Entity = entity;
            entity.componentIds.Add(components.Count);
            components.Add(ref component);
            return ref component;
        }
    }
}
