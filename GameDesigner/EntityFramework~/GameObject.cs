using Net.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Net.EntityFramework
{
    public sealed class GameObject : Object
    {
        public Transform transform { get; private set; }

        public int layer { get; set; }

        public  bool active { get; set; }

        public  bool activeSelf { get; }

        public  bool activeInHierarchy { get; }

        public  bool isStatic { get; set; }

        public string tag { get; set; }

        public List<Component> Components { get; set; } = new List<Component>();

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
            for (int i = 0; i < transform.childs.Count; i++)
            {
                var component = transform.gameObject.GetComponent(type);
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

        public static GameObject FindWithTag(string tag)
        {
            return GameObject.FindGameObjectWithTag(tag);
        }

        public Component AddComponent(Type componentType)
        {
            var component = Activator.CreateInstance(componentType) as Component;
            component.m_Name = m_Name;
            component.gameObject = this;
            component.transform = transform;
            Components.Add(component);
            return component;
        }

        public T AddComponent<T>() where T : Component
        {
            return this.AddComponent(typeof(T)) as T;
        }

        public bool CompareTag(string tag) 
        {
            throw new NotImplementedException();
        }

        public static GameObject FindGameObjectWithTag(string tag) 
        {
            throw new NotImplementedException();
        }

        public static GameObject[] FindGameObjectsWithTag(string tag) 
        {
            throw new NotImplementedException();
        }

        public GameObject() : this(null)
        {
        }

        public GameObject(string name) : this(name, default)
        {
        }

        public GameObject(string name, params Type[] components)
        {
            m_Name = new ObjectName();
            this.name = name;
            transform = new Transform();
            transform.m_Name = m_Name;
            transform.transform = transform;
            transform.gameObject = this;
            GameMain.Instance.CurrentScene.Roots.Add(this);
            if (components == null)
                return;
            foreach (Type componentType in components)
            {
                this.AddComponent(componentType);
            }
        }

        public static GameObject Find(string name) 
        {
            var roots = GameMain.Instance.CurrentScene.Roots;
            for (int i = 0; i < roots.Count; i++)
            {
                if (roots[i].gameObject.name == name)
                    return roots[i];
                for (int j = 0; j < roots[i].transform.childs.Count; j++)
                {
                    if (roots[i].transform.childs[j].gameObject.name == name)
                        return roots[i].transform.childs[j].gameObject;
                }
            }
            return default;
        }

        public Scene scene
        {
            get;
        }

        public GameObject gameObject
        {
            get
            {
                return this;
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
    }
}