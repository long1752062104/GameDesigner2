using Net;
using System;
using System.Collections.Generic;

namespace Net.MMORPG
{
    /// <summary>
    ///  路径点
    /// </summary>
    [Serializable]
    public class RoamingPathData
    {
        public string name;
        /// <summary>
        /// 所有路径点
        /// </summary>
        public List<Vector3> waypoints = new List<Vector3>();
    }
}