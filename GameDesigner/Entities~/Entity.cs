using Net.Helper;
using System;
using System.Collections.Generic;

namespace Net.Entities
{
    [Serializable]
    public sealed class Entity : Object
    {
        public int layer;
        public bool active;
        public bool activeSelf;
        public bool activeInHierarchy;
        public bool isStatic;
        public string tag;
        public List<Component> Components = new();
        public List<Entity> Childs = new();
        public Queue<Action> EventQueue { get; private set; } = new Queue<Action>();

        public void SetActive(bool value)
        {
            throw new NotImplementedException();
        }

        public void SetActiveRecursively(bool state)
        {
            throw new NotImplementedException();
        }

        public unsafe T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is T component)
                    return component;
            }
            return default;
        }

        public Component GetComponent(Type type)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].GetType() == type)
                    return Components[i];
            }
            return default;
        }

        public Component GetComponent(string type)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].GetType().Name == type)
                    return Components[i];
            }
            return default;
        }

        public Component GetComponentInChildren(Type type, bool includeInactive)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                var component = Childs[i].GetComponent(type);
                if (component != null)
                    return component;
                component = Childs[i].GetComponentInChildren(type, includeInactive);
                if (component != null)
                    return component;
            }
            return default;
        }

        public Component GetComponentInChildren(Type type)
        {
            return this.GetComponentInChildren(type, false);
        }

        public T GetComponentInChildren<T>()
        {
            bool includeInactive = false;
            return this.GetComponentInChildren<T>(includeInactive);
        }

        public T GetComponentInChildren<T>([DefaultValue("false")] bool includeInactive)
        {
            return (T)((object)this.GetComponentInChildren(typeof(T), includeInactive));
        }

        public Component GetComponentInParent(Type type, bool includeInactive)
        {
            throw new NotImplementedException();
        }

        public Component GetComponentInParent(Type type)
        {
            return this.GetComponentInParent(type, false);
        }

        public T GetComponentInParent<T>()
        {
            bool includeInactive = false;
            return this.GetComponentInParent<T>(includeInactive);
        }

        public T GetComponentInParent<T>([DefaultValue("false")] bool includeInactive)
        {
            return (T)((object)this.GetComponentInParent(typeof(T), includeInactive));
        }

        public Component[] GetComponents(Type type)
        {
            throw new NotImplementedException();
        }

        public T[] GetComponents<T>()
        {
            throw new NotImplementedException();
        }

        public void GetComponents(Type type, List<Component> results)
        {
            throw new NotImplementedException();
        }

        public void GetComponents<T>(List<T> results)
        {
            throw new NotImplementedException();
        }

        public Component[] GetComponentsInChildren(Type type)
        {
            bool includeInactive = false;
            return this.GetComponentsInChildren(type, includeInactive);
        }

        public Component[] GetComponentsInChildren(Type type, [DefaultValue("false")] bool includeInactive)
        {
            throw new NotImplementedException();
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive)
        {
            throw new NotImplementedException();
        }

        public void GetComponentsInChildren<T>(bool includeInactive, List<T> results)
        {
            throw new NotImplementedException();
        }

        public T[] GetComponentsInChildren<T>()
        {
            return this.GetComponentsInChildren<T>(false);
        }

        public void GetComponentsInChildren<T>(List<T> results)
        {
            this.GetComponentsInChildren<T>(false, results);
        }

        public Component[] GetComponentsInParent(Type type)
        {
            bool includeInactive = false;
            return this.GetComponentsInParent(type, includeInactive);
        }

        public Component[] GetComponentsInParent(Type type, [DefaultValue("false")] bool includeInactive)
        {
            throw new NotImplementedException();
        }

        public void GetComponentsInParent<T>(bool includeInactive, List<T> results)
        {
            throw new NotImplementedException();
        }

        public T[] GetComponentsInParent<T>(bool includeInactive)
        {
            throw new NotImplementedException();
        }

        public T[] GetComponentsInParent<T>()
        {
            return this.GetComponentsInParent<T>(false);
        }

        public unsafe bool TryGetComponent<T>(out T component) where T : Component
        {
            component = GetComponent<T>();
            return component != null;
        }

        public bool TryGetComponent(Type type, out Component component)
        {
            component = GetComponent(type);
            return component != null;
        }

        public static Entity FindWithTag(string tag)
        {
            return Entity.FindGameObjectWithTag(tag);
        }

        public Component AddComponent(Type componentType)
        {
            var component = Activator.CreateInstance(componentType) as Component;
            component.Entity = this;
            Components.Add(component);
            if (component is IEntityAwake entityAwake)
                entityAwake.Awake();
            if (component is IEntityStart entityStart)
                EventQueue.Enqueue(entityStart.Start); //Start在下一帧才会执行
            return component;
        }

        public T AddComponent<T>() where T : Component
        {
            return AddComponent(typeof(T)) as T;
        }

        public bool CompareTag(string tag)
        {
            throw new NotImplementedException();
        }

        public static Entity FindGameObjectWithTag(string tag)
        {
            throw new NotImplementedException();
        }

        public static Entity[] FindGameObjectsWithTag(string tag)
        {
            throw new NotImplementedException();
        }

        private Entity() : this(null)
        {
        }

        internal Entity(string name) : this(name, default)
        {
        }

        internal Entity(string name, params Type[] components)
        {
            Name = name;
            if (components == null)
                return;
            foreach (Type componentType in components)
            {
                AddComponent(componentType);
            }
        }

        public Entity Find(string name)
        {
            if (Name == name)
                return this;
            var layerIndex = name.IndexOf('/');
            var layerName = name;
            if (layerIndex >= 0)
                layerName = name.Substring(0, layerIndex);
            Entity entity = null;
            for (int i = 0; i < Childs.Count; i++)
            {
                if (Childs[i].Name == layerName)
                {
                    entity = Childs[i];
                    break;
                }
            }
            if (entity == null)
                return null;
            if (layerIndex >= 0)
            {
                name = name.Remove(0, layerIndex + 1);
                return entity.Find(name);
            }
            return entity;
        }

        public World scene
        {
            get => World;
        }

        public Entity gameObject
        {
            get
            {
                return this;
            }
        }

        public World World { get; internal set; }

        private Entity m_parent;
        public Entity Parent
        {
            get => m_parent;
            set
            {
                if (m_parent != null)
                    m_parent.Childs.Remove(this);
                m_parent = value;
                if (m_parent == null)
                    World.Roots.Add(this);
                else
                    m_parent.Childs.Add(this);
            }
        }

        public Component AddComponent(string className)
        {
            var type = AssemblyHelper.GetTypeNotOptimized(className);
            if (type == null)
                return null;
            var component = AddComponent(type);
            Components.Add(component);
            return component;
        }

        internal void Execute()
        {
            while (EventQueue.Count > 0)
            {
                EventQueue.Dequeue().Invoke();
            }
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is IEntityUpdate entity)
                    entity.Update();
            }
            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i].Execute();
            }
        }

        public Entity GetChild(int index)
        {
            return Childs[index];
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}