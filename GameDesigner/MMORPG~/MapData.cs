using System;
using System.Collections.Generic;
using System.IO;

namespace Net.MMORPG
{
    /// <summary>
    /// 地图数据
    /// </summary>
    [Serializable]
    public class MapData
    {
        /// <summary>
        /// 地图名称
        /// </summary>
        public string name;
        /// <summary>
        /// 地图的所有怪物点
        /// </summary>
        public List<MapMonsterPoint> monsterPoints = new List<MapMonsterPoint>();

        /// <summary>
        /// 读取地图数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static MapData ReadData(string path)
        {
            var jsonStr = File.ReadAllText(path);
            return Newtonsoft_X.Json.JsonConvert.DeserializeObject<MapData>(jsonStr);
        }

        /// <summary>
        /// 写入地图数据 --只有unity编辑器使用
        /// </summary>
        /// <param name="path"></param>
        public static void WriteData(string path)
        {
#if UNITY_EDITOR
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var roamingPaths = UnityEngine.Object.FindObjectsOfType<RoamingPath>();
            var sceneData = new MapData
            {
                name = scene.name
            };
            foreach (var item in roamingPaths)
            {
                var monsterPoint = item.GetComponent<MonsterPoint>();
                sceneData.monsterPoints.Add(new MapMonsterPoint()
                {
                    patrolPath = new PatrolPath() { waypoints = item.waypointsList.ConvertAll(x => (Vector3)x) },
                    monsters = monsterPoint.monsters,
                });
            }
            var jsonStr = Newtonsoft_X.Json.JsonConvert.SerializeObject(sceneData);
            File.WriteAllText(path, jsonStr);
            UnityEngine.Debug.Log($"地图数据生成成功!--{path}");
#else
            Net.Event.NDebug.LogError("只有Unity编辑器能用!!!");
#endif
        }
    }
}
