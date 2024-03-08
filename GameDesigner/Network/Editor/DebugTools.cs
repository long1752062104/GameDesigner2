#if UNITY_EDITOR
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DebugTools : EditorWindow
{
    [MenuItem("GameDesigner/Network/DebugTools")]
    static void ShowWindow()
    {
        var window = GetWindow<DebugTools>("Debug工具");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(@"此工具可以帮你在控制台打印时, 直接显示Rpc调用到的方法, 可以双击进入Rpc方法位置, 使用此工具会修改
unity编辑器的UnityEditor.CoreModule.dll,修改IL指令让控制台判断打印内容也显示高亮, !!!注意:如果注入IL指令编译后,unity程序集错误导致unity打不开崩溃问题，
请打开Unity安装目录找到UnityEditor.CoreModule.dll.dll备份文件改成UnityEditor.CoreModule.dll后即可!", MessageType.Info);
        if (GUILayout.Button("注入编辑器UnityEditor.CoreModule.dll", GUILayout.Height(30)))
        {
            var modCtx = ModuleDef.CreateModuleContext();
            var path = typeof(EditorWindow).Assembly.Location;
            var dllBytes = File.ReadAllBytes(path);
            if (!File.Exists(path + ".dll"))
                File.WriteAllBytes(path + ".dll", dllBytes);
            var stream = new MemoryStream(dllBytes);
            var module = ModuleDefMD.Load(stream, modCtx);
            foreach (var type in module.Types)
            {
                if (type.FullName == "UnityEditor.ConsoleWindow")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "StacktraceWithHyperlinks")
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                var instruction = method.Body.Instructions[i];
                                if (instruction.OpCode.Code == Code.Ldarg_1)
                                {
                                    //源码部分
                                    //StringBuilder stringBuilder = new StringBuilder();
                                    //stringBuilder.Append(stacktraceText.Substring(0, callstackTextStart));
                                    //string[] array = stacktraceText.Substring(callstackTextStart).Split(new string[]
                                    //{
                                    //    "\n"
                                    //}, 0);

                                    //修改源码的callstackTextStart为0
                                    method.Body.Instructions[i] = OpCodes.Ldc_I4.ToInstruction(0);
                                }
                            }
                        }
                    }
                }
            }
            module.Write(path);
            module.Dispose();
            Debug.Log("注入完成!");
        }
        if (GUILayout.Button("恢复注入", GUILayout.Height(30)))
        {
            var path = typeof(EditorWindow).Assembly.Location;
            if (File.Exists(path + ".dll"))
            {
                var dllBytes = File.ReadAllBytes(path + ".dll");
                File.WriteAllBytes(path, dllBytes);
            }
            Debug.Log("恢复完成!");
        }
        if (GUILayout.Button("打开Unity安装路径", GUILayout.Height(30)))
        {
            var path = typeof(EditorWindow).Assembly.Location;
            Process.Start("explorer.exe", Path.GetDirectoryName(path));
        }
    }
}
#endif