using System;

namespace Net.MMORPG
{
    /// <summary>
    /// 地图怪物点
    /// </summary>
    [Serializable]
    public class MapMonsterPoint
    {
        public string name;
        /// <summary>
        /// 所有怪物数据
        /// </summary>
        public MonsterData[] monsters;
        /// <summary>
        /// 怪物巡逻点
        /// </summary>
        public PatrolPath patrolPath;

        public override string ToString()
        {
            return $"Name:{name} monsters:{monsters.Length} patrolPath:{patrolPath.waypoints.Count}";
        }
    }
}
