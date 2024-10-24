using System;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectConverter
{
    public static bool AsBool(object self)
    {
        return AsBool(self.ToString());
    }

    public static bool AsBool(string self)
    {
        if (!bool.TryParse(self, out var value))
            return self != "0" && !string.IsNullOrEmpty(self);
        return value;
    }

    public static byte AsByte(object self)
    {
        return AsByte(self.ToString());
    }

    public static byte AsByte(string self)
    {
        byte.TryParse(self, out var value);
        return value;
    }

    public static sbyte AsSbyte(object self)
    {
        return AsSbyte(self.ToString());
    }

    public static sbyte AsSbyte(string self)
    {
        sbyte.TryParse(self, out var value);
        return value;
    }

    public static short AsShort(object self)
    {
        return AsShort(self.ToString());
    }

    public static short AsShort(string self)
    {
        short.TryParse(self, out var value);
        return value;
    }

    public static ushort AsUshort(object self)
    {
        return AsUshort(self.ToString());
    }

    public static ushort AsUshort(string self)
    {
        ushort.TryParse(self, out var value);
        return value;
    }

    public static char AsChar(object self)
    {
        return AsChar(self.ToString());
    }

    public static char AsChar(string self)
    {
        char.TryParse(self, out var value);
        return value;
    }

    public static int AsInt(object self)
    {
        return AsInt(self.ToString());
    }

    public static int AsInt(string self)
    {
        int.TryParse(self, out var value);
        return value;
    }

    public static uint AsUint(object self)
    {
        return AsUint(self.ToString());
    }

    public static uint AsUint(string self)
    {
        uint.TryParse(self, out var value);
        return value;
    }

    public static float AsFloat(object self)
    {
        return AsFloat(self.ToString());
    }

    public static float AsFloat(string self)
    {
        float.TryParse(self, out var value);
        return value;
    }

    public static long AsLong(object self)
    {
        return AsLong(self.ToString());
    }

    public static long AsLong(string self)
    {
        long.TryParse(self, out var value);
        return value;
    }

    public static ulong AsUlong(object self)
    {
        return AsUlong(self.ToString());
    }

    public static ulong AsUlong(string self)
    {
        ulong.TryParse(self, out var value);
        return value;
    }

    public static double AsDouble(object self)
    {
        return AsDouble(self.ToString());
    }

    public static double AsDouble(string self)
    {
        double.TryParse(self, out var value);
        return value;
    }

    public static decimal AsDecimal(object self)
    {
        return AsDecimal(self.ToString());
    }

    public static decimal AsDecimal(string self)
    {
        decimal.TryParse(self, out var value);
        return value;
    }

    public static DateTime AsDateTime(object self)
    {
        return AsDateTime(self.ToString());
    }

    public static DateTime AsDateTime(string self)
    {
        DateTime.TryParse(self, out var value);
        return value;
    }

    public static string AsString(object self)
    {
        return self.ToString();
    }

    public static string AsString(string self)
    {
        return self;
    }

    public static T AsEnum<T>(object self) where T : struct
    {
        return AsEnum<T>(self.ToString());
    }

    public static T AsEnum<T>(string self) where T : struct
    {
        Enum.TryParse<T>(self, out var value);
        return value;
    }

    public static object AsEnum(object self, Type type)
    {
        return AsEnum(self.ToString(), type);
    }

    public static object AsEnum(string self, Type type)
    {
        try
        {
            var value = Enum.Parse(type, self);
            return value;
        }
        catch { }
        return default;
    }

    public static Vector2 AsVector2(object self)
    {
        return AsVector2(self.ToString());
    }

    public static Vector2 AsVector2(string self)
    {
        var datas = self.Split(',');
        if (datas.Length < 2)
            return default;
        return new Vector2(float.Parse(datas[0]), float.Parse(datas[1]));
    }

    public static Vector3 AsVector3(object self)
    {
        return AsVector3(self.ToString());
    }

    public static Vector3 AsVector3(string self)
    {
        var datas = self.Split(',');
        if (datas.Length < 3)
            return default;
        return new Vector3(float.Parse(datas[0]), float.Parse(datas[1]), float.Parse(datas[2]));
    }

    public static Vector4 AsVector4(object self)
    {
        return AsVector4(self.ToString());
    }

    public static Vector4 AsVector4(string self)
    {
        var datas = self.Split(',');
        if (datas.Length < 4)
            return default;
        return new Vector4(float.Parse(datas[0]), float.Parse(datas[1]), float.Parse(datas[2]), float.Parse(datas[3]));
    }

    public static Quaternion AsQuaternion(object self)
    {
        return AsQuaternion(self.ToString());
    }

    public static Quaternion AsQuaternion(string self)
    {
        var datas = self.Split(',');
        if (datas.Length < 4)
            return default;
        return new Quaternion(float.Parse(datas[0]), float.Parse(datas[1]), float.Parse(datas[2]), float.Parse(datas[3]));
    }

    public static Rect AsRect(object self)
    {
        return AsRect(self.ToString());
    }

    public static Rect AsRect(string self)
    {
        var datas = self.Split(',');
        if (datas.Length < 4)
            return default;
        return new Rect(float.Parse(datas[0]), float.Parse(datas[1]), float.Parse(datas[2]), float.Parse(datas[3]));
    }

    public static Color AsColor(object self)
    {
        return AsColor(self.ToString());
    }

    public static Color AsColor(string self)
    {
        var datas = self.Split(',');
        if (datas.Length < 4)
            return default;
        return new Color(float.Parse(datas[0]), float.Parse(datas[1]), float.Parse(datas[2]), float.Parse(datas[3]));
    }

    public static Color32 AsColor32(object self)
    {
        return AsColor32(self.ToString());
    }

    public static Color32 AsColor32(string self)
    {
        var datas = self.Split(',');
        if (datas.Length < 4)
            return default;
        return new Color32(byte.Parse(datas[0]), byte.Parse(datas[1]), byte.Parse(datas[2]), byte.Parse(datas[3]));
    }

    public static T[] AsArray<T>(object self)
    {
        return AsArray<T>(self.ToString());
    }

    public static T[] AsArray<T>(string self)
    {
        return Newtonsoft_X.Json.JsonConvert.DeserializeObject<T[]>(self);
    }

    public static List<T> AsList<T>(object self)
    {
        return AsList<T>(self.ToString());
    }

    public static List<T> AsList<T>(string self)
    {
        return Newtonsoft_X.Json.JsonConvert.DeserializeObject<List<T>>(self);
    }
}