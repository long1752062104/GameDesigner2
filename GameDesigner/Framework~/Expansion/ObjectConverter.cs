public static class ObjectConverter
{
    public static int AsInt(this object self)
    {
        var str = self.ToString();
        int.TryParse(str, out var value);
        return value;
    }

    public static string AsString(this object self)
    {
        var str = self.ToString();
        return str;
    }

    public static bool AsBool(this object self)
    {
        var str = self.ToString();
        if (!bool.TryParse(str, out var value))
            return str != "0";
        return value;
    }
}
