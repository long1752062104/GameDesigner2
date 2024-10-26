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
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;

    public class FieldCollectionWindow : EditorWindow
    {
        internal static void Init(FieldCollection field)
        {
            GetWindow<FieldCollectionWindow>("字段收集器", true);
            FieldCollectionEntity.OnEnable(field);
        }

        private void OnEnable()
        {
            FieldCollectionEntity.LoadData();
        }

        private void OnDisable()
        {
            FieldCollectionEntity.OnDisable();
        }

        void OnGUI()
        {
            var rect = new Rect(position.width - 20, 0, 20, 20);
            FieldCollectionEntity.IsLocked = EditorGUI.Toggle(rect, FieldCollectionEntity.IsLocked, "IN LockButton");
            FieldCollectionEntity.OnDragGuiWindow();
            FieldCollectionEntity.OnDragGUI();
        }
    }

    [CustomEditor(typeof(FieldCollection))]
    [CanEditMultipleObjects]
    public class FieldCollectionEdit : Editor
    {
        private FieldCollection self;

        private void OnEnable()
        {
            self = target as FieldCollection;
            if (!FieldCollectionEntity.IsLocked)
                FieldCollectionEntity.OnEnable(self);
        }

        private void OnDisable()
        {
            if (!FieldCollectionEntity.IsLocked)
                FieldCollectionEntity.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < self.fields.Count; i++)
            {
                var field = self.fields[i];
                if (field.typeNames == null)
                    field.Update();
                if (field.enableLabel)
                    EditorGUILayout.LabelField(field.label, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                var content = new GUIContent(field.name, field.note);
                field.target = EditorGUILayout.ObjectField(content, field.target, field.Type, true);
            }
            if (GUILayout.Button("详细界面"))
                FieldCollectionWindow.Init(self);
            if (GUILayout.Button("编辑脚本"))
                FieldCollectionEntity.OpenScript(self);
            if (GUILayout.Button("生成动态代码"))
                FieldCollectionEntity.CodeGeneration(self, true);
            if (GUILayout.Button("生成固定代码"))
                FieldCollectionEntity.CodeGeneration(self, false);
        }
    }

    public class FieldCollectionEntity
    {
        private static FieldCollection collect;
        internal static string search = "", search1 = "", fieldName = "";
        internal static Object selectObject;
        internal static JsonSave data = new JsonSave();
        private static Vector2 scrollPosition;
        private static string searchAssemblies;
        private static ReorderableList fieldList;
        internal static bool IsLocked;

        public class FilePath
        {
            public string path;
            public string pathEx;

            public FilePath() { }

            public FilePath(string path)
            {
                this.path = path;
            }

            public override bool Equals(object obj)
            {
                if (obj is not FilePath pathEntry)
                    return false;
                if (pathEntry.path == path)
                    return true;
                return false;
            }
        }

        public class JsonSave
        {
            public string nameSpace;
            public List<string> savePath = new List<string>(); //兼容旧版本
            public List<string> savePathExt = new List<string>(); //兼容旧版本
            public List<FilePath> filePaths = new List<FilePath>();
            public string searchAssemblies = "UnityEngine.CoreModule|Assembly-CSharp|Assembly-CSharp-firstpass";
            internal string nameSpace1;
            public int currMode;
            internal string addInheritType;
            public List<InheritData> inheritTypes = new List<InheritData>()
            {
                new InheritData(false, "UnityEngine.MonoBehaviour"),
                new InheritData(true, "Net.Component.SingleCase"),
                new InheritData(true, "GameCore.UIBase"),
            };
            internal string SavePath(int savePathIndex) => (filePaths.Count > 0 ? filePaths[savePathIndex].path : string.Empty).Replace("\\", "/"); //苹果mac错误解决
            internal string SavePathExt(int savePathExtIndex) => (filePaths.Count > 0 ? filePaths[savePathExtIndex].pathEx : string.Empty).Replace("\\", "/"); //苹果mac错误解决
            internal InheritData InheritType(int index) => inheritTypes[index];
            private string[] inheritTypesStr = new string[0];
            internal string[] InheritTypesStr
            {
                get
                {
                    if (inheritTypesStr.Length != inheritTypes.Count)
                    {
                        inheritTypesStr = new string[inheritTypes.Count];
                        for (int i = 0; i < inheritTypesStr.Length; i++)
                            inheritTypesStr[i] = inheritTypes[i].inheritType;
                    }
                    return inheritTypesStr;
                }
            }
            private int savePathCount;
            private string[] savePathDisplay = new string[0];
            internal string[] SavePathDisplay
            {
                get
                {
                    if (savePathCount != filePaths.Count)
                    {
                        savePathCount = filePaths.Count;
                        savePathDisplay = new string[filePaths.Count];
                        for (int i = 0; i < filePaths.Count; i++)
                        {
                            savePathDisplay[i] = filePaths[i].path;
                        }
                    }
                    return savePathDisplay;
                }
            }
        }

        public class InheritData
        {
            public bool genericType;
            public string inheritType;

            public InheritData() { }

            public InheritData(bool genericType, string inheritType)
            {
                this.genericType = genericType;
                this.inheritType = inheritType;
            }

            public override int GetHashCode()
            {
                return inheritType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is InheritData obj1)
                    return GetHashCode() == obj1.GetHashCode();
                return false;
            }
        }

        internal static void OnEnable(FieldCollection target)
        {
            LoadData();
            collect = target;
            fieldList = new ReorderableList(target.fields, typeof(FieldCollection.Field), true, false, false, false)
            {
                elementHeightCallback = OnElementHeightCallback,
                drawElementCallback = OnDrawElementCallback
            };
            searchAssemblies = data.searchAssemblies;
            if (!string.IsNullOrEmpty(searchAssemblies))
            {
                var assemblyNames = searchAssemblies.Split('|');
                var types1 = new List<string>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assemblie in assemblies)
                {
                    var name = assemblie.GetName().Name;
                    if (assemblyNames.Contains(name))
                    {
                        var types = assemblie.GetTypes().Where(t => !t.IsAbstract & !t.IsInterface & !t.IsGenericType & !t.IsGenericTypeDefinition);
                        foreach (var type in types)
                            types1.Add(type.ToString());
                    }
                }
            }
        }

        private static float OnElementHeightCallback(int index)
        {
            var field = collect.fields[index];
            float height = 20f;
            if (field.enableLabel)
                height += 20f;
            return height;
        }

        private static void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= collect.fields.Count) //移除元素时报错
                return;
            var field = collect.fields[index];
            if (field.enableLabel)
            {
                rect.height = 20f;
                field.label = EditorGUI.TextField(new Rect(rect) { width = rect.width }, field.label, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                rect.y += 20f;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            field.name = EditorGUI.TextField(new Rect(rect) { width = 150 }, field.name, GUI.skin.label);
            if (field.typeNames == null)
                field.Update();
            field.componentIndex = EditorGUI.Popup(new Rect(rect) { x = 150, width = 200 }, field.componentIndex, field.typeNames);
            field.typeName = field.typeNames[field.componentIndex];
            field.target = EditorGUI.ObjectField(new Rect(rect) { x = 355, width = 150 }, field.target, field.Type, true);
            field.note = EditorGUI.TextField(new Rect(rect) { x = 510, width = rect.width - 540 }, field.note);
            if (string.IsNullOrEmpty(field.note))
            {
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.grey;
                EditorGUI.LabelField(new Rect(rect) { x = 512, width = rect.width - 538 }, "注释!", style);
            }
            if (GUI.Button(new Rect(rect) { x = rect.width - 25, width = 25 }, field.enableLabel ? "-" : "+"))
            {
                field.enableLabel = !field.enableLabel;
                EditorUtility.SetDirty(collect);
            }
            if (GUI.Button(new Rect(rect) { x = rect.width, width = 25 }, "x"))
            {
                collect.fields.RemoveAt(index);
                EditorUtility.SetDirty(collect);
            }
            if (EditorGUI.EndChangeCheck())
            {
                field.Update();
                EditorUtility.SetDirty(collect);
            }
            EditorGUILayout.EndHorizontal();
        }

        internal static void OnDisable()
        {
            SaveData();
            if (collect != null)
                EditorUtility.SetDirty(collect);
        }

        internal static void LoadData()
        {
            data = PersistHelper.Deserialize<JsonSave>("fcdata.json");
            for (int i = 0; i < data.savePath.Count; i++)
            {
                var filePath = new FilePath()
                {
                    path = data.savePath[i],
                    pathEx = i < data.savePathExt.Count ? data.savePathExt[i] : string.Empty
                };
                if (!data.filePaths.Contains(filePath))
                    data.filePaths.Add(filePath);
            }
            for (int i = 0; i < data.filePaths.Count; i++)
            {
                data.filePaths[i].path = data.filePaths[i].path.Replace('/', '\\');
                data.filePaths[i].pathEx = data.filePaths[i].pathEx.Replace('/', '\\');
            }
            if (data.savePath.Count > 0 | data.savePathExt.Count > 0)
            {
                data.savePath.Clear();
                data.savePathExt.Clear();
                SaveData();
            }
        }

        static void SaveData()
        {
            PersistHelper.Serialize(data, "fcdata.json");
        }

        internal static void AddField(string typeName)
        {
            var name = fieldName;
            if (name == "")
                name = "name" + collect.nameIndex++;
            foreach (var f in collect.fields)
            {
                if (f.name == fieldName)
                {
                    name += collect.nameIndex++;
                    break;
                }
            }
            var field1 = new FieldCollection.Field() { name = name, typeName = typeName };
            collect.fields.Add(field1);
            if (selectObject != null)
                field1.target = selectObject;
            field1.Update();
            EditorUtility.SetDirty(collect);
        }

        public static void OnDragGuiWindow()
        {
            data.currMode = EditorGUILayout.Popup("添加字段模式", data.currMode, new string[] { "自动添加字段", "选择添加字段", "修改字段" });
            if ((Event.current.type == EventType.DragUpdated | Event.current.type == EventType.DragPerform) & data.currMode != 2)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;//拖动时显示辅助图标
                if (Event.current.type == EventType.DragPerform)
                {
                    if (data.currMode == 0)
                    {
                        var componentPriority = new List<Type>()
                        {
                            typeof(Button), typeof(Toggle), typeof(Text), typeof(Slider), typeof(Scrollbar), typeof(Dropdown),
                            typeof(ScrollRect), typeof(InputField), typeof(Image)
                        };
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            fieldName = obj.name.Replace(" ", "").Replace("(", "_").Replace(")", "_");
                            if (obj is GameObject go)
                            {
                                var objects = new List<Object>() { obj };
                                objects.AddRange(go.GetComponents<Component>());
                                foreach (var cp in componentPriority)
                                {
                                    var components = objects.Where(item => item.GetType() == cp).ToList();
                                    if (components.Count > 0)
                                    {
                                        selectObject = components[0];
                                        AddField(components[0].GetType().ToString());
                                        goto J;
                                    }
                                }
                                selectObject = objects[objects.Count - 1];
                                AddField(objects[objects.Count - 1].GetType().ToString());
                            }
                            else if (obj is Component component)
                            {
                                selectObject = component;
                                AddField(component.GetType().ToString());
                            }
                        J:;
                        }
                        return;
                    }
                    else if (data.currMode == 1)
                    {
                        var dict = new Dictionary<Type, List<Object[]>>();
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            GameObject gameObject;
                            if (obj is Component component)
                                gameObject = component.gameObject;
                            else
                                gameObject = obj as GameObject;
                            var objects = new List<Object>() { gameObject };
                            objects.AddRange(gameObject.GetComponents<Component>());
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
                                    fieldName = item1[0].name.Replace(" ", "").Replace("(", "_").Replace(")", "_");
                                    selectObject = item1[1];
                                    AddField(typeName);
                                }
                            });
                        }
                        menu.ShowAsContext();
                        Event.current.Use();
                        return;
                    }
                    else
                    {
                        search1 = "";
                        search = DragAndDrop.objectReferences[0].GetType().Name.ToLower();
                    }
                }
            }
        }

        public static void OnDragGUI()
        {
            if (collect == null)
                return;
            data.searchAssemblies = EditorGUILayout.TextField("搜索的程序集", data.searchAssemblies);
            if (data.searchAssemblies != searchAssemblies)
            {
                searchAssemblies = data.searchAssemblies;
                SaveData();
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
            fieldList.DoLayoutList();
            GUILayout.EndScrollView();
            data.nameSpace = EditorGUILayout.TextField("命名空间", data.nameSpace);
            if (data.nameSpace != data.nameSpace1)
            {
                data.nameSpace1 = data.nameSpace;
                SaveData();
            }
            collect.fieldName = EditorGUILayout.TextField("类型名称", collect.fieldName);
            collect.isInherit = EditorGUILayout.Toggle("继承类型", collect.isInherit);
            if (collect.isInherit)
            {
                EditorGUILayout.BeginHorizontal();
                data.addInheritType = EditorGUILayout.TextField("添加继承", data.addInheritType);
                if (GUILayout.Button("添加", GUILayout.Width(60f)))
                {
                    if (!string.IsNullOrEmpty(data.addInheritType))
                    {
                        var inheritData = new InheritData(false, data.addInheritType);
                        if (!data.inheritTypes.Contains(inheritData))
                            data.inheritTypes.Add(inheritData);
                    }
                }
                EditorGUILayout.EndHorizontal();
                var inheritData1 = data.InheritType(collect.inheritTypeInx);
                inheritData1.genericType = EditorGUILayout.Toggle("泛型类型", inheritData1.genericType);
                EditorGUILayout.BeginHorizontal();
                collect.inheritTypeInx = EditorGUILayout.Popup("继承类型", collect.inheritTypeInx, data.InheritTypesStr);
                if (GUILayout.Button("删除", GUILayout.Width(60f)))
                {
                    if (data.inheritTypes.Count > 1)
                    {
                        collect.inheritTypeInx = 0;
                        data.inheritTypes.Remove(inheritData1);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            var rect1 = EditorGUILayout.GetControlRect();
            collect.pathIndex = EditorGUI.Popup(new Rect(rect1.x, rect1.y, rect1.width - 90, rect1.height), "生成路径", collect.pathIndex, data.SavePathDisplay);
            if (GUI.Button(new Rect(rect1.x + rect1.width - 90, rect1.y, 30, rect1.height), "x"))
            {
                if (data.filePaths.Count > 0)
                    data.filePaths.RemoveAt(collect.pathIndex);
                SaveData();
            }
            if (GUI.Button(new Rect(rect1.x + rect1.width - 60, rect1.y, 60, rect1.height), "选择"))
            {
                var path = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                if (string.IsNullOrEmpty(path))
                    return;
                path = PathHelper.GetRelativePath(Application.dataPath, path, '/', '\\');
                var pathEntry = new FilePath(path) { pathEx = string.Empty };
                if (!data.filePaths.Contains(pathEntry))
                {
                    data.filePaths.Add(pathEntry);
                    collect.pathIndex = data.filePaths.Count - 1;
                }
                SaveData();
            }
            var rect2 = EditorGUILayout.GetControlRect();
            var pathEx = collect.pathIndex < data.filePaths.Count ? data.filePaths[collect.pathIndex].pathEx : string.Empty;
            EditorGUI.LabelField(new Rect(rect2.x, rect2.y, rect2.width - 90, rect2.height), "扩展路径", pathEx);
            if (GUI.Button(new Rect(rect1.x + rect2.width - 90, rect2.y, 30, rect2.height), "x"))
            {
                if (collect.pathIndex < data.filePaths.Count)
                    data.filePaths[collect.pathIndex].pathEx = string.Empty;
                SaveData();
            }
            if (GUI.Button(new Rect(rect2.x + rect2.width - 60, rect2.y, 60, rect2.height), "选择"))
            {
                var path = EditorUtility.OpenFolderPanel("选择扩展路径", "", "");
                if (string.IsNullOrEmpty(path))
                    return;
                path = PathHelper.GetRelativePath(Application.dataPath, path, '/', '\\');
                if (collect.pathIndex < data.filePaths.Count)
                    data.filePaths[collect.pathIndex].pathEx = path;
                SaveData();
            }
            if (GUILayout.Button("编辑脚本"))
                OpenScript(collect);
            if (GUILayout.Button("生成动态代码"))
                CodeGeneration(collect, true);
            if (GUILayout.Button("生成固定代码"))
                CodeGeneration(collect, false);
        }

        internal static void CodeGeneration(FieldCollection field, bool isDynamic)
        {
            CodeGenerationFieldPart(isDynamic);
            CodeGenerationEditPart();
            AssetDatabase.Refresh();
            field.isDynamic = isDynamic;
            field.compiling = true;
        }

        internal static void OpenScript(FieldCollection collect)
        {
            var path = data.SavePathExt(collect.pathIndex);
            if (string.IsNullOrEmpty(path))
                return;
            path += $"/{collect.fieldName}Ext.cs";
            if (File.Exists(path))
                InternalEditorUtility.OpenFileAtLineExternal(path, 0, 0);
            else
                Debug.Log("文件未生成!");
        }

        private static void CodeGenerationEditPart()
        {
            var codeTemplate = @"namespace {nameSpace} 
{
--
    //项目需要用到UTF8编码进行保存, 默认情况下是中文编码(GB2312), 如果更新MVC后发现脚本的中文乱码则需要处理一下
    //以下是设置UTF8编码的Url:方法二 安装插件
    //url:https://blog.csdn.net/hfy1237/article/details/129858976
    public partial class {typeName}
    {
        private void Start()
        {
            InitListener();
        }

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
            var hasns = !string.IsNullOrEmpty(data.nameSpace);
            if (string.IsNullOrEmpty(collect.fieldName))
                collect.fieldName = collect.name;
            codeTemplate = codeTemplate.Replace("{nameSpace}", data.nameSpace);
            var typeName = collect.fieldName;
            codeTemplate = codeTemplate.Replace("{typeName}", typeName);
            var inheritData = data.InheritType(collect.inheritTypeInx);
            var inheritType = inheritData.genericType ? $"{inheritData.inheritType}<{typeName}>" : inheritData.inheritType;
            codeTemplate = codeTemplate.Replace("{inherit}", inheritType);
            var codes = codeTemplate.Split(new string[] { "--\r\n" }, StringSplitOptions.None);
            var codeText = new StringBuilder();
            var lostenerCodeText = new StringBuilder();
            if (hasns) codeText.Append(codes[0]);
            codeText.Append(codes[1]);
            for (int i = 0; i < collect.fields.Count; i++)
            {
                if (collect.fields[i].Type == typeof(Button))
                {
                    var addListenerText = $"{collect.fields[i].name}.onClick.AddListener(On{collect.fields[i].name}Click);";
                    var fieldCode = codes[2].Replace("{AddListener}", addListenerText);
                    codeText.Append(fieldCode);

                    fieldCode = codes[4].Replace("{methodEvent}", $"On{collect.fields[i].name}Click()");
                    lostenerCodeText.Append(fieldCode);
                }
                else if (collect.fields[i].Type == typeof(Toggle))
                {
                    var addListenerText = $"{collect.fields[i].name}.onValueChanged.AddListener(On{collect.fields[i].name}Changed);";
                    var fieldCode = codes[2].Replace("{AddListener}", addListenerText);
                    codeText.Append(fieldCode);

                    fieldCode = codes[4].Replace("{methodEvent}", $"On{collect.fields[i].name}Changed(bool isOn)");
                    lostenerCodeText.Append(fieldCode);
                }
            }
            codeText.Append(codes[3]);
            codeText.AppendLine();
            codeText.Append(lostenerCodeText.ToString());
            codeText.Append(codes[5]);
            if (hasns) codeText.Append(codes[6]);
            var scriptCode = codeText.ToString();
            if (!hasns)
            {
                var scriptCodes = scriptCode.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                codeText.Clear();
                for (int i = 0; i < scriptCodes.Length; i++)
                {
                    if (scriptCodes[i].StartsWith("        "))
                        scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                    else if (scriptCodes[i].StartsWith("    "))
                        scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                    codeText.AppendLine(scriptCodes[i]);
                }
                scriptCode = codeText.ToString();
            }
            while (scriptCode.EndsWith("\n") | scriptCode.EndsWith("\r"))
                scriptCode = scriptCode.Remove(scriptCode.Length - 1, 1);
            var path = data.SavePathExt(collect.pathIndex);
            if (string.IsNullOrEmpty(path))
                return;
            path += $"/{collect.fieldName}Ext.cs";
            if (File.Exists(path))
            {
                var lines = new List<string>(File.ReadAllLines(path));
                int startIndex = 0;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("public void InitListener()"))
                    {
                        startIndex = i + 2;
                        break;
                    }
                }
                for (int i = 0; i < collect.fields.Count; i++)
                {
                    var isBtn = collect.fields[i].Type == typeof(Button);
                    var isToggle = collect.fields[i].Type == typeof(Toggle);
                    if (!isBtn & !isToggle)
                        continue;
                    var addListenerText = $"{collect.fields[i].name}.{(isBtn ? "onClick" : "onValueChanged")}.AddListener"; //监听的方法有时候需要改动,所以只判断一半
                    if (!Contains(lines, addListenerText))
                    {
                        addListenerText += $"(On{collect.fields[i].name}{(isBtn ? "Click" : "Changed")});";
                        lines.Insert(startIndex, (hasns ? "            " : "        ") + addListenerText);
                        startIndex++;
                        var fieldCode = codes[4].Replace("{methodEvent}", $"On{collect.fields[i].name}{(isBtn ? "Click()" : "Changed(bool isOn)")}");
                        var fieldCodes = fieldCode.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int x = 0; x < fieldCodes.Length; x++)
                        {
                            var fieldCode2 = fieldCodes[x];
                            if (!hasns)
                            {
                                if (fieldCode2.StartsWith("        "))
                                    fieldCode2 = fieldCode2.Remove(0, 4);
                                else if (fieldCode2.StartsWith("    "))
                                    fieldCode2 = fieldCode2.Remove(0, 4);
                            }
                            lines.Insert(lines.Count - (hasns ? 2 : 1), fieldCode2);
                        }
                    }
                }
                scriptCode = "";
                for (int i = 0; i < lines.Count; i++)
                    scriptCode += lines[i] + "\r\n";
            }
            File.WriteAllText(path, scriptCode);
            Debug.Log($"生成成功:{path}");
        }

        private static void CodeGenerationFieldPart(bool isDynamic)
        {
            var codeTemplate = @"namespace {nameSpace} 
{
--
    public partial class {typeName} {inherit}
    {
--
        public MVC.View.FieldCollection collect;
--
        {note}
        {field}
--
        public void Init(MVC.View.FieldCollection collect)
        {
            this.collect = collect;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var getComponentMethod = GetType().GetMethod(""GetComponent"", new System.Type[] { typeof(System.Type) }); //当处于热更新脚本, 不继承MonoBehaviour时处理
            if (getComponentMethod != null)
                collect = getComponentMethod.Invoke(this, new object[] { typeof(MVC.View.FieldCollection) }) as MVC.View.FieldCollection;
        }
#endif
--
    }
--
}";
            var hasns = !string.IsNullOrEmpty(data.nameSpace);
            if (string.IsNullOrEmpty(collect.fieldName))
                collect.fieldName = collect.name;
            codeTemplate = codeTemplate.Replace("{nameSpace}", data.nameSpace);
            var typeName = collect.fieldName;
            codeTemplate = codeTemplate.Replace("{typeName}", typeName);
            var inheritData = data.InheritType(collect.inheritTypeInx);
            var inheritType = inheritData.genericType ? $"{inheritData.inheritType}<{typeName}>" : inheritData.inheritType;
            if (collect.isInherit) codeTemplate = codeTemplate.Replace("{inherit}", $": {inheritType}");
            else codeTemplate = codeTemplate.Replace("{inherit}", "");
            var codes = codeTemplate.Split(new string[] { "--\r\n" }, StringSplitOptions.None);
            var codeText = new StringBuilder();
            if (hasns) codeText.Append(codes[0]);
            codeText.Append(codes[1]);
            if (isDynamic)
                codeText.Append(codes[2]);
            for (int i = 0; i < collect.fields.Count; i++)
            {
                string fieldCode;
                if (isDynamic)
                    fieldCode = codes[3].Replace("{field}", $"public {collect.fields[i].Type} {collect.fields[i].name} {{ get => collect.Get<{collect.fields[i].Type}>({i}); set => collect.Set({i}, value); }}");
                else
                    fieldCode = codes[3].Replace("{field}", $"public {collect.fields[i].Type} {collect.fields[i].name};");
                fieldCode = fieldCode.Replace("{note}", string.IsNullOrEmpty(collect.fields[i].note) ? "" : $"/// <summary>{collect.fields[i].note}</summary>");
                codeText.Append(fieldCode);
            }
            codeText.AppendLine();
            if (isDynamic)
                codeText.Append(codes[4]);
            codeText.Append(codes[5]);
            if (hasns) codeText.Append(codes[6]);
            var scriptCode = codeText.ToString();
            if (!hasns)
            {
                var scriptCodes = scriptCode.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                codeText.Clear();
                for (int i = 0; i < scriptCodes.Length; i++)
                {
                    if (scriptCodes[i].StartsWith("        "))
                        scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                    else if (scriptCodes[i].StartsWith("    "))
                        scriptCodes[i] = scriptCodes[i].Remove(0, 4);
                    codeText.AppendLine(scriptCodes[i]);
                }
                scriptCode = codeText.ToString();
            }
            while (scriptCode.EndsWith("\n") | scriptCode.EndsWith("\r"))
                scriptCode = scriptCode.Remove(scriptCode.Length - 1, 1);
            var path = data.SavePath(collect.pathIndex);
            if (string.IsNullOrEmpty(path))
                return;
            path += $"/{collect.fieldName}.cs";
            File.WriteAllText(path, scriptCode);
            Debug.Log($"生成成功:{path}");
        }

        private static bool Contains(List<string> list, string text)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains(text))
                    return true;
            }
            return false;
        }

        [DidReloadScripts]
        static void OnScriptCompilation()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                if (gameObject == null)
                    continue;
                if (!gameObject.TryGetComponent<FieldCollection>(out var fieldCollection))
                    continue;
                if (fieldCollection.compiling)
                {
                    fieldCollection.compiling = false;
                    var data = PersistHelper.Deserialize<JsonSave>("fcdata.json");
                    string componentTypeName;
                    if (string.IsNullOrEmpty(data.nameSpace))
                        componentTypeName = fieldCollection.fieldName;
                    else
                        componentTypeName = data.nameSpace + "." + fieldCollection.fieldName;
                    var type = AssemblyHelper.GetType(componentTypeName);
                    if (type == null)
                        continue;
                    if (!type.IsSubclassOf(typeof(Component)))
                        continue;
                    if (!fieldCollection.TryGetComponent(type, out var component))
                        component = fieldCollection.gameObject.AddComponent(type);
                    if (fieldCollection.isDynamic)
                    {
                        var onValidateMethod = type.GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        onValidateMethod.Invoke(component, null);
                    }
                    else
                    {
                        foreach (var item in fieldCollection.fields)
                        {
                            var field = type.GetField(item.name, BindingFlags.Public | BindingFlags.Instance);
                            if (field == null) continue;
                            field.SetValue(component, item.target);
                        }
                        EditorUtility.SetDirty(component);
                    }
                }
            }
        }
    }
}
#endif