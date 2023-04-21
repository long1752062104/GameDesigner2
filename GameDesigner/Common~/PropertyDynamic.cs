using Net.Helper;
using Net.Share;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Net.Common
{
    /// <summary>
    /// 模糊属性观察类, 此类只支持byte, sbyte, short, ushort, char, int, uint, float, long, ulong, double
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyDynamic<T>
    {
        private string name;
        private long valueAtk;
        private long valueAtkKey;
        private byte crcValue;
        public Action<T> OnValueChanged;

        private PropertyDynamic() { }

        public PropertyDynamic(string name, T value, Action<T> onValueChanged)
        {
            this.name = name;
            valueAtkKey = RandomHelper.Range(0, int.MaxValue);
            SetValue(value);
            OnValueChanged = onValueChanged;
        }

        public unsafe T GetValue()
        {
            var value = valueAtk ^ valueAtkKey;
            var ptr = (byte*)&value;
            var crcIndex = (byte)(valueAtk % 247);
            crcValue = Net.Helper.CRCHelper.CRC8(ptr, 0, 8, crcIndex);
            if (this.crcValue != crcValue)
            {
                AntiCheatHelper.OnDetected?.Invoke(name, value, value);
                return default;
            }
            var value1 = Unsafe.As<long, T>(ref value);
            return value1;
        }

        public unsafe void SetValue(T value, bool isNotify = true)
        {
            var value1 = Unsafe.As<T, long>(ref value);
            valueAtk = value1 ^ valueAtkKey;
            var ptr = (byte*)&value1;
            var crcIndex = (byte)(valueAtk % 247);
            crcValue = Net.Helper.CRCHelper.CRC8(ptr, 0, 8, crcIndex);
            if (isNotify) OnValueChanged?.Invoke(value);
        }

        public PropertyDynamic<T> Clone()
        {
            var property = new PropertyDynamic<T>();
            property.name = name;
            property.valueAtk = valueAtk;
            property.valueAtkKey = valueAtkKey;
            property.crcValue = crcValue;
            property.OnValueChanged = OnValueChanged;
            return property;
        }
    }
}