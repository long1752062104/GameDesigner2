#if UNITY_EDITOR
using UnityEditor;

namespace LockStep
{
    public class StateMachineDefineSymbolsTools
    {
        [MenuItem("GameDesigner/LockStep/启用一致性软浮点数")]
        private static void EnableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (!defineSymbols.Contains("LOCK_STEP"))
                defineSymbols += ";LOCK_STEP";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }

        [MenuItem("GameDesigner/LockStep/关闭一致性软浮点数")]
        private static void DisableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (defineSymbols.Contains("LOCK_STEP"))
                defineSymbols = defineSymbols.Replace("LOCK_STEP", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }
    }
}
#endif