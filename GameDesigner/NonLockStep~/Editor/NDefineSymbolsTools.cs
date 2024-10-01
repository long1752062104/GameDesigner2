#if UNITY_EDITOR
using UnityEditor;

namespace NonLockStep
{
    public class StateMachineDefineSymbolsTools
    {
        [MenuItem("GameDesigner/NonLockStep/启用一致性软浮点数")]
        private static void EnableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (!defineSymbols.Contains("NON_LOCK_STEP"))
                defineSymbols += ";NON_LOCK_STEP";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }

        [MenuItem("GameDesigner/NonLockStep/关闭一致性软浮点数")]
        private static void DisableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (defineSymbols.Contains("NON_LOCK_STEP"))
                defineSymbols = defineSymbols.Replace("NON_LOCK_STEP", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }
    }
}
#endif