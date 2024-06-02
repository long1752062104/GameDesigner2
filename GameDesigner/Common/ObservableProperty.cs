using System;
using System.Collections.Generic;

namespace Net.Common
{
    public interface IObservableProperty
    {
        Action<object> OnChanged { get; set; }
    }

    public interface IObservableProperty<T> : IObservableProperty
    {
    }

    public class ObservableArray<T> : IObservableProperty<T>
    {
        public T[] array; //序列化用

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action<object> OnChanged { get; set; }

        public int Length => array.Length;

        public ObservableArray() : this(0) { }

        public ObservableArray(int size)
        {
            array = new T[size];
        }

        public T this[int index]
        {
            get
            {
                return array[index];
            }
            set
            {
                array[index] = value;
                OnChanged?.Invoke(this);
            }
        }
    }

    public class ObservableList<T> : IObservableProperty<T>
    {
        public List<T> list; //序列化用

        [Net.Serialize.NonSerialized]
        [Newtonsoft_X.Json.JsonIgnore]
        public Action<object> OnChanged { get; set; }

        public int Count => list.Count;

        public ObservableList() : this(0) { }

        public ObservableList(int size)
        {
            list = new List<T>(size);
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
                OnChanged?.Invoke(this);
            }
        }

        public void Add(T item)
        {
            list.Add(item);
            OnChanged?.Invoke(this);
        }

        public void Clear()
        {
            list.Clear();
            OnChanged?.Invoke(this);
        }
    }
}
