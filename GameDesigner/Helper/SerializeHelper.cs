using Net.Common;
using Net.Serialize;
using System;

namespace Net.Helper
{
    public static class SerializeHelper
    {
        public static byte[] ByteSerialize(this object self)
        {
            if (self == null)
                return null;
            return NetConvertBinary.SerializeObject(self).ToArray(true);
        }

        public static T ByteDeserialize<T>(this byte[] self, T defaultValue = default)
        {
            if (self == null)
                return defaultValue;
            if (self.Length == 0)
                return defaultValue;
            return NetConvertBinary.DeserializeObject<T>(self, 0, self.Length);
        }

        public static string JsonSerialize(this object self)
        {
            if (self == null)
                return null;
            return Newtonsoft_X.Json.JsonConvert.SerializeObject(self);
        }

        public static T JsonDeserialize<T>(this string self)
        {
            if (self == null)
                return default;
            return Newtonsoft_X.Json.JsonConvert.DeserializeObject<T>(self);
        }

        public static object JsonDeserialize(this string self, Type type)
        {
            if (self == null)
                return default;
            return Newtonsoft_X.Json.JsonConvert.DeserializeObject(self, type);
        }

        public static T GetProperty<T>(ref T target, string jsonText, Action<object> onChanged = null)
        {
            if (Equals(target, default(T)))
            {
                target = JsonDeserialize<T>(jsonText);
                if (target is IObservableProperty observable)
                    observable.OnChanged = onChanged;
            }
            return target;
        }

        public static string SetProperty<T>(ref T target, T value, Action<object> onChanged = null)
        {
            target = value;
            if (target is IObservableProperty observable)
            {
                observable.OnChanged = onChanged;
            }
            return JsonSerialize(value);
        }
    }
}