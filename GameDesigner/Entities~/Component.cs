using System;
using System.Collections.Generic;

namespace Net.Entities
{
    [Serializable]
    public class Component : Object
    {
        public Entity Entity;
        public Transform transform => GetComponent<Transform>();

        public Component GetComponent(Type type)
        {
            return Entity.GetComponent(type);
        }

        public unsafe T GetComponent<T>() where T : Component
        {
            return Entity.GetComponent<T>();
        }

        public bool TryGetComponent(Type type, out Component component)
        {
            return Entity.TryGetComponent(type, out component);
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            return Entity.TryGetComponent<T>(out component);
        }

        public Component GetComponent(string type)
        {
            return Entity.GetComponent(type);
        }

        public Component GetComponentInChildren(Type t, bool includeInactive)
        {
            return Entity.GetComponentInChildren(t, includeInactive);
        }

        public Component GetComponentInChildren(Type t)
        {
            return GetComponentInChildren(t, false);
        }

        public T GetComponentInChildren<T>([DefaultValue("false")] bool includeInactive)
        {
            return (T)((object)GetComponentInChildren(typeof(T), includeInactive));
        }

        public T GetComponentInChildren<T>()
        {
            return (T)((object)GetComponentInChildren(typeof(T), false));
        }

        public Component[] GetComponentsInChildren(Type t, bool includeInactive)
        {
            return Entity.GetComponentsInChildren(t, includeInactive);
        }

        public Component[] GetComponentsInChildren(Type t)
        {
            return Entity.GetComponentsInChildren(t, false);
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive)
        {
            return Entity.GetComponentsInChildren<T>(includeInactive);
        }

        public void GetComponentsInChildren<T>(bool includeInactive, List<T> result)
        {
            Entity.GetComponentsInChildren<T>(includeInactive, result);
        }

        public T[] GetComponentsInChildren<T>()
        {
            return GetComponentsInChildren<T>(false);
        }

        public void GetComponentsInChildren<T>(List<T> results)
        {
            GetComponentsInChildren<T>(false, results);
        }

        public Component GetComponentInParent(Type t, bool includeInactive)
        {
            return Entity.GetComponentInParent(t, includeInactive);
        }

        public Component GetComponentInParent(Type t)
        {
            return Entity.GetComponentInParent(t, false);
        }

        public T GetComponentInParent<T>([DefaultValue("false")] bool includeInactive)
        {
            return (T)((object)GetComponentInParent(typeof(T), includeInactive));
        }

        public T GetComponentInParent<T>()
        {
            return (T)((object)GetComponentInParent(typeof(T), false));
        }

        public Component[] GetComponentsInParent(Type t, [DefaultValue("false")] bool includeInactive)
        {
            return Entity.GetComponentsInParent(t, includeInactive);
        }

        public Component[] GetComponentsInParent(Type t)
        {
            return GetComponentsInParent(t, false);
        }

        public T[] GetComponentsInParent<T>(bool includeInactive)
        {
            return Entity.GetComponentsInParent<T>(includeInactive);
        }

        public void GetComponentsInParent<T>(bool includeInactive, List<T> results)
        {
            Entity.GetComponentsInParent<T>(includeInactive, results);
        }

        public T[] GetComponentsInParent<T>()
        {
            return GetComponentsInParent<T>(false);
        }

        public Component[] GetComponents(Type type)
        {
            return Entity.GetComponents(type);
        }

        public void GetComponents(Type type, List<Component> results)
        {
            Entity.GetComponents(type, results);
        }

        public void GetComponents<T>(List<T> results)
        {
            Entity.GetComponents(results);
        }

        public string tag
        {
            get
            {
                return Entity.tag;
            }
            set
            {
                Entity.tag = value;
            }
        }

        public T[] GetComponents<T>()
        {
            return Entity.GetComponents<T>();
        }

        public bool CompareTag(string tag)
        {
            return Entity.CompareTag(tag);
        }

        public override string ToString()
        {
            return $"{Entity.Name} ({GetType()})";
        }
    }
}