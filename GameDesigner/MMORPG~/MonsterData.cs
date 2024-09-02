using System;
using Unity;

namespace Net.MMORPG
{
    /// <summary>
    /// 怪物数据
    /// </summary>
    [Serializable]
    public class MonsterData
    {
        /// <summary>
        /// 怪物索引
        /// </summary>
        public int id;
        /// <summary>
        /// 怪物生命值
        /// </summary>
        public int health;
        /// <summary>
        /// 可以赋值更多信息
        /// </summary>
        public string json;
        /// <summary>
        /// 记录更多字段信息
        /// </summary>
        public Metadata[] fields;

        // 暂时保留字段
        public int wanderType, dialougeID, Level;

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetValue<T>(string name)
        {
            for (int i = 0; i < fields.Length; i++)
                if (fields[i].name == name)
                    return (T)fields[i].Read();
            return default;
        }
    }
}