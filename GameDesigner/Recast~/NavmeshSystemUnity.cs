#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Net.Component;
using UnityEngine.AI;
using UnityEngine.Networking;
using System.Collections;
using System;
#if RECAST_NATIVE
using Net.AI.Native;
using static Net.AI.Native.RecastDll;
#else
using Recast;
using static Recast.RecastGlobal;
#endif

namespace Net.AI
{
    public enum LoadPathMode
    {
        None = 0,
        streamingAssetsPath,
        persistentDataPath,
    }

    public class NavmeshSystemUnity : SingleCase<NavmeshSystemUnity>
    {
        public NavmeshSystem System = new NavmeshSystem();
        public LayerMask bakeLayer;
        public LoadPathMode loadPathMode;
        [SerializeField] private string navMashPath;
        public int vertexCountHorizontal = 100;
        public int vertexCountVertical = 100;
        private Mesh navMesh;
        public bool drawNavmesh = true;
        public bool drawWireNavmesh = true;

        public string NavmeshPath
        {
            get
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(navMashPath))
                    navMashPath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "NavMesh.bin";
#endif
                if (loadPathMode == LoadPathMode.streamingAssetsPath)
                    return Application.streamingAssetsPath + "/" + navMashPath;
                if (loadPathMode == LoadPathMode.persistentDataPath)
                    return Application.persistentDataPath + "/" + navMashPath;
                return navMashPath;
            }
            set { navMashPath = value; }
        }

        public void Start()
        {
            StartCoroutine(Load());
        }

        public string ExportMeshText(Mesh mesh)
        {
            var sw = new StringBuilder();
            foreach (Vector3 vertex in mesh.vertices)
            {
                sw.AppendLine("v " + vertex.x + " " + vertex.y + " " + vertex.z);
            }

            foreach (Vector3 normal in mesh.normals)
            {
                sw.AppendLine("vn " + normal.x + " " + normal.y + " " + normal.z);
            }

            foreach (Vector2 uv in mesh.uv)
            {
                sw.AppendLine("vt " + uv.x + " " + uv.y);
            }

            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                int[] triangles = mesh.GetTriangles(submesh);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sw.AppendLine("f " + (triangles[i] + 1) + "/" + (triangles[i] + 1) + "/" + (triangles[i] + 1) +
                                 " " + (triangles[i + 1] + 1) + "/" + (triangles[i + 1] + 1) + "/" + (triangles[i + 1] + 1) +
                                 " " + (triangles[i + 2] + 1) + "/" + (triangles[i + 2] + 1) + "/" + (triangles[i + 2] + 1));
                }
            }

            return sw.ToString();
        }

        public IEnumerator Load()
        {
            System.Init();
#if UNITY_WEBGL
            using (UnityWebRequest request = UnityWebRequest.Get(NavmeshPath)) //web不需要file///
#else
            using (UnityWebRequest request = UnityWebRequest.Get("file:///" + NavmeshPath))
#endif
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (!System.LoadNavmesh(request.downloadHandler.data))
                        throw new Exception($"加载寻路网格数据失败! path:{NavmeshPath}");
                    UpdateNavMeshFace();
                }
                else
                {
                    Debug.LogError("加载寻路网格文件失败: " + request.error);
                }
            }
        }

        public void LoadMeshObj()
        {
            System.Init();
            LoadMeshFile(System.Sample, NavmeshPath);
            Build(System.Sample);
            UpdateNavMeshFace();
        }

        public void Save()
        {
            System.Init();
            SaveNavMesh(System.Sample, NavmeshPath);
        }

        public void Bake()
        {
            var mesh = Merge();
            var objText = ExportMeshText(mesh);
            System.Init();
            LoadMeshData(System.Sample, objText);
            Build(System.Sample);
            UpdateNavMeshFace();
        }

        public void ReadUnityNavmesh()
        {
            var triangulation = NavMesh.CalculateTriangulation();
            var vertices = triangulation.vertices;
            var triangles = triangulation.indices;
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var objText = ExportMeshText(mesh);
            DestroyImmediate(mesh);
            System.Init();
            var buildSettings = System.buildSettings;
            buildSettings.agentRadius = 0f; //读取unity的烘焙数据时, 不需要留边缘, 因为unity已经留边缘
            System.SetSettings(buildSettings);
            LoadMeshData(System.Sample, objText);
            Build(System.Sample);
            UpdateNavMeshFace();
        }

        public void SaveMeshObj()
        {
            var mesh = Merge();
            var objText = ExportMeshText(mesh);
            File.WriteAllText(NavmeshPath, objText);
        }

        public void SaveUnityNavmeshObj()
        {
            var triangulation = NavMesh.CalculateTriangulation();
            var vertices = triangulation.vertices;
            var triangles = triangulation.indices;
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var objText = ExportMeshText(mesh);
            File.WriteAllText(NavmeshPath, objText);
        }

        private unsafe void UpdateNavMeshFace()
        {
#if UNITY_EDITOR //编译后这里没用了
            int vertsCount = GetDrawNavMeshCount(System.Sample);
            float* vertsArray = stackalloc float[vertsCount];
            GetDrawNavMesh(System.Sample, vertsArray, out vertsCount);
            var m_Triangles = new List<RenderTriangle>();
            var col = new Color(0f, 1f, 1f, 1f);
            for (int i = 0; i < vertsCount; i += 9)
            {
                var a = new UnityEngine.Vector3(vertsArray[i + 0], vertsArray[i + 1], vertsArray[i + 2]);
                var b = new UnityEngine.Vector3(vertsArray[i + 3], vertsArray[i + 4], vertsArray[i + 5]);
                var c = new UnityEngine.Vector3(vertsArray[i + 6], vertsArray[i + 7], vertsArray[i + 8]);
                m_Triangles.Add(new RenderTriangle(a, b, c, col));
            }
            if (navMesh != null)
                DestroyImmediate(navMesh, true);
            navMesh = new Mesh();
            int triCount = m_Triangles.Count;
            var verts = new UnityEngine.Vector3[3 * triCount];
            var tris = new int[3 * triCount];
            var colors = new UnityEngine.Color[3 * triCount];
            for (int i = 0; i < triCount; ++i)
            {
                var tri = m_Triangles[i];
                int v = i * 3;
                for (int j = 0; j < 3; ++j)
                {
                    verts[v + j] = tri.m_Verts[j];
                    tris[v + j] = v + j;
                    colors[v + j] = tri.m_Colors[j];
                }
            }
            navMesh.vertices = verts;
            navMesh.triangles = tris;
            navMesh.colors = colors;
            navMesh.RecalculateNormals();
#endif
        }

        private Mesh Merge()
        {
            var meshFilters = FindObjectsOfType<MeshFilter>().Where(mf => ((1 << mf.gameObject.layer) & bakeLayer) > 0).ToArray();
            var mergedMesh = new Mesh();
            var combine = new CombineInstance[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }
            mergedMesh.CombineMeshes(combine);
            return mergedMesh;
        }

        public List<Vector3> GetPath(Vector3 currPosition, Vector3 destination, float agentHeight = 1f, FindPathMode pathMode = FindPathMode.FindPathStraight)
        {
            var paths = new List<Vector3>();
            GetPath(currPosition, destination, paths, agentHeight, pathMode);
            return paths;
        }

        public void GetPath(Vector3 currPosition, Vector3 destination, List<Vector3> paths, float agentHeight = 1f, FindPathMode pathMode = FindPathMode.FindPathStraight)
        {
            System.GetPath(currPosition, destination, paths, agentHeight, pathMode);
        }

        private void OnDrawGizmos()
        {
            if (navMesh == null)
                return;
            if (drawNavmesh)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
                Gizmos.DrawMesh(navMesh);
            }
            if (drawWireNavmesh)
            {
                Gizmos.color = new Color(0f, 0f, 0f, 0.3f);
                Gizmos.DrawWireMesh(navMesh);
            }
        }

        private void OnDestroy()
        {
            System.Free();
        }
    }
}
#endif