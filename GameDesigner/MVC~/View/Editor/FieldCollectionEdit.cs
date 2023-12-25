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
            FieldCollectionEntity.OnDragGuiWindow();
            FieldCollectionEntity.OnDragGUI();
        }
    }

    [CustomEditor(typeof(FieldCollection))]
    public class FieldCollectionEdit : Editor
    {
        private FieldCollection self;

        private void OnEnable()
        {
            self = target as FieldCollection;
            FieldCollectionEntity.OnEnable(self);
        }

        private void OnDisable()
        {
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
            if (GUILayout.Button("代码生成"))
                FieldCollectionEntity.CodeGeneration(self);
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

        public class JsonSave
        {
            public string nameSpace;
            public List<string> savePath = new List<string>();
            public List<string> savePathExt = new List<string>();
            public string csprojFile;
            public string searchAssemblies = "UnityEngine.CoreModule|Assembly-CSharp|Assembly-CSharp-firstpass";
            internal string nameSpace1;
            public bool changeField;
            public bool addField;
            public bool seleAddField;
            internal string addInheritType;
            public List<InheritData> inheritTypes = new List<InheritData>()
            {
                new InheritData(false, "UnityEngine.MonoBehaviour"),
                new InheritData(true, "Net.Component.SingleCase")
            };
            internal string SavePath(int savePathIndex) => savePath.Count > 0 ? savePath[savePathIndex] : string.Empty;
            internal string SavePathExt(int savePathExtIndex) => savePathExt.Count > 0 ? savePathExt[savePathExtIndex] : string.Empty;
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
                data.savePath[i] = data.savePath[i].Replace('/', '\\');
            for (int i = 0; i < data.savePathExt.Count; i++)
                data.savePathExt[i] = data.savePathExt[i].Replace('/', '\\');
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
            GUILayout.Label("将组件拖到此窗口上! 如果是赋值模式, 拖入的对象将不会显示选择组件!");
            data.changeField = GUILayout.Toggle(data.changeField, "赋值变量");
            data.addField = GUILayout.Toggle(data.addField, "直接添加变量");
            data.seleAddField = GUILayout.Toggle(data.seleAddField, "选择添加变量组件");
            if ((Event.current.type == EventType.DragUpdated | Event.current.type == EventType.DragPerform) & !data.changeField)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;//拖动时显示辅助图标
                if (Event.current.type == EventType.DragPerform)
                {
                    if (data.addField)
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
                    else if (data.seleAddField)
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
            collect.fieldName = EditorGUILayout.TextField("收集器名称", collect.fieldName);
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
            var rect1 = EditorGUILayout.GetControlRect();
            collect.savePathInx = EditorGUI.Popup(new Rect(rect1.x, rect1.y, rect1.width - 90, rect1.height), "生成路径:", collect.savePathInx, data.savePath.ToArray());
            if (GUI.Button(new Rect(rect1.x + rect1.width - 90, rect1.y, 30, rect1.height), "x"))
            {
                if (data.savePath.Count > 0)
                    data.savePath.RemoveAt(collect.savePathInx);
                SaveData();
            }
            if (GUI.Button(new Rect(rect1.x + rect1.width - 60, rect1.y, 60, rect1.height), "选择"))
            {
                var path = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                if (string.IsNullOrEmpty(path))
                    return;
                path = PathHelper.GetRelativePath(Application.dataPath, path, '/', '\\');
                if (!data.savePath.Contains(path))
                {
                    data.savePath.Add(path);
                    collect.savePathInx = data.savePath.Count - 1;
                }
                SaveData();
            }
            var rect4 = EditorGUILayout.GetControlRect();
            collect.savePathExtInx = EditorGUI.Popup(new Rect(rect4.x, rect4.y, rect4.width - 90, rect4.height), "扩展路径:", collect.savePathExtInx, data.savePathExt.ToArray());
            if (GUI.Button(new Rect(rect4.x + rect4.width - 90, rect4.y, 30, rect4.height), "x"))
            {
                if (data.savePathExt.Count > 0)
                    data.savePathExt.RemoveAt(collect.savePathExtInx);
                SaveData();
            }
            if (GUI.Button(new Rect(rect4.x + rect4.width - 60, rect4.y, 60, rect4.height), "选择"))
            {
                var path = EditorUtility.OpenFolderPanel("选择保存路径", "", "");
                if (string.IsNullOrEmpty(path))
                    return;
                path = PathHelper.GetRelativePath(Application.dataPath, path, '/', '\\');
                if (!data.savePathExt.Contains(path))
                {
                    data.savePathExt.Add(path);
                    collect.savePathExtInx = data.savePathExt.Count - 1;
                }
                SaveData();
            }
            var rect3 = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect3, "csproj文件:", data.csprojFile);
            if (GUI.Button(new Rect(rect3.x + rect3.width - 60, rect3.y, 60, rect3.height), "选择"))
            {
                var path = EditorUtility.OpenFilePanel("选择文件", "", "csproj");
                if (string.IsNullOrEmpty(path))
                    return;
                data.csprojFile = PathHelper.GetRelativePath(Application.dataPath, path, '\\', '/');
                SaveData();
            }
            EditorGUILayout.BeginHorizontal();
            data.addInheritType = EditorGUILayout.TextField("自定义继承类型", data.addInheritType);
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
            if (GUILayout.Button("编辑脚本"))
                OpenScript(collect);
            if (GUILayout.Button("代码生成"))
                CodeGeneration(collect);
        }

        internal static void CodeGeneration(FieldCollection field)
        {
            CodeGenerationFieldPart();
            CodeGenerationEditPart();
            AssetDatabase.Refresh();
            field.compiling = true;
        }

        internal static void OpenScript(FieldCollection collect)
        {
            var path = data.SavePathExt(collect.savePathExtInx);
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
            var path = data.SavePathExt(collect.savePathExtInx);
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

        private static void CodeGenerationFieldPart()
        {
            var codeTemplate = @"namespace {nameSpace} 
{
--
    public partial class {typeName} : {inherit}
    {
        public MVC.View.FieldCollection collect;
--
        {note}
        public {fieldType} {fieldName} { get => collect.Get<{fieldType}>({index}); set => collect.Set({index}, value); }
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
            if (hasns) codeText.Append(codes[0]);
            codeText.Append(codes[1]);
            for (int i = 0; i < collect.fields.Count; i++)
            {
                var fieldCode = codes[2].Replace("{fieldType}", collect.fields[i].Type.ToString());
                fieldCode = fieldCode.Replace("{fieldName}", collect.fields[i].name);
                fieldCode = fieldCode.Replace("{index}", i.ToString());
                fieldCode = fieldCode.Replace("{note}", string.IsNullOrEmpty(collect.fields[i].note) ? "" : $"/// <summary>{collect.fields[i].note}</summary>");
                codeText.Append(fieldCode);
            }
            codeText.AppendLine();
            codeText.Append(codes[3]);
            if (hasns) codeText.Append(codes[4]);
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
            var path = data.SavePath(collect.savePathInx);
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
            var gameObject = Selection.activeGameObject;
            if (gameObject == null)
                return;
            if (!gameObject.TryGetComponent<FieldCollection>(out var fieldCollection))
                return;
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
                    return;
                if (!type.IsSubclassOf(typeof(Component)))
                    return;
                if (fieldCollection.TryGetComponent(type, out var component))
                {
                    var onValidateMethod = component.GetType().GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    onValidateMethod.Invoke(component, null);
                    return;
                }
                fieldCollection.gameObject.AddComponent(type);
            }
        }
    }
}
#endif