#if UNITY_EDITOR
using UnityEditor;

namespace NonLockStep
{
    public class StateMachineDefineSymbolsTools
    {
        [MenuItem("GameDesigner/NonLockStep/启用全局软浮点数", priority = 1)]
        private static void EnableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (!defineSymbols.Contains("NON_LOCK_STEP"))
                defineSymbols += ";NON_LOCK_STEP";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }

        [MenuItem("GameDesigner/NonLockStep/关闭全局软浮点数", priority = 2)]
        private static void DisableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (defineSymbols.Contains("NON_LOCK_STEP"))
                defineSymbols = defineSymbols.Replace("NON_LOCK_STEP", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }

        [MenuItem("GameDesigner/NonLockStep/启用Jitter2物理", priority = 3)]
        private static void EnableJitter2Physics()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (!defineSymbols.Contains("JITTER2_PHYSICS"))
                defineSymbols += ";JITTER2_PHYSICS";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }

        [MenuItem("GameDesigner/NonLockStep/关闭Jitter2物理", priority = 4)]
        private static void DisableJitter2Physics()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (defineSymbols.Contains("JITTER2_PHYSICS"))
                defineSymbols = defineSymbols.Replace("JITTER2_PHYSICS", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }
    }
}
#endif