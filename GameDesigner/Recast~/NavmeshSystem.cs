using System;
using System.Collections.Generic;

namespace Net.AI
{
    [Serializable]
    public class NavmeshSystem
    {
        private IntPtr sample;
        public BuildSettings buildSettings = BuildSettings.Default;
        private readonly float[] m_Paths = new float[2048 * 3];
        public IntPtr Sample => sample;

        public void Init()
        {
            if (sample == IntPtr.Zero)
                sample = RecastDll.CreateSoloMesh();
            RecastDll.CollectSettings(sample, buildSettings);
        }

        public void Init(string navmeshPath)
        {
            Init();
            RecastDll.LoadNavMesh(sample, navmeshPath);
        }

        public List<Vector3> GetPath(Vector3 currPosition, Vector3 destination, float agentHeight = 1f, FindPathMode pathMode = FindPathMode.FindPathStraight)
        {
            var paths = new List<Vector3>();
            GetPath(currPosition, destination, paths, agentHeight, pathMode);
            return paths;
        }

        public void GetPath(Vector3 currPosition, Vector3 destination, List<Vector3> paths, float agentHeight = 1f, FindPathMode pathMode = FindPathMode.FindPathStraight)
        {
            paths.Clear();
            int outPointCount;
            if (pathMode == FindPathMode.FindPathStraight)
                RecastDll.FindPathStraight(sample, currPosition, destination, m_Paths, out outPointCount);
            else
                RecastDll.FindPathFollow(sample, currPosition, destination, m_Paths, out outPointCount);
            for (int i = 1; i < outPointCount; i++) //为什么不能要最后一条线? 因为后面一条线偶尔出现y=1的问题, 最后一个不要也不影响
            {
                int v = i * 3;
                paths.Add(new Vector3(m_Paths[v - 3], m_Paths[v - 2] + agentHeight, m_Paths[v - 1])); //a线
                paths.Add(new Vector3(m_Paths[v + 0], m_Paths[v + 1] + agentHeight, m_Paths[v + 2])); //b线
            }
        }

        public void Free()
        {
            if (sample != IntPtr.Zero)
            {
                RecastDll.FreeSoloMesh(sample);
                sample = IntPtr.Zero;
            }
        }
    }
}