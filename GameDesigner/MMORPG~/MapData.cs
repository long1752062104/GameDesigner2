﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Net.MMORPG
{
    /// <summary>
    /// 地图九宫格初始化数据
    /// </summary>
    [Serializable]
    public class MapAOIData
    {
        /// <summary>
        /// x开始位置
        /// </summary>
        public float xPos;
        /// <summary>
        /// z开始位置
        /// </summary>
        public float zPos;
        /// <summary>
        /// x列最大值
        /// </summary>
        public uint xMax;
        /// <summary>
        /// z列最大值
        /// </summary>
        public uint zMax;
        /// <summary>
        /// 格子宽度
        /// </summary>
        public int width;
        /// <summary>
        /// 格子高度
        /// </summary>
        public int height;
    }
    
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
        ///  简单在路径点，可以用作服务器NPC位置，玩家出生位置等等
        /// </summary>
        public List<RoamingPathData> roamingPaths = new List<RoamingPathData>();
        /// <summary>
        /// 地图九宫格数据
        /// </summary>
        public MapAOIData aoiData = new MapAOIData();
        /// <summary>
        /// 寻路网格文件路径
        /// </summary>
        public string navmeshPath;

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
                if (monsterPoint != null)
                {
                    sceneData.monsterPoints.Add(new MapMonsterPoint()
                    {
                        name = item.name,
                        patrolPath = new RoamingPathData() { name = item.Name, waypoints = item.WorldPointList },
                        monsters = monsterPoint.monsters,
                    });
                }
                else
                {
                    sceneData.roamingPaths.Add( new RoamingPathData() { name = item.Name, waypoints = item.WorldPointList });
                }
            }
            var aoiMgr = UnityEngine.Object.FindObjectOfType<Component.AOIManager>();
            if (aoiMgr != null)
            {
                sceneData.aoiData = new MapAOIData()
                {
                    xPos = aoiMgr.xPos,
                    zPos = aoiMgr.zPos,
                    xMax = aoiMgr.xMax,
                    zMax = aoiMgr.zMax,
                    width = aoiMgr.width,
                    height = aoiMgr.height,
                };
            }
            var navmeshSystemUnityType = Helper.AssemblyHelper.GetTypeNotOptimized("Net.AI.NavmeshSystemUnity");
            if (navmeshSystemUnityType != null)
            {
                var navmesh = UnityEngine.Object.FindObjectOfType(navmeshSystemUnityType);
                if (navmesh != null)
                {
                    var navmeshPath = navmeshSystemUnityType.GetProperty("NavmeshPath");
                    if (navmeshPath != null)
                    {
                        sceneData.navmeshPath = navmeshPath.GetValue(navmesh) as string;
                    }
                }
            }
            var jsonStr = Newtonsoft_X.Json.JsonConvert.SerializeObject(sceneData);
            File.WriteAllText(path, jsonStr);
            UnityEngine.Debug.Log($"地图数据生成成功: {new DirectoryInfo(path).FullName}");
#else
            Net.Event.NDebug.LogError("只有Unity编辑器能用!!!");
#endif
        }
    }
}