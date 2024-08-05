#if UNITY_EDITOR
using dnlib.DotNet;
using Net.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class AssemblyItem
{
    public string path;

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (obj is AssemblyItem data)
            return data.path == path;
        return false;
    }
}

public class AssemblyListHandler
{
    public List<AssemblyItem> assemblys = new List<AssemblyItem>();
    private readonly ReorderableList assemblyList;

    public AssemblyListHandler(List<AssemblyItem> assemblys)
    {
        this.assemblys = assemblys;
        assemblyList = new ReorderableList(assemblys, typeof(AssemblyItem), true, false, false, false)
        {
            elementHeightCallback = OnElementHeightCallback,
            drawElementCallback = OnDrawElementCallback
        };
    }

    public void DoLayoutList()
    {
        assemblyList.DoLayoutList();
    }

    public virtual float OnElementHeightCallback(int index)
    {
        return 20f;
    }

    public virtual void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        try
        {
            var data = assemblys[index];
            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(new Rect(rect) { width = rect.width - 30f, height = 20f }, "程序集路径:", data.path);
            if (GUI.Button(new Rect(rect) { width = 20, height = 20f, x = rect.width - 3f }, "x"))
            {
                assemblys.RemoveAt(index);
                CodeObfuscationWindow.SaveData();
            }
            if (EditorGUI.EndChangeCheck())
            {
                CodeObfuscationWindow.SaveData();
            }
        }
        catch (Exception)
        {
        }
    }
}

public class CodeObfuscationData
{
    public List<AssemblyItem> assemblys = new List<AssemblyItem>();
    public List<string> monoMethods = new List<string>();
    public bool TypeName;
    public bool Fields;
    public bool Properties;
    public bool Events;
    public bool Methods;
}

public class CodeObfuscationWindow : EditorWindow
{
    private static CodeObfuscationData data;
    private Vector2 scrollPosition1;
    private Vector2 scrollPosition2;
    private AssemblyListHandler assemblyList;
    private ReorderableList monoMethodList;

    [MenuItem("GameDesigner/CodeObfuscation/Window")]
    static void ShowWindow()
    {
        var window = GetWindow<CodeObfuscationWindow>("代码混淆");
        window.Show();
    }

    private void OnEnable()
    {
        LoadData();
        if (data.assemblys.Count == 0)
        {
            var path = Application.dataPath + "/../Library/ScriptAssemblies/";
            var files = Directory.GetFiles(path, "*.dll");
            foreach (var file in files)
            {
                if (file.Contains("Editor"))
                    continue;
                //相对于Assets路径
                path = PathHelper.GetRelativePath(Application.dataPath, file);
                data.assemblys.Add(new AssemblyItem()
                {
                    path = Path.GetFullPath(path),
                });
            }
        }
        if (data.monoMethods.Count == 0)
            data.monoMethods = monoMethods.ToList();
        assemblyList = new AssemblyListHandler(data.assemblys);
        monoMethodList = new ReorderableList(data.monoMethods, typeof(string), true, false, true, true)
        {
            onAddCallback = (list) => SaveData(),
            onRemoveCallback = (list) => SaveData(),
        };
    }

    private void OnDisable()
    {
        SaveData();
    }

    internal static void LoadData()
    {
        data = PersistHelper.Deserialize<CodeObfuscationData>("code Obfuscation.json");
    }

