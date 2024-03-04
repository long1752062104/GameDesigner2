#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ServiceTools
{
    [MenuItem("GameDesigner/Example/Example1_Service", priority = 1)]
    static void Init() 
    {
        var files = Directory.GetFiles(Application.dataPath, "ExampleServer.exe", SearchOption.AllDirectories);
        if (files.Length == 0)
            return;
        var exe = files[0];
        Process p = new Process();
        p.StartInfo.FileName = exe;
        p.StartInfo.Arguments = "Example1";
        p.Start();
    }
    [MenuItem("GameDesigner/Example/Example2_Service", priority = 2)]
    static void Init1()
    {
        var files = Directory.GetFiles(Application.dataPath, "ExampleServer.exe", SearchOption.AllDirectories);
        if (files.Length == 0)
            return;
        var exe = files[0];
        Process p = new Process();
        p.StartInfo.FileName = exe;
        p.StartInfo.Arguments = "Example2";
        p.Start();
    }
    [MenuItem("GameDesigner/Example/LockStepService", priority = 3)]
    static void Init2()
    {
        var files = Directory.GetFiles(Application.dataPath, "ExampleServer.exe", SearchOption.AllDirectories);
        if (files.Length == 0)
            return;
        var exe = files[0];
        Process p = new Process();
        p.StartInfo.FileName = exe;
        p.StartInfo.Arguments = "Example3";
        p.Start();
    }
    [MenuItem("GameDesigner/Example/AOIService", priority = 4)]
    static void Init3()
    {
        var files = Directory.GetFiles(Application.dataPath, "ExampleServer.exe", SearchOption.AllDirectories);
        if (files.Length == 0)
            return;
        var exe = files[0];
        Process p = new Process();
        p.StartInfo.FileName = exe;
        p.StartInfo.Arguments = "Example4";
        p.Start();
    }
    [MenuItem("GameDesigner/Example/Server Source Project")]
    static void Init4()
    {
        var files = Directory.GetFiles(Application.dataPath, "ExampleServer.sln", SearchOption.AllDirectories);
        if (files.Length == 0)
            return;
        var exe = files[0];
        var p = new Process();
        p.StartInfo.FileName = exe;
        p.Start();
    }
    [MenuItem("GameDesigner/Example/Distributed Server Source Project")]
    static void Init5()
    {
        var files = Directory.GetFiles(Application.dataPath, "DistributedExampleServer.sln", SearchOption.AllDirectories);
        if (files.Length == 0)
            return;
        var exe = files[0];
        var p = new Process();
        p.StartInfo.FileName = exe;
        p.Start();
    }
}
#endif