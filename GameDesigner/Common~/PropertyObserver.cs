using Net.Helper;
using Net.Share;
using System;
using System.Runtime.CompilerServices;

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
        private long valueAtk;
        private readonly long valueAtkKey;

        public ObscuredPropertyObserver() { valueAtk = valueAtkKey = RandomHelper.Range(0, int.MaxValue); }
        public ObscuredPropertyObserver(T value) : this(null, value) { }
        public ObscuredPropertyObserver(string name, T value) : this(name, value, null) { }
        public ObscuredPropertyObserver(string name, T value, Action<T> onValueChanged) : this()
        {
            this.name = name;
            Value = value;
            OnValueChanged = onValueChanged;
        }

        public override T GetValue()
        {
            var atkValue = valueAtk ^ valueAtkKey;
            var value = Unsafe.As<long, T>(ref atkValue);
            if (!Equals(value, this.value))
            {
                AntiCheatHelper.OnDetected?.Invoke(name, value, this.value);
                this.value = value;
            }
            return value;
        }

        public override void SetValue(T value, bool isNotify = true)
        {
            if (Equals(this.value, value))
                return;
            this.value = value;
            var value3 = Unsafe.As<T, long>(ref this.value);
            valueAtk = value3 ^ valueAtkKey;
            if (isNotify) OnValueChanged?.Invoke(value);
        }
    }

    public class PropertyObserverAuto<T> : ObscuredPropertyObserver<T>
    {
        private readonly bool available;
        public PropertyObserverAuto() { }
        public PropertyObserverAuto(string name, bool available, Action<T> onValueChanged)
        {
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
