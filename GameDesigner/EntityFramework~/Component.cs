using System;
using System.Collections.Generic;

namespace Net.EntityFramework
{
    public class Component : Object
    {
        public Transform transform { get; internal set; }

        public GameObject gameObject { get; internal set; }

        public Component GetComponent(Type type)
        {
            return this.gameObject.GetComponent(type);
        }

        public unsafe T GetComponent<T>() where T : Component
        {
            return this.gameObject.GetComponent<T>();
        }

        public bool TryGetComponent(Type type, out Component component)
        {
            return this.gameObject.TryGetComponent(type, out component);
        }

        
        public bool TryGetComponent<T>(out T component) where T : Component
        {
            return this.gameObject.TryGetComponent<T>(out component);
        }

        public Component GetComponent(string type) 
        {
            return this.gameObject.GetComponent(type);
        }

        
        public Component GetComponentInChildren(Type t, bool includeInactive)
        {
            return this.gameObject.GetComponentInChildren(t, includeInactive);
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
            return this.gameObject.GetComponentsInChildren(t, includeInactive);
        }

        
        public Component[] GetComponentsInChildren(Type t)
        {
            return this.gameObject.GetComponentsInChildren(t, false);
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive)
        {
            return this.gameObject.GetComponentsInChildren<T>(includeInactive);
        }

        public void GetComponentsInChildren<T>(bool includeInactive, List<T> result)
        {
            this.gameObject.GetComponentsInChildren<T>(includeInactive, result);
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
            return this.gameObject.GetComponentInParent(t, includeInactive);
        }

        
        public Component GetComponentInParent(Type t)
        {
            return this.gameObject.GetComponentInParent(t, false);
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
            return this.gameObject.GetComponentsInParent(t, includeInactive);
        }

        
        public Component[] GetComponentsInParent(Type t)
        {
            return this.GetComponentsInParent(t, false);
        }

        public T[] GetComponentsInParent<T>(bool includeInactive)
        {
            return this.gameObject.GetComponentsInParent<T>(includeInactive);
        }

        public void GetComponentsInParent<T>(bool includeInactive, List<T> results)
        {
            this.gameObject.GetComponentsInParent<T>(includeInactive, results);
        }

        public T[] GetComponentsInParent<T>()
        {
            return this.GetComponentsInParent<T>(false);
        }

        public Component[] GetComponents(Type type)
        {
            return this.gameObject.GetComponents(type);
        }

        public void GetComponents(Type type, List<Component> results)
        {
            this.gameObject.GetComponents(type, results);
        }

        public void GetComponents<T>(List<T> results)
        {
            this.gameObject.GetComponents(results);
        }

        public string tag
        {
            get
            {
                return this.gameObject.tag;
            }
            set
            {
                this.gameObject.tag = value;
            }
        }

        public T[] GetComponents<T>()
        {
            return this.gameObject.GetComponents<T>();
        }

        public bool CompareTag(string tag)
        {
            return this.gameObject.CompareTag(tag);
        }
    }
}