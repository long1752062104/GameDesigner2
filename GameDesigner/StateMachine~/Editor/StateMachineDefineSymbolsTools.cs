#if UNITY_EDITOR
using UnityEditor;

namespace GameDesigner
{
    public class StateMachineDefineSymbolsTools
    {
        [MenuItem("GameDesigner/StateMachine/启用Shader网格动画")]
        private static void EnableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (!defineSymbols.Contains("SHADER_ANIMATED"))
                defineSymbols += ";SHADER_ANIMATED";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }

        [MenuItem("GameDesigner/StateMachine/关闭Shader网格动画")]
        private static void DisableShaderMeshAnimated()
        {
            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            if (defineSymbols.Contains("SHADER_ANIMATED"))
                defineSymbols = defineSymbols.Replace("SHADER_ANIMATED", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, defineSymbols);
        }
    }
}
#endif