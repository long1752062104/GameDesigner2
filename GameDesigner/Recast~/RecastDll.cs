using System;
using System.Runtime.InteropServices;

namespace Net.AI
{
    public enum SamplePartitionType
    {
        SAMPLE_PARTITION_WATERSHED,
        SAMPLE_PARTITION_MONOTONE,
        SAMPLE_PARTITION_LAYERS
    };

    [Serializable]
    public struct BuildSettings
    {
        // Cell size in world units
        public float cellSize;
        // Cell height in world units
        public float cellHeight;
        // Agent height in world units
        public float agentHeight;
        // Agent radius in world units
        public float agentRadius;
        // Agent max climb in world units
        public float agentMaxClimb;
        // Agent max slope in degrees
        public float agentMaxSlope;
        // Region minimum size in voxels.
        // regionMinSize = sqrt(regionMinArea)
        public float regionMinSize;
        // Region merge size in voxels.
        // regionMergeSize = sqrt(regionMergeArea)
        public float regionMergeSize;
        // Edge max length in world units
        public float edgeMaxLen;
        // Edge max error in voxels
        public float edgeMaxError;
        public float vertsPerPoly;
        // Detail sample distance in voxels
        public float detailSampleDist;
        // Detail sample max error in voxel heights.
        public float detailSampleMaxError;
        // Partition type, see SamplePartitionType
        public SamplePartitionType partitionType;
        // Bounds of the area to mesh
        public float[] navMeshBMin;
        public float[] navMeshBMax;
        // Size of the tiles in voxels
        public float tileSize;

        public static BuildSettings Default => new BuildSettings()
        {
            cellSize = 0.3f,
            cellHeight = 0.2f,
            agentHeight = 2.0f,
            agentRadius = 0.6f,
            agentMaxClimb = 0.9f,
            agentMaxSlope = 45.0f,
            regionMinSize = 8,
            regionMergeSize = 20,
            edgeMaxLen = 12.0f,
            edgeMaxError = 1.3f,
            vertsPerPoly = 6.0f,
            detailSampleDist = 6.0f,
            detailSampleMaxError = 1.0f,
            partitionType = SamplePartitionType.SAMPLE_PARTITION_WATERSHED,
        };
    }

    public enum UpdateFlags : byte
    {
        DT_CROWD_ANTICIPATE_TURNS = 1,
        DT_CROWD_OBSTACLE_AVOIDANCE = 2,
        DT_CROWD_SEPARATION = 4,
        DT_CROWD_OPTIMIZE_VIS = 8,          ///< Use #dtPathCorridor::optimizePathVisibility() to optimize the agent path.
		DT_CROWD_OPTIMIZE_TOPO = 16         ///< Use dtPathCorridor::optimizePathTopology() to optimize the agent path.
	};

    [Serializable]
    public struct AgentParams
    {
        public float radius;                       ///< Agent radius. [Limit: >= 0]
		public float height;                       ///< Agent height. [Limit: > 0]
		public float maxAcceleration;              ///< Maximum allowed acceleration. [Limit: >= 0]
		public float maxSpeed;                     ///< Maximum allowed speed. [Limit: >= 0]

                                                   /// Defines how close a collision element must be before it is considered for steering behaviors. [Limits: > 0]
        public float collisionQueryRange;

        public float pathOptimizationRange;        ///< The path visibility optimization range. [Limit: > 0]

                                                   /// How aggresive the agent manager should be at avoiding collisions with this agent. [Limit: >= 0]
        public float separationWeight;

        /// Flags that impact steering behavior. (See: #UpdateFlags)
        [NonSerialized]
        public UpdateFlags updateFlags;

        /// The index of the avoidance configuration to use for the agent. 
        /// [Limits: 0 <= value <= #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
        public byte obstacleAvoidanceType;

        /// The index of the query filter used by this agent.
        public byte queryFilterType;

        /// User defined data attached to the agent.
        public IntPtr userData;

