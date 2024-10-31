#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameCore
{
    public class Tools
    {
        [MenuItem("CONTEXT/MonoBehaviour/ChangeFieldNames")]
        private static void ChangeFieldNames(MenuCommand menuCommand)
        {
            var target = menuCommand.context;
            var type = target.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!field.FieldType.IsSubclassOf(typeof(Object)))
                    continue;
                var value = field.GetValue(target);
                if (value == null)
                    continue;
                var obj = value as Object;
                obj.name = field.Name;
            }
            EditorUtility.SetDirty(target);
        }

        [MenuItem("GameDesigner/GameCore/OpenGameConfig", priority = 5)]
        private static void OpenExcel()
        {
            var path = "Tools/Excel/GameConfig.xlsx";
            InternalEditorUtility.OpenFileAtLineExternal(path, 0);
        }

        [MenuItem("GameDesigner/GameCore/启用HybridCLR", false, 6)]
        private static void EnableHybridCLR()
        {
            bool isChecked = Menu.GetChecked("GameDesigner/GameCore/启用HybridCLR");
            Menu.SetChecked("GameDesigner/GameCore/启用HybridCLR", !isChecked);
            BuildTargetGroup currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (!isChecked)
            {
                if (!defineSymbols.Contains("HYBRIDCLR"))
                    defineSymbols += ";HYBRIDCLR";
            }
            else
            {
                if (defineSymbols.Contains("HYBRIDCLR"))
                    defineSymbols = defineSymbols.Replace("HYBRIDCLR", "");
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }
    }
}
#endif