    internal static void SaveData()
    {
        PersistHelper.Serialize(data, "code Obfuscation.json");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("混淆程序集:");
        if (GUILayout.Button("添加混淆程序集", GUILayout.Width(100)))
        {
            var dataPath = EditorUtility.OpenFilePanel("程序集路径", "", "*.dll");
            if (!string.IsNullOrEmpty(dataPath))
            {
                //相对于Assets路径
                var path = PathHelper.GetRelativePath(Application.dataPath, dataPath);
                var item = new AssemblyItem()
                {
                    path = Path.GetFullPath(path),
                };
                if (!data.assemblys.Contains(item))
                    data.assemblys.Add(item);
            }
            SaveData();
        }
        GUILayout.EndHorizontal();
        scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, false, true, GUILayout.MaxHeight(position.height / 2));
        assemblyList.DoLayoutList();
        GUILayout.EndScrollView();
        EditorGUILayout.LabelField("MonoBehaviour事件函数过滤:");
        scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, false, true, GUILayout.MaxHeight(position.height / 2));
        monoMethodList.DoLayoutList();
        GUILayout.EndScrollView();
        EditorGUILayout.LabelField("混淆设置:");
        EditorGUI.BeginChangeCheck();
        data.TypeName = EditorGUILayout.Toggle("类名:", data.TypeName);
        data.Fields = EditorGUILayout.Toggle("字段:", data.Fields);
        data.Properties = EditorGUILayout.Toggle("属性:", data.Properties);
        data.Events = EditorGUILayout.Toggle("事件:", data.Events);
        data.Methods = EditorGUILayout.Toggle("方法:", data.Methods);
        if (EditorGUI.EndChangeCheck())
            SaveData();
        GUI.color = Color.red;
        if (GUILayout.Button("执行混淆程序集", GUILayout.Height(50)))
        {
            foreach (var assembly in data.assemblys)
                Obscure(assembly.path);
            Debug.Log("混淆程序集完成,请使用dnSpy查看!");
        }
    }

    private void Obscure(string dllPath)
    {
        var moduleContext = new ModuleContext();
        var assemblyResolver = new AssemblyResolver(moduleContext);
        var resolver = new Resolver(assemblyResolver);
        moduleContext.AssemblyResolver = assemblyResolver;
        moduleContext.Resolver = resolver;
        assemblyResolver.DefaultModuleContext = moduleContext;
        var module = ModuleDefMD.Load(File.ReadAllBytes(dllPath), moduleContext);
        var dllPaths = AssemblyHelper.GetAssembliePaths();
        foreach (var item in dllPaths)
            assemblyResolver.PreSearchPaths.Add(item);
        var tick = DateTime.Now.Ticks;
        var types = module.GetTypes().ToArray();
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            if (type.Name == "<Module>")
                continue;
            if (type.CustomAttributes.Any(attr => attr.TypeFullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")) // 检查类是否具有[CompilerGenerated]特性
                continue;
            var isUnityEngineType = false;
            var baseType = type.GetBaseType();
            while (baseType != null)
            {
                if (baseType.FullName == "UnityEngine.MonoBehaviour")
                {
                    isUnityEngineType = true;
                    break;
                }
                baseType = baseType.GetBaseType();
            }
            if (data.TypeName /*&& !isUnityEngineType*/)
                type.Name = tick++.ToString();
            if (data.Fields)
            {
                foreach (var field in type.Fields)
                {
                    field.Name = tick++.ToString();
                }
            }
            if (data.Properties)
            {
                foreach (var property in type.Properties)
                {
                    property.Name = tick++.ToString();
                }
            }
            if (data.Events)
            {
                foreach (var @event in type.Events)
                {
                    @event.Name = tick++.ToString();
                }
            }
            if (data.Methods)
            {
                foreach (var method in type.Methods)
                {
                    if (method.IsConstructor) //构造函数混淆导致崩溃
                        continue;
                    if (method.IsVirtual) //重写虚方法不能混淆
                        continue;
                    if (isUnityEngineType)
                        if (monoMethods.Contains(method.Name))
                            continue;
                    method.Name = tick++.ToString();
                }
            }
        }
        module.Write(dllPath);
        module.Dispose();
    }

    public HashSet<string> monoMethods = new HashSet<string>()
    {
        "Awake", "OnEnable", "Start","FixedUpdate","Update","LateUpdate","OnDisable","OnDestroy","OnApplicationQuit","OnValidate","OnDrawGizmos",
        "OnDrawGizmosSelected","OnGUI","OnCollisionEnter","OnCollisionStay","OnCollisionExit","OnTriggerEnter","OnTriggerStay","OnTriggerExit","OnApplicationFocus","OnApplicationPause",
        "OnApplicationFocus","OnBecameInvisible","OnBecameVisible","OnTransformChildrenChanged","OnTransformParentChanged","OnTriggerEnter2D","OnTriggerStay2D","OnTriggerExit2D",
        "OnAnimatorIK","OnAnimatorMove","OnAudioFilterRead","OnJointBreak","OnParticleCollision","OnParticleTrigger","OnPostRender","OnPreCull","OnPreRender","OnRenderImage",
        "OnRenderObject","OnWillRenderObject","OnMouseDown","OnMouseUp","OnMouseEnter","OnMouseExit","OnMouseOver","OnServerInitialized","OnConnectedToServer","OnDisconnectedFromServer",
        "OnFailedToConnect","OnPlayerConnected","OnPlayerDisconnected","OnControllerColliderHit","OnJointBreak2D","OnPointerEnter","OnPointerExit","OnPointerDown","OnPointerUp","OnPointerClick",
        "OnBeginDrag","OnDrag","OnEndDrag","OnDrop","OnScroll","OnSelect","OnDeselect","OnUpdateSelected",
        "OnMove","OnSubmit",
    };
}
#endif