        public static AgentParams Default => new AgentParams()
        {
            radius = 0.6f,
            height = 2.0f,
            maxAcceleration = 8.0f,
            maxSpeed = 3.5f,
            collisionQueryRange = /*radius*/0.6f * 12.0f,
            pathOptimizationRange = /*radius*/0.6f * 30.0f,
            updateFlags = UpdateFlags.DT_CROWD_ANTICIPATE_TURNS | UpdateFlags.DT_CROWD_OPTIMIZE_VIS | UpdateFlags.DT_CROWD_OPTIMIZE_TOPO | UpdateFlags.DT_CROWD_OBSTACLE_AVOIDANCE | UpdateFlags.DT_CROWD_SEPARATION,

            obstacleAvoidanceType = 3,
            separationWeight = 2f,
        };
    }

    public struct CrowdAgent
    {
        public float[] npos;     ///< The current agent position. [(x, y, z)]
        //float disp[3];		///< A temporary value used to accumulate agent displacement during iterative collision resolution. [(x, y, z)]
        public float[] dvel;      ///< The desired velocity of the agent. Based on the current path, calculated from scratch each frame. [(x, y, z)]
        //float nvel[3];		///< The desired velocity adjusted by obstacle avoidance, calculated from scratch each frame. [(x, y, z)]
        public float[] vel;       ///< The actual velocity of the agent. The change from nvel -> vel is constrained by max acceleration. [(x, y, z)]


    }

    public class RenderTriangle
    {
        public Vector3[] m_Verts = new Vector3[3];
        public Color[] m_Colors = new Color[3] { Color.white, Color.white, Color.white };

        public RenderTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            m_Verts[0] = a;
            m_Verts[1] = b;
            m_Verts[2] = c;
            for (int i = 0; i < m_Colors.Length; ++i)
            {
                m_Colors[i] = color;
            }
        }
    }

    public enum FindPathMode 
    {
        FindPathStraight,
        FindPathFollow
    }

    public class RecastDll
    {
        /*创建寻路网格实例*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateSoloMesh();

        /*设置构建寻路网格参数*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CollectSettings(IntPtr sample, BuildSettings settings);

        /*加载网格模型.obj*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LoadMeshFile(IntPtr sample, string path);

        /*加载网格模型.obj*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LoadObjText(IntPtr sample, string text, long textLen);

        /*加载网格数据 unity*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LoadMesh(IntPtr sample, float[] m_verts, int[] m_tris, int m_vertCount, int m_triCount);

        /*加载已经烘焙好的网格文件.bin*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LoadNavMesh(IntPtr sample, string path);

        /*保存已经烘焙好的网格文件.bin*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SaveNavMesh(IntPtr sample, string path);

        /*构建寻路网格*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Build(IntPtr sample);

        /*查找直线路径*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void FindPathStraight(IntPtr sample, Vector3 m_spos, Vector3 m_epos, float[] outPoints, out int outPointCount);

        /*查找跟随路径 -- 当出现坡度时不是直线行走, 而是先上坡再下坡*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void FindPathFollow(IntPtr sample, Vector3 m_spos, Vector3 m_epos, float[] outPoints, out int outPointCount);

        /*释放寻路网格实例*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeSoloMesh(IntPtr sample);

        /*初始化代码群组*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitCrowd(IntPtr sample);

        /*添加寻路代理, 返回的是代理索引*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AddAgentDefault(IntPtr sample, Vector3 pos);

        /*添加寻路代理, 返回的是代理索引*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AddAgent(IntPtr sample, Vector3 pos, AgentParams ap);

        /*移除寻路代理*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveAgent(IntPtr sample, int idx);

        /*设置代理移动目标*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMoveTarget(IntPtr sample, int agentIdx, Vector3 p, bool adjust);

        /*每帧更新代理群组*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateTick(IntPtr sample, float dt);

        /*获取代理信息*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void GetAgent(IntPtr sample, int idx, float[] npos, float[] vel, float[] dvel);

        /*获取绘制烘焙网格面*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void GetDrawNavMesh(IntPtr sample, float[] vertsArray, out int vertsCount);

        /*获取绘制烘焙网格顶点长度*/
        [DllImport("RecastDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int GetDrawNavMeshCount(IntPtr sample);
    }
}