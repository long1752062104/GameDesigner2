using Net.Helper;
using Net.Share;
using System;

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
            this.OnValueChanged = onValueChanged;
        }

        public virtual T GetValue()
        {
            return this.value;
        }

        public virtual void SetValue(T value, bool isNotify = true)
        {
            if (Object.Equals(this.value, value))
                return;
            this.value = value;
            if (isNotify) OnValueChanged?.Invoke(value);
        }

        public void SetValueWithoutNotify(T value) => SetValue(value, false);
    }

    /// <summary>
    /// 模糊属性观察类, 此类只支持byte, sbyte, short, ushort, char, int, uint, float, long, ulong, double
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObscuredPropertyObserver<T> : PropertyObserver<T> where T : struct
    {
        private string name;
        private long valueAtk, valueAtkKey, valueCrc;

        public ObscuredPropertyObserver() { }
        public ObscuredPropertyObserver(T value) : this(null, value) { }
        public ObscuredPropertyObserver(string name, T value) : this(name, value, null) { }
        public ObscuredPropertyObserver(string name, T value, Action<T> onValueChanged)
        {
            this.name = name;
            Value = value;
            this.OnValueChanged = onValueChanged;
        }

        public override unsafe T GetValue()
        {
            var atkValue = this.valueAtk ^ this.valueAtkKey;
            var crcValue = this.valueCrc ^ this.valueAtkKey;
            var value = *(T*)&atkValue;
            if (!Object.Equals(value, this.value) | !Object.Equals(crcValue, this.valueAtk))
            {
                AntiCheatHelper.OnDetected?.Invoke(name);
                this.value = value;
            }
            return this.value;
        }

        public override unsafe void SetValue(T value, bool isNotify = true)
        {
            if (Object.Equals(this.value, value))
                return;
            this.value = value;
            this.valueAtkKey = RandomHelper.Range(0, int.MaxValue);
            this.valueAtk = *(long*)&value ^ this.valueAtkKey;
            this.valueCrc = this.valueAtk ^ this.valueAtkKey;
            if (isNotify) OnValueChanged?.Invoke(value);
        }
    }
}
