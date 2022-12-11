using System;

public static class ObjectConverter
{
    public static bool AsBool(this object self)
    {
        var str = self.ToString();
        if (!bool.TryParse(str, out var value))
            return str != "0";
        return value;
    }

    public static byte AsByte(this object self)
    {
        var str = self.ToString();
        byte.TryParse(str, out var value);
        return value;
    }

    public static sbyte AsSbyte(this object self)
    {
        var str = self.ToString();
        sbyte.TryParse(str, out var value);
        return value;
    }

    public static short AsShort(this object self)
    {
        var str = self.ToString();
        short.TryParse(str, out var value);
        return value;
    }

    public static ushort AsUshort(this object self)
    {
        var str = self.ToString();
        ushort.TryParse(str, out var value);
        return value;
    }

    public static char AsChar(this object self)
    {
        var str = self.ToString();
        char.TryParse(str, out var value);
        return value;
    }

    public static int AsInt(this object self)
    {
        var str = self.ToString();
        int.TryParse(str, out var value);
        return value;
    }

    public static uint AsUint(this object self)
    {
        var str = self.ToString();
        uint.TryParse(str, out var value);
        return value;
    }

    public static float AsFloat(this object self)
    {
        var str = self.ToString();
        float.TryParse(str, out var value);
        return value;
    }

    public static long AsLong(this object self)
    {
        var str = self.ToString();
        long.TryParse(str, out var value);
        return value;
    }

    public static ulong AsUlong(this object self)
    {
        var str = self.ToString();
        ulong.TryParse(str, out var value);
        return value;
    }

    public static double AsDouble(this object self)
    {
        var str = self.ToString();
        double.TryParse(str, out var value);
        return value;
    }

    public static decimal AsDecimal(this object self)
    {
        var str = self.ToString();
        decimal.TryParse(str, out var value);
        return value;
    }

    public static DateTime AsDateTime(this object self)
    {
        var str = self.ToString();
        DateTime.TryParse(str, out var value);
        return value;
    }

    public static string AsString(this object self)
    {
        var str = self.ToString();
        return str;
    }
}
