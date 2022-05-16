public static class SystemBaseExt
{
    public static string FormatCoin(this int self)
    {
        return FormatCoin((long)self);
    }

    public static string FormatCoin(this long self)
    {
        double value = self;
        if (value < 1000)
        {
            return value.ToString("f0");// + "$";
        }
        value /= 1000d;
        if (value < 1000)
        {
            var str = value.ToString("#.##");
            return str + "K";
        }
        value /= 1000d;
        if (value < 1000)
        {
            var str = value.ToString("#.##");
            return str + "B";
        }
        value /= 1000d;
        if (value < 1000)
        {
            var str = value.ToString("#.##");
            return str + "T";
        }
        value /= 1000d;
        if (value < 1000)
        {
            var str = value.ToString("#.##");
            return str + "q";
        }
        value /= 1000d;
        {
            var str = value.ToString("#.##");
            return str + "Q";
        }
    }
}
