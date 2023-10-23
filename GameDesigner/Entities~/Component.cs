using System;
using System.Collections.Generic;

namespace Net.Entities
{
    public class Component : Object
    {
        public Entity Entity { get; internal set; }

        public Component GetComponent(Type type)
        {
            return this.Entity.GetComponent(type);
        }

        public unsafe T GetComponent<T>() where T : Component
        {
            return this.Entity.GetComponent<T>();
        }

        public bool TryGetComponent(Type type, out Component component)
        {
            return this.Entity.TryGetComponent(type, out component);
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            return this.Entity.TryGetComponent<T>(out component);
        }

        public Component GetComponent(string type) 
        {
            return this.Entity.GetComponent(type);
        }

        public Component GetComponentInChildren(Type t, bool includeInactive)
        {
            return this.Entity.GetComponentInChildren(t, includeInactive);
        }

        public Component GetComponentInChildren(Type t)
        {
            return this.GetComponentInChildren(t, false);
        }

        public T GetComponentInChildren<T>([DefaultValue("false")] bool includeInactive)
        {
            return (T)((object)this.GetComponentInChildren(typeof(T), includeInactive));
        }

        public T GetComponentInChildren<T>()
        {
            return (T)((object)this.GetComponentInChildren(typeof(T), false));
        }

        public Component[] GetComponentsInChildren(Type t, bool includeInactive)
        {
            return this.Entity.GetComponentsInChildren(t, includeInactive);
        }

        public Component[] GetComponentsInChildren(Type t)
        {
            return this.Entity.GetComponentsInChildren(t, false);
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive)
        {
            return this.Entity.GetComponentsInChildren<T>(includeInactive);
        }

        public void GetComponentsInChildren<T>(bool includeInactive, List<T> result)
        {
            this.Entity.GetComponentsInChildren<T>(includeInactive, result);
        }

        public T[] GetComponentsInChildren<T>()
        {
            return this.GetComponentsInChildren<T>(false);
        }

        public void GetComponentsInChildren<T>(List<T> results)
        {
            this.GetComponentsInChildren<T>(false, results);
        }

        public Component GetComponentInParent(Type t, bool includeInactive)
        {
            return this.Entity.GetComponentInParent(t, includeInactive);
        }

        public Component GetComponentInParent(Type t)
        {
            return this.Entity.GetComponentInParent(t, false);
        }

        public T GetComponentInParent<T>([DefaultValue("false")] bool includeInactive)
        {
            return (T)((object)this.GetComponentInParent(typeof(T), includeInactive));
        }

        public T GetComponentInParent<T>()
        {
            return (T)((object)this.GetComponentInParent(typeof(T), false));
        }

        public Component[] GetComponentsInParent(Type t, [DefaultValue("false")] bool includeInactive)
        {
            return this.Entity.GetComponentsInParent(t, includeInactive);
        }

        public Component[] GetComponentsInParent(Type t)
        {
            return this.GetComponentsInParent(t, false);
        }

        public T[] GetComponentsInParent<T>(bool includeInactive)
        {
            return this.Entity.GetComponentsInParent<T>(includeInactive);
        }

        public void GetComponentsInParent<T>(bool includeInactive, List<T> results)
        {
            this.Entity.GetComponentsInParent<T>(includeInactive, results);
        }

        public T[] GetComponentsInParent<T>()
        {
            return this.GetComponentsInParent<T>(false);
        }

        public Component[] GetComponents(Type type)
        {
            return this.Entity.GetComponents(type);
        }

        public void GetComponents(Type type, List<Component> results)
        {
            this.Entity.GetComponents(type, results);
        }

        public void GetComponents<T>(List<T> results)
        {
            this.Entity.GetComponents(results);
        }

        public string tag
        {
            get
            {
                return this.Entity.tag;
            }
            set
            {
                this.Entity.tag = value;
            }
        }

        public T[] GetComponents<T>()
        {
            return this.Entity.GetComponents<T>();
        }

        public bool CompareTag(string tag)
        {
            return this.Entity.CompareTag(tag);
        }
    }
}