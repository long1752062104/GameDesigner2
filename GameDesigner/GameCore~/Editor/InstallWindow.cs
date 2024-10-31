using Net.Helper;
using Net.Share;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameCore
{
    public class InstallWindow : EditorWindow
    {
        private Data data;

        public class Data
        {
            public string gameCorePath = "Assets/Plugins/GameDesigner";
            public string scriptPath = "Assets/Scripts";
            public string resourcePath = "Assets/Arts";
            public string excelScriptEx = "Assets/Scripts/Data/ConfigEx";
        }

        [MenuItem("GameDesigner/GameCore/Install", priority = 1)]
        static void ShowWindow()
        {
            var window = GetWindow<InstallWindow>("游戏框架安装");
            window.Show();
        }

        private void OnEnable()
        {
            LoadData();
        }

        private void OnDisable()
        {
            SaveData();
        }

        void LoadData()
        {
            data = PersistHelper.Deserialize<Data>("gameCoreData.json");
        }

        void SaveData()
        {
            PersistHelper.Serialize(data, "gameCoreData.json");
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            data.gameCorePath = EditorGUILayout.TextField("框架路径:", data.gameCorePath);
            data.scriptPath = EditorGUILayout.TextField("脚本路径:", data.scriptPath);
            data.resourcePath = EditorGUILayout.TextField("资源路径:", data.resourcePath);
            data.excelScriptEx = EditorGUILayout.TextField("Excel脚本扩展路径:", data.excelScriptEx);
            if (GUILayout.Button("安装", GUILayout.Height(30f)))
                InstallStep1();
            if (EditorGUI.EndChangeCheck())
                SaveData();
        }

        private void InstallStep1()
        {
            var path = "Tools/Excel/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var excelPath = $"{data.gameCorePath}/GameCore/Template/GameConfig.xlsx";
            var excelPath1 = path + "GameConfig.xlsx";
            if (!File.Exists(excelPath1))//如果存在表则不能复制进去了, 避免使用者数据丢失
                File.Copy(excelPath, excelPath1);
            Debug.Log($"复制配置表格文件完成:{excelPath1}");

            var paths = new List<string>()
            {
                $"{data.scriptPath}/Data/DB/", $"{data.scriptPath}/Data/DBExt/", $"{data.scriptPath}/Data/Proto/",
                $"{data.scriptPath}/Data/Binding/", $"{data.scriptPath}/Data/BindingExt/",
                $"{data.scriptPath}/Data/Config", $"{data.scriptPath}/Data/ConfigEx", $"{data.scriptPath}/GameCoreEx",
                $"{data.resourcePath}/Audio", $"{data.resourcePath}/Prefabs", $"{data.resourcePath}/UI", $"{data.resourcePath}/Table",
            };

            foreach (var item in paths)
            {
                if (!Directory.Exists(item))
                    Directory.CreateDirectory(item);
                Debug.Log($"创建的脚本路径:{item}");
            }

            path = $"{data.gameCorePath}/GameCore/Template/Global.txt";
            excelPath1 = $"{data.scriptPath}/GameCoreEx/Global.cs";
            if (!File.Exists(excelPath1))
                File.Copy(path, excelPath1);

            path = $"{data.gameCorePath}/GameCore/Template/UIManager.txt";
            excelPath1 = $"{data.scriptPath}/GameCoreEx/UIManager.cs";
            if (!File.Exists(excelPath1))
                File.Copy(path, excelPath1);

            path = $"{data.gameCorePath}/GameCore/Template/TableManager.txt";
            excelPath1 = $"{data.scriptPath}/GameCoreEx/TableManager.cs";
            if (!File.Exists(excelPath1))
                File.Copy(path, excelPath1);

            path = $"{data.gameCorePath}/GameCore/Template/ResourcesManager.txt";
            excelPath1 = $"{data.scriptPath}/GameCoreEx/ResourcesManager.cs";
            if (!File.Exists(excelPath1))
                File.Copy(path, excelPath1);

            path = $"{data.gameCorePath}/GameCore/Template/AssetBundleCheckUpdate.txt";
            excelPath1 = $"{data.scriptPath}/GameCoreEx/AssetBundleCheckUpdate.cs";
            if (!File.Exists(excelPath1))
                File.Copy(path, excelPath1);

            path = $"{data.gameCorePath}/GameCore/Template/EventCommand.txt";
            excelPath1 = $"{data.scriptPath}/Data/EventCommand.cs";
            if (!File.Exists(excelPath1))
                File.Copy(path, excelPath1);

            path = $"{data.gameCorePath}/GameCore/Template/OPCommand.txt";
            excelPath1 = $"{data.scriptPath}/Data/OPCommand.cs";
            if (!File.Exists(excelPath1))
                File.Copy(path, excelPath1);

            AssetDatabase.Refresh();
            if (EditorApplication.isCompiling)
                EditorPrefs.SetBool("GameCoreInstall", true);
            else
                InstallStep2();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnReload()
        {
            if (EditorPrefs.HasKey("GameCoreInstall"))
            {
                var isCompiling = EditorPrefs.GetBool("GameCoreInstall");
                if (isCompiling)
                {
                    var window = GetWindow<InstallWindow>("游戏框架安装");
                    if (window != null)
                        window.InstallStep2();
                }
                EditorPrefs.DeleteKey("GameCoreInstall");
            }
        }

        private void InstallStep2()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            GameObject globalObj = null;
            foreach (var root in roots)
            {
                var globalComponent = root.GetComponentInChildren<Global>();
                if (globalComponent != null)
                {
                    globalObj = globalComponent.gameObject;
                    break;
                }
            }
            if (globalObj == null)
            {
                var path = $"{data.gameCorePath}/GameCore/Prefabs/Global.prefab";
                var globalAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                globalObj = PrefabUtility.InstantiatePrefab(globalAsset) as GameObject;
                PrefabUtility.UnpackPrefabInstance(globalObj, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }
            var global = globalObj.GetComponent<Global>();
            global.platform = (Platform)EditorUserBuildSettings.activeBuildTarget;

            var assetBundleCheckUpdate = globalObj.GetComponent<AssetBundleCheckUpdate>();
            assetBundleCheckUpdate.url = $"http://{NetPort.GetIP()}/";
            assetBundleCheckUpdate.metadataList = $"{data.resourcePath}/Hotfix/MetadataList.bytes";
            assetBundleCheckUpdate.hotfixDll = $"{data.resourcePath}/Hotfix/Main.dll.bytes";

            var tableManager = globalObj.GetComponent<TableManager>();
            tableManager.tablePath = $"{data.resourcePath}/Table/GameConfig.bytes";

            ModifyScript(globalObj.GetComponent<Global>(), $"{data.scriptPath}/GameCoreEx/Global.cs");
            ModifyScript(globalObj.GetComponent<UIManager>(), $"{data.scriptPath}/GameCoreEx/UIManager.cs");
            ModifyScript(globalObj.GetComponent<TableManager>(), $"{data.scriptPath}/GameCoreEx/TableManager.cs");
            ModifyScript(globalObj.GetComponent<ResourcesManager>(), $"{data.scriptPath}/GameCoreEx/ResourcesManager.cs");
            ModifyScript(globalObj.GetComponent<AssetBundleCheckUpdate>(), $"{data.scriptPath}/GameCoreEx/AssetBundleCheckUpdate.cs");

            EditorUtility.SetDirty(globalObj);

            Debug.Log($"环境安装完成!");
        }

        private void ModifyScript(MonoBehaviour monoBehaviour, string monoScriptPath)
        {
            var serializedObject = new SerializedObject(monoBehaviour);
            var scriptProperty = serializedObject.FindProperty("m_Script");
            scriptProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<MonoScript>(monoScriptPath);
            serializedObject.ApplyModifiedProperties();
        }
    }
}