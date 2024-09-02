#if UNITY_EDITOR
using UnityEditor;

public class GeneratedCSProjectPostprocessor : AssetPostprocessor
{
    private static string OnGeneratedCSProject(string path, string content)
    {
        return ExternalReferenceTools.ChangeCSProject(path, content);
    }
}
#endif