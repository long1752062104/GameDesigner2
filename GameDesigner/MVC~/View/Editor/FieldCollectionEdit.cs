#if (UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL) && UNITY_EDITOR
namespace MVC.View
{
    using Net.Helper;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;

    public class FieldCollectionWindow : EditorWindow
    {
        private FieldCollectionEdit field;
        
        internal static void Init(FieldCollectionEdit field)
        {
            var win = GetWindow<FieldCollectionWindow>("字段收集器", true);
            win.field = field;
        }

        void OnGUI() 
        {
            if (field == null)
                return;
            GUILayout.Label("将组件拖到此窗口上! 如果是赋值模式, 拖入的对象将不会显示选择组件!");
            field.data.changeField = GUILayout.Toggle(field.data.changeField, "赋值变量");
            field.data.addField = GUILayout.Toggle(field.data.addField, "直接添加变量");
            field.data.seleAddField = GUILayout.Toggle(field.data.seleAddField, "选择添加变量组件");
            if ((Event.current.type == EventType.DragUpdated | Event.current.type == EventType.DragPerform) & !field.data.changeField)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;//拖动时显示辅助图标
                if (Event.current.type == EventType.DragPerform)
                {
                    if (field.data.addField)
                    {
                        var componentPriority = new List<Type>()
                        {
                            typeof(Button), typeof(Toggle), typeof(Text), typeof(Slider), typeof(Scrollbar), typeof(Dropdown),
                            typeof(ScrollRect), typeof(InputField), typeof(Image)
                        };
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            var go = obj as GameObject;
                            var objects = new List<Object>() { obj };
                            objects.AddRange(go.GetComponents<Component>());
                            foreach (var cp in componentPriority)
                            {
                                var components = objects.Where(item => item.GetType() == cp).ToList();
                                if (components.Count != 0)
                                {
                                    field.fieldName = obj.name.Replace(" ", "").Replace("(", "_").Replace(")", "_");
                                    field.selectObject = components[0];
                                    field.AddField(components[0].GetType().ToString());
                                    goto J;
                                }
                            }
                            field.fieldName = obj.name.Replace(" ", "").Replace("(", "_").Replace(")", "_");
                            field.selectObject = objects[objects.Count - 1];
                            field.AddField(objects[objects.Count - 1].GetType().ToString());
                        J:;
                        }
                        return;
                    }
                    else if (field.data.seleAddField)
                    {
                        var dict = new Dictionary<Type, List<Object[]>>();
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            var go = obj as GameObject;
                            var objects = new List<Object>() { obj };
                            objects.AddRange(go.GetComponents<Component>());
                            foreach (var obj1 in objects)
                            {
                                var type = obj1.GetType();
                                if (!dict.TryGetValue(type, out var objects1))
                                    dict.Add(type, objects1 = new List<Object[]>());
                                objects1.Add(new Object[] { obj, obj1 });
                            }
                        }
                        var menu = new GenericMenu();
                        foreach (var item in dict)
                        {
                            var typeName = item.Key.ToString();
                            menu.AddItem(new GUIContent(typeName), false, () =>
                            {
                                foreach (var item1 in item.Value)
                                {
                                    field.fieldName = item1[0].name.Replace(" ", "").Replace("(", "_").Replace(")", "_");
                                    field.selectObject = item1[1];
                                    field.AddField(typeName);
                                }
                            });
                        }
                        menu.ShowAsContext();
                        Event.current.Use();
                        return;
                    }
                    else
                    {
                        field.search1 = "";
                        field.search = DragAndDrop.objectReferences[0].GetType().Name.ToLower();
                    }
                }
            }
            field.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(FieldCollection))]
    public class FieldCollectionEdit : Editor
    {
        private FieldCollection field;
        private bool selectType;
        internal string search = "", search1 = "", fieldName = "";
        private string[] types = new string[0];
        private DateTime searchTime;
        private string selectTypeName;
        internal Object selectObject;
        internal JsonSave data = new JsonSave();
        private Vector2 scrollPosition;

        public class JsonSave 
        {
            public string nameSpace;
            public string savePath;
            public string csprojFile;
            public bool fullPath;
            public string savePathExt;
            internal string nameSpace1;
            public bool changeField;
            public bool addField;
            public bool seleAddField;
            public bool genericType = true;
            public string inheritType = "Net.Component.SingleCase";
            public string addInheritType;
            public List<string> inheritTypes = new List<string>() { "Net.Component.SingleCase", "UnityEngine.MonoBehaviour" };
        }

        private void OnEnable()
        {
            field = target as FieldCollection;
            var objects = Resources.FindObjectsOfTypeAll<Object>();
            HashSet<string> types1 = new HashSet<string>();
            foreach (var obj in objects)
            {
                var str = obj.GetType().FullName;
                if (!types1.Contains(str))
                    types1.Add(str);
            }
            var types2 = typeof(Vector2).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Object))).ToArray();
            foreach (var obj in types2)
            {
                var str = obj.FullName;
                if (!types1.Contains(str))
                    types1.Add(str);
            }
            types = types1.ToArray();
            LoadData();
            if (string.IsNullOrEmpty(data.savePath))
                data.savePath = Application.dataPath;
        }

        private void OnDisable()
        {
            SaveData();
        }

        void LoadData() 
        {
            data = PersistHelper.Deserialize<JsonSave>("fcdata.txt");
        }

        void SaveData() 
        {
            PersistHelper.Serialize(data, "fcdata.txt");
        }

        internal void AddField(string typeName) 
        {
            selectType = true;
            var name = fieldName;
            if (name == "")
                name = "name" + field.nameIndex++;
            foreach (var f in field.fields)
            {
                if (f.name == fieldName)
                {
                    name += field.nameIndex++;
                    break;
                }
            }
            var field1 = new FieldCollection.Field() { name = name, typeName = typeName };
            field.fields.Add(field1);
            if (selectObject != null)
                field1.target = selectObject;
            selectTypeName = typeName;
            field1.Update();
            EditorUtility.SetDirty(field);
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开收集器界面")) 
                FieldCollectionWindow.Init(this);
            field.fieldName = EditorGUILayout.TextField("收集器名称", field.fieldName);
            var rect2 = EditorGUILayout.GetControlRect();
            fieldName = EditorGUI.TextField(rect2, "字段名称", fieldName);
            if (GUI.Button(new Rect(rect2.x + 100, rect2.y, 20, rect2.height), "+"))
            {
                if (string.IsNullOrEmpty(selectTypeName)) 
                {
                    Debug.Log("请先选择一次字段类型!");
                    return;
                }
                var name = fieldName;
                if (name == "")
                    name = "name" + field.nameIndex++;
                foreach (var f in field.fields)
                {
                    if (f.name == fieldName)
                    {
                        name += field.nameIndex++;
                        break;
                    }
                }
                field.fields.Add(new FieldCollection.Field() { name = name, typeName = selectTypeName });
                EditorUtility.SetDirty(field);
                return;
            }
            search = EditorGUILayout.TextField("字段类型", search);
            if (search != search1)
            {
                selectType = false;
                search1 = search;
                searchTime = DateTime.Now.AddMilliseconds(20);
            }
            if (DateTime.Now > searchTime & !selectType & search.Length > 0)
            {
                foreach (var type1 in types)
                {
                    if (!type1.ToLower().Contains(search))
                        continue;
                    if (GUILayout.Button(type1))
                    {
                        AddField(type1);
                        return;
                    }
                }
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
            for (int i = 0; i < field.fields.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                field.fields[i].name = EditorGUILayout.TextField(field.fields[i].name, GUI.skin.label, GUILayout.MaxWidth(100));
                if (field.fields[i].typeNames == null)
                    field.fields[i].Update();
                field.fields[i].componentIndex = EditorGUILayout.Popup(field.fields[i].componentIndex, field.fields[i].typeNames, GUILayout.MaxWidth(200));
                field.fields[i].typeName = field.fields[i].typeNames[field.fields[i].componentIndex];
                field.fields[i].target = EditorGUILayout.ObjectField(field.fields[i].target, field.fields[i].Type, true);
                if (GUILayout.Button("x", GUILayout.Width(25)))
                {
                    field.fields.RemoveAt(i);
                    EditorUtility.SetDirty(field);
                    return;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    field.fields[i].Update();
                    EditorUtility.SetDirty(field);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            //if (Event.current.type == EventType.DragUpdated | Event.current.type == EventType.DragPerform)
            //{
            //    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;//拖动时显示辅助图标
            //    if (Event.current.type == EventType.DragPerform)
            //    {
            //        search1 = "";
            //        search = DragAndDrop.objectReferences[0].GetType().Name.ToLower();
            //    }
            //}
            data.nameSpace = EditorGUILayout.TextField("命名空间", data.nameSpace);
            if (data.nameSpace != data.nameSpace1)
            {
                data.nameSpace1 = data.nameSpace;
                SaveData();
            }
            data.fullPath = EditorGUILayout.Toggle("(绝/相)对路径", data.fullPath);
            var rect1 = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect1, "文件路径:", data.savePath);
            if (GUI.Button(new Rect(rect1.x + rect1.width - 60, rect1.y, 60, rect1.height), "选择"))
            {
                if (data.fullPath)
                {
                    data.savePath = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                    SaveData();
                }
                else 
                {
                    var path = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                    //相对于Assets路径
                    var uri = new Uri(Application.dataPath.Replace('/', '\\'));
                    var relativeUri = uri.MakeRelativeUri(new Uri(path));
                    data.savePath = relativeUri.ToString();
                    SaveData();
                }
            }
            var rect4 = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect4, "文件路径扩展:", data.savePathExt);
            if (GUI.Button(new Rect(rect4.x + rect4.width - 60, rect4.y, 60, rect4.height), "选择"))
            {
                if (data.fullPath)
                {
                    data.savePathExt = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                    SaveData();
                }
                else
                {
                    var path = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                    //相对于Assets路径
                    var uri = new Uri(Application.dataPath.Replace('/', '\\'));
                    var relativeUri = uri.MakeRelativeUri(new Uri(path));
                    data.savePathExt = relativeUri.ToString();
                    SaveData();
                }
            }
            var rect3 = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect3, "csproj文件:", data.csprojFile);
            if (GUI.Button(new Rect(rect3.x + rect3.width - 60, rect3.y, 60, rect3.height), "选择"))
            {
                if (data.fullPath) 
                {
                    data.csprojFile = EditorUtility.OpenFilePanel("选择文件", "", "csproj");
                    SaveData();
                }
                else
                {
                    var path = EditorUtility.OpenFilePanel("选择文件", "", "csproj");
                    //相对于Assets路径
                    var uri = new Uri(Application.dataPath.Replace('/', '\\'));
                    var relativeUri = uri.MakeRelativeUri(new Uri(path));
                    data.csprojFile = relativeUri.ToString();
                    SaveData();
                }
            }
            EditorGUILayout.BeginHorizontal();
            data.addInheritType = EditorGUILayout.TextField("自定义继承类型", data.addInheritType);
            if (GUILayout.Button("添加"))
            {
                if (!data.inheritTypes.Contains(data.addInheritType))
                    data.inheritTypes.Add(data.addInheritType);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            foreach (var item in data.inheritTypes)
            {
                if (GUILayout.Button(item))
                {
                    data.inheritType = item;
                }
            }
            EditorGUILayout.EndHorizontal();
            data.genericType = EditorGUILayout.Toggle("继承泛型", data.genericType);
            data.inheritType = EditorGUILayout.TextField("继承类型", data.inheritType);
            if (GUILayout.Button("生成脚本(hotfix)"))
            {
                var codeTemplate = @"namespace {nameSpace} 
{
--
    public partial class {typeName} : {inherit}
    {
        public UnityEngine.GameObject panel;
--
        public {fieldType} {fieldName};
--
        public void Init(MVC.View.FieldCollection fc)
        {
            panel = fc.gameObject;
--
            {fieldName} = fc[""{fieldName}""].target as {fieldType};
--
        }
    }
--
}";
                var codeTemplate1 = @"namespace {nameSpace} 
{
--
    public partial class {typeName} : {inherit}
    {
--
        public void InitListener()
        {
--
            {AddListener}
--
        }
--
        private void {methodEvent}
        {
        }
--
    }
--
}";
                string scriptCode;
                {
                    var hasns = !string.IsNullOrEmpty(data.nameSpace);
                    if (string.IsNullOrEmpty(field.fieldName))
                        field.fieldName = field.name;
                    codeTemplate = codeTemplate.Replace("{nameSpace}", data.nameSpace);
                    var typeName = field.fieldName;
                    codeTemplate = codeTemplate.Replace("{typeName}", typeName);
                    var inheritType = data.genericType ? $"{data.inheritType}<{typeName}>" : data.inheritType;
                    codeTemplate = codeTemplate.Replace("{inherit}", inheritType);
                    var codes = codeTemplate.Split(new string[] { "--\r\n" }, StringSplitOptions.None);
                    var sb = new StringBuilder();
                    var sb1 = new StringBuilder();
                    if (hasns)
                        sb.Append(codes[0]);
                    sb.Append(codes[1]);
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        var fieldCode = codes[2].Replace("{fieldType}", field.fields[i].Type.ToString());
                        fieldCode = fieldCode.Replace("{fieldName}", field.fields[i].name);
                        sb.Append(fieldCode);

                        fieldCode = codes[4].Replace("{fieldType}", field.fields[i].Type.ToString());
                        fieldCode = fieldCode.Replace("{fieldName}", field.fields[i].name);
                        fieldCode = fieldCode.Replace("{index}", i.ToString());
                        sb1.Append(fieldCode);
                    }
                    sb.AppendLine();
                    sb.Append(codes[3]);
                    sb.Append(sb1.ToString());
                    sb.Append(codes[5]);
                    if (hasns)
                        sb.Append(codes[6]);
                    scriptCode = sb.ToString();
                    if (!hasns)
                    {
                        var scriptCodes = scriptCode.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        sb.Clear();
                        for (int i = 0; i < scriptCodes.Length; i++)
                        {
                            if (scriptCodes[i].StartsWith("        "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            else if (scriptCodes[i].StartsWith("    "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            sb.AppendLine(scriptCodes[i]);
                        }
                        scriptCode = sb.ToString();
                    }
                    while (scriptCode.EndsWith("\n") | scriptCode.EndsWith("\r"))
                        scriptCode = scriptCode.Remove(scriptCode.Length - 1, 1);
                }

                string scriptCode1;
                {
                    var hasns = !string.IsNullOrEmpty(data.nameSpace);
                    if (string.IsNullOrEmpty(field.fieldName))
                        field.fieldName = field.name;
                    codeTemplate1 = codeTemplate1.Replace("{nameSpace}", data.nameSpace);
                    var typeName = field.fieldName;
                    codeTemplate1 = codeTemplate1.Replace("{typeName}", typeName);
                    var inheritType = data.genericType ? $"{data.inheritType}<{typeName}>" : data.inheritType;
                    codeTemplate1 = codeTemplate1.Replace("{inherit}", inheritType);
                    var codes = codeTemplate1.Split(new string[] { "--\r\n" }, StringSplitOptions.None);
                    var sb = new StringBuilder();
                    var sb1 = new StringBuilder();
                    if (hasns)
                        sb.Append(codes[0]);
                    sb.Append(codes[1]);
                    sb.Append(codes[2]);
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        if (field.fields[i].Type == typeof(Button))
                        {
                            var addListenerText = $"{field.fields[i].name}.onClick.AddListener(On{field.fields[i].name}Click);";
                            var fieldCode = codes[3].Replace("{AddListener}", addListenerText);
                            sb.Append(fieldCode);

                            fieldCode = codes[5].Replace("{methodEvent}", $"On{field.fields[i].name}Click()");
                            sb1.Append(fieldCode);
                        }
                        else if (field.fields[i].Type == typeof(Toggle))
                        {
                            var addListenerText = $"{field.fields[i].name}.onValueChanged.AddListener(On{field.fields[i].name}Changed);";
                            var fieldCode = codes[3].Replace("{AddListener}", addListenerText);
                            sb.Append(fieldCode);

                            fieldCode = codes[5].Replace("{methodEvent}", $"On{field.fields[i].name}Changed(bool isOn)");
                            sb1.Append(fieldCode);
                        }
                    }
                    sb.Append(codes[4]);
                    sb.AppendLine();
                    sb.Append(sb1.ToString());
                    sb.Append(codes[6]);
                    if (hasns)
                        sb.Append(codes[7]);
                    scriptCode1 = sb.ToString();
                    if (!hasns)
                    {
                        var scriptCodes = scriptCode1.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        sb.Clear();
                        for (int i = 0; i < scriptCodes.Length; i++)
                        {
                            if (scriptCodes[i].StartsWith("        "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            else if (scriptCodes[i].StartsWith("    "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            sb.AppendLine(scriptCodes[i]);
                        }
                        scriptCode1 = sb.ToString();
                    }
                    while (scriptCode1.EndsWith("\n") | scriptCode1.EndsWith("\r"))
                        scriptCode1 = scriptCode1.Remove(scriptCode1.Length - 1, 1);
                }
                string path;
                string path1;
                if (data.fullPath)
                {
                    path = data.savePath + $"/{field.fieldName}.cs";
                    path1 = data.savePathExt + $"/{field.fieldName}Ext.cs";
                }
                else
                {
                    path = data.savePath + $"/{field.fieldName}.cs";
                    path1 = data.savePathExt + $"/{field.fieldName}Ext.cs";
                }
                File.WriteAllText(path, scriptCode);
                if (!string.IsNullOrEmpty(path1))
                {
                    if (!File.Exists(path1))
                    {
                        File.WriteAllText(path1, scriptCode1);
                    }
                    else
                    {
                        var code = EditorUtility.DisplayDialogComplex("写入脚本文件", "脚本已存在, 是否替换? 或 尾部添加?", "替换", string.Empty, "尾部添加");
                        switch (code)
                        {
                            case 0:
                                File.WriteAllText(path1, scriptCode1);
                                break;
                            case 2:
                                File.AppendAllText(path1, scriptCode1);
                                break;
                            default:
                                return;
                        }
                    }
                }
                //csproj对主工程无效
                AssetDatabase.Refresh();
                Debug.Log($"生成成功:{path}");
            }
            if (GUILayout.Button("生成脚本(主工程)"))
            {
                var codeTemplate = @"namespace {nameSpace} 
{
--
    public partial class {typeName} : {inherit}
    {
--
        public {fieldType} {fieldName};
--
        void OnValidate()
        {
--
            {fieldName} = transform.GetComponentsInChildren<{fieldType}>(true)[{index}]{extend};
--
        }
    }
--
}";
                var codeTemplate1 = @"namespace {nameSpace} 
{
--
    public partial class {typeName} : {inherit}
    {
--
        private void Start()
        {
--
            {AddListener}
--
        }
--
        private void {methodEvent}
        {
        }
--
    }
--
}";
                string scriptCode;
                {
                    var hasns = !string.IsNullOrEmpty(data.nameSpace);
                    if (string.IsNullOrEmpty(field.fieldName))
                        field.fieldName = field.name;
                    codeTemplate = codeTemplate.Replace("{nameSpace}", data.nameSpace);
                    var typeName = field.fieldName;
                    codeTemplate = codeTemplate.Replace("{typeName}", typeName);
                    var inheritType = data.genericType ? $"{data.inheritType}<{typeName}>" : data.inheritType;
                    codeTemplate = codeTemplate.Replace("{inherit}", inheritType);
                    var codes = codeTemplate.Split(new string[] { "--\r\n" }, StringSplitOptions.None);
                    var sb = new StringBuilder();
                    var sb1 = new StringBuilder();
                    if (hasns)
                        sb.Append(codes[0]);
                    sb.Append(codes[1]);
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        var fieldCode = codes[2].Replace("{fieldType}", field.fields[i].Type.ToString());
                        fieldCode = fieldCode.Replace("{fieldName}", field.fields[i].name);
                        sb.Append(fieldCode);

                        if (field.fields[i].Type == typeof(GameObject))
                        {
                            fieldCode = codes[4].Replace("{fieldType}", "UnityEngine.Transform");
                            fieldCode = fieldCode.Replace("{extend}", ".gameObject");
                        }
                        else
                        {
                            fieldCode = codes[4].Replace("{fieldType}", field.fields[i].Type.ToString());
                            fieldCode = fieldCode.Replace("{extend}", "");
                        }
                        fieldCode = fieldCode.Replace("{fieldName}", field.fields[i].name);
                        fieldCode = fieldCode.Replace("{index}", i.ToString());
                        sb1.Append(fieldCode);
                    }
                    sb.AppendLine();
                    sb.Append(codes[3]);
                    sb.Append(sb1.ToString());
                    sb.Append(codes[5]);
                    if (hasns)
                        sb.Append(codes[6]);
                    scriptCode = sb.ToString();
                    if (!hasns)
                    {
                        var scriptCodes = scriptCode.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        sb.Clear();
                        for (int i = 0; i < scriptCodes.Length; i++)
                        {
                            if (scriptCodes[i].StartsWith("        "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            else if (scriptCodes[i].StartsWith("    "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            sb.AppendLine(scriptCodes[i]);
                        }
                        scriptCode = sb.ToString();
                    }
                    while (scriptCode.EndsWith("\n") | scriptCode.EndsWith("\r"))
                        scriptCode = scriptCode.Remove(scriptCode.Length - 1, 1);
                }

                string scriptCode1;
                {
                    var hasns = !string.IsNullOrEmpty(data.nameSpace);
                    if (string.IsNullOrEmpty(field.fieldName))
                        field.fieldName = field.name;
                    codeTemplate1 = codeTemplate1.Replace("{nameSpace}", data.nameSpace);
                    var typeName = field.fieldName;
                    codeTemplate1 = codeTemplate1.Replace("{typeName}", typeName);
                    var inheritType = data.genericType ? $"{data.inheritType}<{typeName}>" : data.inheritType;
                    codeTemplate1 = codeTemplate1.Replace("{inherit}", inheritType);
                    var codes = codeTemplate1.Split(new string[] { "--\r\n" }, StringSplitOptions.None);
                    var sb = new StringBuilder();
                    var sb1 = new StringBuilder();
                    if (hasns)
                        sb.Append(codes[0]);
                    sb.Append(codes[1]);
                    sb.Append(codes[2]);
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        if (field.fields[i].Type == typeof(Button))
                        {
                            var addListenerText = $"{field.fields[i].name}.onClick.AddListener(On{field.fields[i].name}Click);";
                            var fieldCode = codes[3].Replace("{AddListener}", addListenerText);
                            sb.Append(fieldCode);

                            fieldCode = codes[5].Replace("{methodEvent}", $"On{field.fields[i].name}Click()");
                            sb1.Append(fieldCode);
                        }
                        else if (field.fields[i].Type == typeof(Toggle))
                        {
                            var addListenerText = $"{field.fields[i].name}.onValueChanged.AddListener(On{field.fields[i].name}Changed);";
                            var fieldCode = codes[3].Replace("{AddListener}", addListenerText);
                            sb.Append(fieldCode);

                            fieldCode = codes[5].Replace("{methodEvent}", $"On{field.fields[i].name}Changed(bool isOn)");
                            sb1.Append(fieldCode);
                        }
                    }
                    sb.Append(codes[4]);
                    sb.AppendLine();
                    sb.Append(sb1.ToString());
                    sb.Append(codes[6]);
                    if (hasns)
                        sb.Append(codes[7]);
                    scriptCode1 = sb.ToString();
                    if (!hasns)
                    {
                        var scriptCodes = scriptCode1.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        sb.Clear();
                        for (int i = 0; i < scriptCodes.Length; i++)
                        {
                            if (scriptCodes[i].StartsWith("        "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            else if (scriptCodes[i].StartsWith("    "))
                                scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                            sb.AppendLine(scriptCodes[i]);
                        }
                        scriptCode1 = sb.ToString();
                    }
                    while (scriptCode1.EndsWith("\n") | scriptCode1.EndsWith("\r"))
                        scriptCode1 = scriptCode1.Remove(scriptCode1.Length - 1, 1);
                }
                string path;
                string path1;
                if (data.fullPath)
                {
                    path = data.savePath + $"/{field.fieldName}.cs";
                    path1 = data.savePathExt + $"/{field.fieldName}Ext.cs";
                }
                else
                {
                    path = data.savePath + $"/{field.fieldName}.cs";
                    path1 = data.savePathExt + $"/{field.fieldName}Ext.cs";
                }
                File.WriteAllText(path, scriptCode);
                if (!string.IsNullOrEmpty(path1))
                {
                    if (!File.Exists(path1))
                    {
                        File.WriteAllText(path1, scriptCode1);
                    }
                    else
                    {
                        var code = EditorUtility.DisplayDialogComplex("写入脚本文件", "脚本已存在, 是否替换? 或 尾部添加?", "替换", string.Empty, "尾部添加");
                        switch (code)
                        {
                            case 0:
                                File.WriteAllText(path1, scriptCode1);
                                break;
                            case 2:
                                File.AppendAllText(path1, scriptCode1);
                                break;
                            default:
                                return;
                        }
                    }
                }
                //csproj对主工程无效
                AssetDatabase.Refresh();
                Debug.Log($"生成成功:{path}");
                field.compiling = true;
            }
        }

        [DidReloadScripts]
        static void OnScriptCompilation()
        {
            var gameObject = Selection.activeGameObject;
            if (gameObject == null)
                return;
            if (!gameObject.TryGetComponent<FieldCollection>(out var fieldCollection))
                return;
            if (fieldCollection.compiling)
            {
                fieldCollection.compiling = false;
                var data = PersistHelper.Deserialize<JsonSave>("fcdata.txt");
                string componentTypeName;
                if (string.IsNullOrEmpty(data.nameSpace))
                    componentTypeName = fieldCollection.fieldName;
                else
                    componentTypeName = data.nameSpace + "." + fieldCollection.fieldName;
                var type = AssemblyHelper.GetType(componentTypeName);
                if (type == null)
                    return;
                if (fieldCollection.TryGetComponent(type, out var component))
                {
                    component.GetType().GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(component, null);
                    return;
                }
                fieldCollection.gameObject.AddComponent(type);
            }
        }
    }
}
#endif