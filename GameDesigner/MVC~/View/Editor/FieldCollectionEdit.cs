﻿#if (UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL) && UNITY_EDITOR
namespace MVC.View
{
    using Net.Helper;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
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
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            field.fieldName = obj.name;
                            field.selectObject = obj;
                            field.AddField(obj.GetType().FullName);
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
                                    field.fieldName = item1[0].name;
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
            try { field.OnInspectorGUI(); } catch { }
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
        private int deleteArrayIndex = -1;
        private bool doubleClick;
        private int index;
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
            EditorUtility.SetDirty(field);
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开收集器界面")) 
                FieldCollectionWindow.Init(this);
            var so = serializedObject;
            so.Update();
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
                try
                {
                    if (deleteArrayIndex != -1)
                    {
                        field.fields.RemoveAt(deleteArrayIndex);
                        deleteArrayIndex = -1;
                        EditorUtility.SetDirty(field);
                        break;
                    }
                    var rect = EditorGUILayout.GetControlRect();
                    so.FindProperty("fields").GetArrayElementAtIndex(i).FindPropertyRelative("target").objectReferenceValue = EditorGUI.ObjectField(rect, field.fields[i].name, field.fields[i].target, field.fields[i].Type, true);
                    if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))//判断鼠标右键事件
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("删除字段"), false, (index) =>
                        {
                            deleteArrayIndex = (int)index;
                        }, i);
                        menu.ShowAsContext();
                        Event.current.Use();//设置该事件被使用
                    }
                    if (Event.current.clickCount == 2 && rect.Contains(Event.current.mousePosition))//判断鼠标左键事件
                    {
                        index = i;
                        doubleClick = true;
                    }
                    if (doubleClick & index == i) 
                    {
                        field.fields[i].name = EditorGUI.TextField(rect, field.fields[i].name);
                        if (Event.current.type == EventType.MouseDown | Event.current.keyCode == KeyCode.Return) 
                        {
                            doubleClick = false;
                            index = -1;
                            EditorUtility.SetDirty(field);
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
            GUILayout.EndScrollView();
            if (Event.current.type == EventType.DragUpdated | Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;//拖动时显示辅助图标
                if (Event.current.type == EventType.DragPerform)
                {
                    search1 = "";
                    search = DragAndDrop.objectReferences[0].GetType().Name.ToLower();
                }
            }
            data.nameSpace = EditorGUILayout.TextField("namespace", data.nameSpace);
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
                    data.savePath = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                    var strs = data.savePath.ToCharArray();
                    var strs1 = Application.dataPath.Replace("Assets", "").ToCharArray();
                    int index = 0;
                    for (int i = 0; i < strs.Length; i++)
                    {
                        if (i >= strs1.Length)
                        {
                            index = i;
                            break;
                        }
                        if (strs[i] != strs1[i])
                        {
                            index = i;
                            break;
                        }
                    }
                    data.savePath = data.savePath.Remove(0, index);
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
                    data.savePathExt = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                    var strs = data.savePathExt.ToCharArray();
                    var strs1 = Application.dataPath.Replace("Assets", "").ToCharArray();
                    int index = 0;
                    for (int i = 0; i < strs.Length; i++)
                    {
                        if (i >= strs1.Length)
                        {
                            index = i;
                            break;
                        }
                        if (strs[i] != strs1[i])
                        {
                            index = i;
                            break;
                        }
                    }
                    data.savePathExt = data.savePathExt.Remove(0, index);
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
                    data.csprojFile = EditorUtility.OpenFilePanel("选择文件", "", "csproj");
                    var strs = data.csprojFile.ToCharArray();
                    var strs1 = Application.dataPath.Replace("Assets", "").ToCharArray();
                    int index = 0;
                    for (int i = 0; i < strs.Length; i++)
                    {
                        if (i >= strs1.Length)
                        {
                            index = i;
                            break;
                        }
                        if (strs[i] != strs1[i])
                        {
                            index = i;
                            break;
                        }
                    }
                    data.csprojFile = data.csprojFile.Remove(0, index);
                    SaveData();
                }
            }
            if (GUILayout.Button("生成脚本(hotfix)"))
            {
                bool hasns = data.nameSpace != "";
                Func<string> action = new Func<string>(()=> {
                    string str = "";
                    for (int i = 0; i < field.fields.Count; i++) 
                    {
                        str += $"{(hasns ? "\t\t" : "\t")}" + $"public {field.fields[i].Type.Name} {field.fields[i].name};\n";
                    }
                    return str + "\n";
                });
                Func<string> action1 = new Func<string>(() => {
                    string str = "";
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        str += $"{(hasns ? "\t\t\t" : "\t\t")}" + $"{field.fields[i].name} = fc[\"{field.fields[i].name}\"].target as {field.fields[i].Type.Name};\n";
                    }
                    return str;
                });
                Func<string> action2 = new Func<string>(() => {
                    string str = "";
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        if (field.fields[i].Type == typeof(Button))
                        {
                            str += $"{(hasns ? "\t\t\t" : "\t\t")}" + $"{field.fields[i].name}.onClick.AddListener(() => " + "{" + "});\n";
                        }
                        else if (field.fields[i].Type == typeof(Toggle))
                        {
                            str += $"{(hasns ? "\t\t\t" : "\t\t")}" + $"{field.fields[i].name}.onValueChanged.AddListener((value) => " + "{" + "});\n";
                        }
                    }
                    return str;
                });
                if (string.IsNullOrEmpty(field.fieldName))
                    field.fieldName = field.name;
                var scriptStr = "using MVC.View;\n" +
                "using UnityEngine;\n" +
                "using UnityEngine.UI;\n\n" +
                (hasns ? "namespace " + data.nameSpace + "\n{\n" : "") +
                $"{(hasns ? "\t" : "")}" + $"//热更新生成的脚本, 请看gitee的mvc模块使用介绍图示\n" +
                $"{(hasns ? "\t" : "")}" + $"public class {field.fieldName}\n" +
                $"{(hasns ? "\t" : "")}" + "{\n" +
                $"{(hasns ? "\t\t" : "\t")}" + $"public static {field.fieldName} Instance = new {field.fieldName}();\n" +
                $"{(hasns ? "\t\t" : "\t")}" + "public GameObject panel;\n" +
                action() +
                $"{(hasns ? "\t\t" : "\t")}" + "public void Init(FieldCollection fc)\n" +
                $"{(hasns ? "\t\t" : "\t")}" + "{\n" +
                $"{(hasns ? "\t\t\t" : "\t\t")}" + "panel = fc.gameObject;\n" +
                action1() +
                action2() +
                $"{(hasns ? "\t\t" : "\t")}" + "}\n" +
                $"{(hasns ? "\t" : "")}" + "}" +
                (hasns ? "\n}" : "");
                string path = "";
                string path1 = "";
                if (data.fullPath)
                {
                    path = data.savePath + $"/{field.fieldName}.cs";
                    path1 = data.csprojFile;
                }
                else
                {
                    path = Application.dataPath.Replace("Assets", "") + data.savePath + $"/{field.fieldName}.cs";
                    path1 = Application.dataPath.Replace("Assets", "") + data.csprojFile;
                }
                if (File.Exists(path)) 
                {
                    if(!EditorUtility.DisplayDialog("写入脚本文件", "脚本已存在, 是否替换? 或 尾部添加?", "替换", "尾部添加"))
                        File.AppendAllText(path, scriptStr);
                    else File.WriteAllText(path, scriptStr);
                } else File.WriteAllText(path, scriptStr);
                if (File.Exists(path1)) 
                {
                    var rows = File.ReadAllLines(path1);
                    foreach (var row in rows)
                    {
                        if (row.Contains("<Compile Include=\"")) 
                        {
                            var row1 = row.Replace("<Compile Include=\"", "");
                            row1 = row1.Replace("\" />", "");
                            var csName = Path.GetFileName(row1);
                            var csName1 = Path.GetFileName(path);
                            if (csName == csName1)
                                goto J;
                        }
                    }
                    var cspath = Path.GetDirectoryName(path1).Replace("\\", "/");
                    var path2 = path.Replace(cspath, "").TrimStart('/').Replace("/", "\\");
                    List<string> rows1 = new List<string>(rows);
                    rows1.Insert(rows.Length - 3, $"    <Compile Include=\"{path2}\" />");
                    File.WriteAllLines(path1, rows1);
                }
                J: AssetDatabase.Refresh();
                Debug.Log($"生成成功:{path}");
            }
            if (GUILayout.Button("生成脚本(主工程)"))
            {
                bool hasns = data.nameSpace != "";
                Func<string> action = new Func<string>(() => {
                    string str = "";
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        str += $"{(hasns ? "\t\t" : "\t")}" + $"public {field.fields[i].Type.Name} {field.fields[i].name};\n";
                    }
                    return str;
                });
                Func<string> action1 = new Func<string>(() => {
                    string str = "";
                    int index = 0;
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        if (field.fields[i].Type == typeof(GameObject))
                            continue;
                        if (field.fields[i].Type == typeof(UnityEngine.Object))
                            continue;
                        if (!field.fields[i].Type.IsSubclassOf(typeof(Component)))
                            continue;
                        var comps = field.transform.GetComponentsInChildren(field.fields[i].Type);
                        for (int ii = 0; ii < comps.Length; ii++)
                        {
                            var comp = field.fields[i].target as Component;
                            if (comp == comps[ii]) {
                                index = ii;
                                break;
                            }
                        }
                        str += $"{(hasns ? "\t\t\t" : "\t\t")}" + $"{field.fields[i].name} = transform.GetComponentsInChildren<{field.fields[i].Type.Name}>()[{index}];\n";
                    }
                    return str;
                });
                Func<string> action2 = new Func<string>(() => {
                    string str = "";
                    for (int i = 0; i < field.fields.Count; i++)
                    {
                        if (field.fields[i].Type == typeof(Button))
                        {
                            str += $"{(hasns ? "\t\t\t" : "\t\t")}" + $"{field.fields[i].name}.onClick.AddListener(() => " + "{" + "});\n";
                        }
                        else if (field.fields[i].Type == typeof(Toggle))
                        {
                            str += $"{(hasns ? "\t\t\t" : "\t\t")}" + $"{field.fields[i].name}.onValueChanged.AddListener((value) => " + "{" + "});\n";
                        }
                    }
                    return str;
                });
                if (string.IsNullOrEmpty(field.fieldName))
                    field.fieldName = field.name;
                var scriptStr = "using Net.Component;\n" +
                "using UnityEngine;\n" +
                "using UnityEngine.UI;\n\n" +
                (hasns ? "namespace " + data.nameSpace + "\n{\n" : "") +
                $"{(hasns ? "\t" : "")}public partial class {field.fieldName} : SingleCase<{field.fieldName}>\n" +
                $"{(hasns ? "\t" : "")}" + "{\n" +
                action() +
                $"\n{(hasns ? "\t\t" : "\t")}void OnValidate()\n" +
                $"{(hasns ? "\t\t" : "\t")}" + "{\n" +
                action1() +
                $"{(hasns ? "\t\t" : "\t")}" + "}\n" +
                $"{(hasns ? "\t" : "")}" + "}" +
                (hasns ? "\n}" : "");
                string path = "";
                string path1 = "";
                if (data.fullPath)
                {
                    path = data.savePath + $"/{field.fieldName}.cs";
                    path1 = data.savePathExt + $"/{field.fieldName}Ext.cs";
                }
                else
                {
                    path = Application.dataPath.Replace("Assets", "") + data.savePath + $"/{field.fieldName}.cs";
                    path1 = Application.dataPath.Replace("Assets", "") + data.savePathExt + $"/{field.fieldName}Ext.cs";
                }
                if (File.Exists(path)) 
                {
                    if(!EditorUtility.DisplayDialog("写入脚本文件", "脚本已存在, 是否替换? 或 尾部添加?", "替换", "尾部添加"))
                        File.AppendAllText(path, scriptStr);
                    else File.WriteAllText(path, scriptStr);
                } else File.WriteAllText(path, scriptStr);
                if (!File.Exists(path1))
                {
                    var scriptStr1 = "using Net.Component;\n" +
                    "using UnityEngine;\n" +
                    "using UnityEngine.UI;\n\n" +
                    (hasns ? "namespace " + data.nameSpace + "\n{\n" : "") +
                    $"{(hasns ? "\t" : "")}public partial class {field.fieldName} : SingleCase<{field.fieldName}>\n" +
                    $"{(hasns ? "\t" : "")}" + "{\n" +
                    $"\n{(hasns ? "\t\t" : "\t")}void Start()\n" +
                    $"{(hasns ? "\t\t" : "\t")}" + "{\n" +
                    action2() +
                    $"{(hasns ? "\t\t" : "\t")}" + "}\n" +
                    $"{(hasns ? "\t" : "")}" + "}" +
                    (hasns ? "\n}" : "");
                    File.WriteAllText(path1, scriptStr1);
                }
                //csproj对主工程无效
                AssetDatabase.Refresh();
                Debug.Log($"生成成功:{path}");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif