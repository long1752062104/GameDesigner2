using Net.Helper;
using Net.Share;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Net.Common
{
    /// <summary>
    /// 属性观察类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyObserver<T>
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
        public PropertyObserver(string name, bool available, Action<T> onValueChanged) { }
        
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

        public void SetValueWithoutNotify(T value) => SetValue(value, false);

        public override string ToString()
        {
            return $"{value}";
        }
    }

    /// <summary>
    /// 模糊属性观察类, 此类只支持byte, sbyte, short, ushort, char, int, uint, float, long, ulong, double
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObscuredPropertyObserver<T> : PropertyObserver<T>
    {
        protected string name;
        protected long valueAtk; 
        protected long valueAtk1;
        protected long valueAtkKey;

        public ObscuredPropertyObserver() { valueAtk = valueAtk1 = valueAtkKey = RandomHelper.Range(0, int.MaxValue); }
        public ObscuredPropertyObserver(T value) : this(null, value) { }
        public ObscuredPropertyObserver(string name, T value) : this(name, value, null) { }
        public ObscuredPropertyObserver(string name, T value, Action<T> onValueChanged)
        {
            this.name = name;
            Value = value;
            OnValueChanged = onValueChanged;
        }

        public override T GetValue()
        {
            var atkValue = valueAtk ^ valueAtkKey;
            var atkValue1 = valueAtk1 ^ atkValue;
            var value = Unsafe.As<long, T>(ref atkValue);
            if (atkValue1 != valueAtk)
            {
                AntiCheatHelper.OnDetected?.Invoke(name, value, this.value);
                return default;
            }
            return value;
        }

        public override void SetValue(T value, bool isNotify = true)
        {
            var value3 = Unsafe.As<T, long>(ref value);
            valueAtk = value3 ^ valueAtkKey;
            valueAtk1 = value3 ^ valueAtk;
            if (isNotify) OnValueChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// 属性观察自动类, 可模糊,不模糊
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyObserverAuto<T> : ObscuredPropertyObserver<T>
    {
        private readonly bool available;
        public PropertyObserverAuto() { }
        /// <summary>
        /// 属性观察自动类构造
        /// </summary>
        /// <param name="name">当属性被发现修改时提示名称</param>
        /// <param name="available">使用模糊属性?</param>
        /// <param name="onValueChanged">当属性被修改事件</param>
        public PropertyObserverAuto(string name, bool available, Action<T> onValueChanged)
        {
            if (!AntiCheatHelper.IsActive | !available)
                return;
            this.name = name;
            this.available = available;
            OnValueChanged = onValueChanged;
        }

        public override T GetValue()
        {
            if (available & AntiCheatHelper.IsActive) 
                return base.GetValue();
            return value;
        }

        public override void SetValue(T value, bool isNotify = true)
        {
            if (available & AntiCheatHelper.IsActive)
                base.SetValue(value, isNotify);
            else
                SetValue1(value, isNotify);
        }

        private void SetValue1(T value, bool isNotify = true)
        {
            if (Equals(this.value, value))
                return;
            this.value = value;
            if (isNotify) OnValueChanged?.Invoke(value);
        }
    }
}
