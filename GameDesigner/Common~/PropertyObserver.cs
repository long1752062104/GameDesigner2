using Net.Helper;
using Net.Share;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Net.Common
{
    /// <summary>
    /// 属性观察接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPropertyObserver<T>
    {
        /// <summary>
        /// 属性值
        /// </summary>
        T Value { get; set; }
        /// <summary>
        /// 当属性被修改事件
        /// </summary>
        Action<T> OnValueChanged { get; set; }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <returns></returns>
        T GetValue();
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="value">新的属性值</param>
        /// <param name="isNotify">是否通知事件</param>
        void SetValue(T value, bool isNotify = true);
    }

    /// <summary>
    /// 属性观察类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyObserver<T> : IPropertyObserver<T>
    {
        protected T value;
        public T Value { get => GetValue(); set => SetValue(value); }
        public Action<T> OnValueChanged { get; set; }
        public PropertyObserver() { }
        public PropertyObserver(T value) : this(value, null) { }
        public PropertyObserver(T value, Action<T> onValueChanged)
        {
            this.value = value;
            OnValueChanged = onValueChanged;
        }
        
        public virtual T GetValue()
        {
            return value;
        }

        public virtual void SetValue(T value, bool isNotify = true)
        {
            if (Equals(this.value, value))
                return;
            this.value = value;
            if (isNotify) OnValueChanged?.Invoke(value);
        }

        public override string ToString()
        {
            return $"{value}";
        }
    }

    /// <summary>
    /// 模糊属性观察类, 此类只支持byte, sbyte, short, ushort, char, int, uint, float, long, ulong, double
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObscuredPropertyObserver<T> : IPropertyObserver<T>
    {
        protected PropertyDynamic<T> property;

        public T Value { get => GetValue(); set => SetValue(value); }
        public Action<T> OnValueChanged { get => property.OnValueChanged; set => property.OnValueChanged = value; }

        public ObscuredPropertyObserver(T value) : this(null, value) { }
        public ObscuredPropertyObserver(string name, T value) : this(name, value, null) { }
        public ObscuredPropertyObserver(string name, T value, Action<T> onValueChanged)
        {
            property = new PropertyDynamic<T>(name, value, onValueChanged);
        }

        public unsafe T GetValue()
        {
            property = property.Clone();
            return property.GetValue();
        }

        public unsafe void SetValue(T value, bool isNotify = true)
        {
            property = property.Clone();
            property.SetValue(value, isNotify);
        }
    }

    /// <summary>
    /// 属性观察自动类, 可模糊,不模糊
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyObserverAuto<T> : IPropertyObserver<T>
    {
        private readonly bool available;
        private IPropertyObserver<T> binding;
        public T Value { get => GetValue(); set => SetValue(value); }
        public Action<T> OnValueChanged { get => binding.OnValueChanged; set => binding.OnValueChanged = value; }

        public PropertyObserverAuto() { }
        /// <summary>
        /// 属性观察自动类构造
        /// </summary>
        /// <param name="name">当属性被发现修改时提示名称</param>
        /// <param name="available">使用模糊属性?</param>
        /// <param name="onValueChanged">当属性被修改事件</param>
        public PropertyObserverAuto(string name, bool available, Action<T> onValueChanged)
        {
            this.available = available;
            if (!AntiCheatHelper.IsActive | !available)
                binding = new PropertyObserver<T>(default, onValueChanged);
            else
                binding = new ObscuredPropertyObserver<T>(name, default, onValueChanged);
        }

        public T GetValue()
        {
            return binding.GetValue();
        }

        public void SetValue(T value, bool isNotify = true)
        {
            binding.SetValue(value, isNotify);
        }
    }
}
