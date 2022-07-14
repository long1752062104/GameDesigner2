using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Config
{
    public static class NetConfig
    {
        public static ConfigData Config;

        static NetConfig() 
        {
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
#if UNITY_STANDALONE || UNITY_WSA
            var streamingAssetsPath = UnityEngine.Application.streamingAssetsPath;
            if (!Directory.Exists(streamingAssetsPath))
                Directory.CreateDirectory(streamingAssetsPath);
            var path = streamingAssetsPath;
#else
            var path = UnityEngine.Application.persistentDataPath;
#endif
#else
            var path = AppDomain.CurrentDomain.BaseDirectory;
#endif
            var configPath = path + $"/network.config";
            if (File.Exists(configPath))
            {
                var jsonStr = File.ReadAllText(configPath);
                Config = Newtonsoft_X.Json.JsonConvert.DeserializeObject<ConfigData>(jsonStr);
            }
            else 
            {
                Config = new ConfigData();
                var jsonStr = Newtonsoft_X.Json.JsonConvert.SerializeObject(Config);
                File.WriteAllText(configPath, jsonStr);
            }
        }
    }

    public class ConfigData
    {
        /// <summary>
        /// 使用内存流进行缓存? 默认是文件流缓存, 速度会比较慢, 运行内存占用比较少!
        /// 使用内存流缓存速度会比较快, 但运行内存占用比较大
        /// </summary>
        public bool UseMemoryStream { get; set; }
    }
}
