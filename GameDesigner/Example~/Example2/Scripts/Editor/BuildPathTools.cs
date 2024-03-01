using Net.MMORPG;
using System.IO;
using UnityEditor;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class BuildPathTools
{
    [MenuItem("GameDesigner/Example/Example2/BuildSceneData")]
    static void Init()
    {
        var dirs = Directory.GetDirectories(Application.dataPath, "ExampleServer~", SearchOption.AllDirectories);
        if (dirs.Length == 0)
            throw new System.Exception("找不到目录!");
        var scene = SceneManager.GetActiveScene();
        var path = dirs[0] + "/bin/Debug/Data/" + scene.name + ".mapData";
        MapData.WriteData(path);
    }
}