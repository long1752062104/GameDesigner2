using Newtonsoft_X.Json;
using System.IO;

/// <summary>
/// 持久化数据记录帮助类
/// </summary>
public class PersistHelper
{
    public static T Deserialize<T>(string name) where T : class, new()
    {
#if UNITY_EDITOR
        var path = "ProjectSettings/gdnet/";
#else
        var path = Net.Config.Config.BasePath + "/ProjectSettings/gdnet/";
#endif
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var file = path + name;
        if (!File.Exists(file))
            return new T();
        var jsonStr = File.ReadAllText(file);
        return JsonConvert.DeserializeObject<T>(jsonStr, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace });
    }

    public static void Serialize<T>(T obj, string name)
    {
#if UNITY_EDITOR
        var path = "ProjectSettings/gdnet/";
#else
        var path = Net.Config.Config.BasePath + "/ProjectSettings/gdnet/";
#endif
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var file = path + name;
        var jsonStr = JsonConvert.SerializeObject(obj);
        File.WriteAllText(file, jsonStr);
    }
}
