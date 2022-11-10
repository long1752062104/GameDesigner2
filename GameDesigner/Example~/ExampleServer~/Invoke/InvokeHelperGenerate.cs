using Net.Helper;
using Net.Serialize;
using Net.Share;
using Net.System;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>此类必须在主项目程序集, 如在unity时必须是Assembly-CSharp程序集, 在控制台项目时必须在Main入口类的程序集</summary>
internal static class SyncVarGetSetHelperGenerate
{
    internal static void Init()
    {
        SyncVarGetSetHelper.Cache.Clear();

        SyncVarGetSetHelper.Cache.Add(typeof(Example1.Client), new Dictionary<string, SyncVarInfo>() {

            { "testint", new SyncVarInfoPtr<Example1.Client, System.Int32>(testint) },

            { "teststring", new SyncVarInfoPtr<Example1.Client, System.String>(teststring) },

        });

    }

    internal static void testint(this Example1.Client self, ref System.Int32 testint, ushort id, ref Segment segment, bool isWrite, Action<System.Int32, System.Int32> onValueChanged) 
    {
        if (isWrite)
        {
            if (Equals(self.testint, null))
                return;
            if (self.testint == testint)
                return;
            if (segment == null)
                segment = BufferPool.Take();
            segment.Write(id);
            var pos = segment.Position;
            segment.Write(self.testint);
            var end = segment.Position;
            segment.Position = pos;
            testint = segment.ReadInt32();
            segment.Position = end;
        }
        else 
        {
            var pos = segment.Position;
            var testint1 = segment.ReadInt32();
            var end = segment.Position;
            segment.Position = pos;
            testint = segment.ReadInt32();
            segment.Position = end;
            if (onValueChanged != null)
                onValueChanged(self.testint, testint1);
            self.testint = testint1;
        }
    }

    internal static void teststring(this Example1.Client self, ref System.String teststring, ushort id, ref Segment segment, bool isWrite, Action<System.String, System.String> onValueChanged) 
    {
        if (isWrite)
        {
            if (Equals(self.teststring, null))
                return;
            if (self.teststring == teststring)
                return;
            if (segment == null)
                segment = BufferPool.Take();
            segment.Write(id);
            var pos = segment.Position;
            segment.Write(self.teststring);
            var end = segment.Position;
            segment.Position = pos;
            teststring = segment.ReadString();
            segment.Position = end;
        }
        else 
        {
            var pos = segment.Position;
            var teststring1 = segment.ReadString();
            var end = segment.Position;
            segment.Position = pos;
            teststring = segment.ReadString();
            segment.Position = end;
            if (onValueChanged != null)
                onValueChanged(self.teststring, teststring1);
            self.teststring = teststring1;
        }
    }

}

/// <summary>定位辅助类路径</summary>
internal static class HelperFileInfo 
{
    internal static string GetPath()
    {
        return GetClassFileInfo();
    }

    internal static string GetClassFileInfo([CallerFilePath] string sourceFilePath = "")
    {
        return sourceFilePath;
    }
}