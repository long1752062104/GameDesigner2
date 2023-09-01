#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using AmazingAssets.TerrainToMesh;
using Net.Component;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Net.AI
{
    public class Pathfinding : SingleCase<Pathfinding>
    {
        public BuildSettings buildSettings = BuildSettings.Default;
        public LayerMask bakeLayer;
        public string navMashPath;
        public int vertexCountHorizontal = 100;
        public int vertexCountVertical = 100;
        internal IntPtr sample;
        private readonly float[] m_Paths = new float[2048 * 3];
        private Mesh navMesh;
        public bool drawNavmesh = true;
        public bool drawWireNavmesh = true;

        void Start()
        {
            Load();
        }

        public void ExportMesh(string filePath, Mesh mesh)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    sw.WriteLine("v " + vertex.x + " " + vertex.y + " " + vertex.z);
                }

                foreach (Vector3 normal in mesh.normals)
                {
                    sw.WriteLine("vn " + normal.x + " " + normal.y + " " + normal.z);
                }

                foreach (Vector2 uv in mesh.uv)
                {
                    sw.WriteLine("vt " + uv.x + " " + uv.y);
                }

                for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
                {
                    int[] triangles = mesh.GetTriangles(submesh);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        sw.WriteLine("f " + (triangles[i] + 1) + "/" + (triangles[i] + 1) + "/" + (triangles[i] + 1) +
                                     " " + (triangles[i + 1] + 1) + "/" + (triangles[i + 1] + 1) + "/" + (triangles[i + 1] + 1) +
                                     " " + (triangles[i + 2] + 1) + "/" + (triangles[i + 2] + 1) + "/" + (triangles[i + 2] + 1));
                    }
                }
            }
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

        public void LoadMesh() 
        {
            var mesh = Merge();

            //ExportMesh(@"D:\recastnavigation_share\RecastDemo\Bin\Meshes\Merge.obj", mesh);
            var objText = ExportMeshText(mesh);

            sample = RecastDll.CreateSoloMesh();
            RecastDll.CollectSettings(sample, BuildSettings.Default);

            float[] m_verts = new float[mesh.vertices.Length * 3];
            for (int i = 0; i < mesh.vertices.Length; i += 3)
            {
                m_verts[i + 0] = mesh.vertices[i].x;
                m_verts[i + 1] = mesh.vertices[i].y;
                m_verts[i + 2] = mesh.vertices[i].z;
            }

            var m_triangles = mesh.triangles;

            //var r = RecastDll.LoadMesh(sample, m_verts, m_triangles, mesh.vertexCount, m_triangles.Length / 3);
            //var r = RecastDll.LoadMeshFile(sample, @"D:\\recastnavigation_share\\RecastDemo\\Bin\\Meshes\\dungeon.obj");
            //var r = RecastDll.LoadMeshFile(sample, @"D:\recastnavigation_share\RecastDemo\Bin\Meshes\Merge.obj");
            var r = RecastDll.LoadObjText(sample, objText, objText.Length);
            var r1 = RecastDll.Build(sample);

            //var r2 = RecastDll.LoadNavMesh(sample, "D:\\recastnavigation_share\\RecastDemo\\Bin\\solo_navmesh.bin");

            //RecastDll.InitCrowd(sample);


            float[] vertsArray = new float[65536];
            RecastDll.GetDrawNavMesh(sample, vertsArray, out int vertsCount);

            var m_Triangles = new List<RenderTriangle>();

            {
                var verts = new List<UnityEngine.Vector3>();
                for (int i = 0; i < vertsCount; i += 3)
                {
                    verts.Add(new UnityEngine.Vector3(vertsArray[i + 0], vertsArray[i + 1], vertsArray[i + 2]));
                }

                Color col = new Color(0f, 1f, 1f, 1f);
                for (int i = 0; i < verts.Count; i += 3)
                {
                    m_Triangles.Add(new RenderTriangle(verts[i + 0], verts[i + 1], verts[i + 2], col));
                }
            }

            {
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
            }

        }

        public void Load() 
        {
            if (sample == IntPtr.Zero)
                sample = RecastDll.CreateSoloMesh();
            RecastDll.CollectSettings(sample, buildSettings);
            RecastDll.LoadNavMesh(sample, navMashPath);
            UpdateNavMeshFace();
        }

        public void Save()
        {
            RecastDll.SaveNavMesh(sample, navMashPath);
        }

        public void Bake() 
        {
            var mesh = Merge();
            var objText = ExportMeshText(mesh);
            if (sample == IntPtr.Zero)
                sample = RecastDll.CreateSoloMesh();
            RecastDll.CollectSettings(sample, buildSettings);
            RecastDll.LoadObjText(sample, objText, objText.Length);
            RecastDll.Build(sample);
            UpdateNavMeshFace();
        }

        private void UpdateNavMeshFace()
        {
            int vertsCount = RecastDll.GetDrawNavMeshCount(sample);
            var vertsArray = new float[vertsCount];
            RecastDll.GetDrawNavMesh(sample, vertsArray, out vertsCount);
            var m_Triangles = new List<RenderTriangle>();
            var col = new Color(0f, 1f, 1f, 1f);
            for (int i = 0; i < vertsCount; i += 9)
            {
                var a = new UnityEngine.Vector3(vertsArray[i + 0], vertsArray[i + 1], vertsArray[i + 2]);
                var b = new UnityEngine.Vector3(vertsArray[i + 3], vertsArray[i + 4], vertsArray[i + 5]);
                var c = new UnityEngine.Vector3(vertsArray[i + 6], vertsArray[i + 7], vertsArray[i + 8]);
                m_Triangles.Add(new RenderTriangle(a, b, c, col));
            }
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
        }

        private Mesh Merge()
        {
            var meshFilters = FindObjectsOfType<MeshFilter>(true).Where(mf => ((1 << mf.gameObject.layer) & bakeLayer) > 0).ToArray();
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

        public void BakeTerrain() 
        {
            var terrain = FindObjectOfType<Terrain>();
            var mesh = terrain.terrainData.TerrainToMesh().ExportMesh(vertexCountHorizontal, vertexCountVertical, Normal.CalculateFromMesh);//AddTerrain(terrain);
            var path = Path.GetTempFileName();
            ExportMesh(path, mesh);
            //var objText = ExportMeshText(mesh);
            if (sample == IntPtr.Zero)
                sample = RecastDll.CreateSoloMesh();
            RecastDll.CollectSettings(sample, buildSettings);
            var objText = File.ReadAllText(path);
            RecastDll.LoadObjText(sample, objText, objText.Length);
            RecastDll.Build(sample);
            UpdateNavMeshFace();
        }

        public void SaveTerrainMesh() 
        {
            var terrain = FindObjectOfType<Terrain>();
            var mesh = terrain.terrainData.TerrainToMesh().ExportMesh(vertexCountHorizontal, vertexCountVertical, Normal.CalculateFromMesh); // AddTerrain(terrain);
            ExportMesh("D:\\recastnavigation_share\\RecastDemo\\Bin\\Meshes\\Terrain.obj", mesh);
        }

        public Mesh AddTerrain(Terrain terrain)
        {
            //Terrain Data
            int terrainWidth = terrain.terrainData.heightmapResolution;
            int terrainHeight = terrain.terrainData.heightmapResolution;
            float[,] terrainData = terrain.terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);

            Vector3 meshScale = new Vector3(terrain.terrainData.size.x / (terrainWidth - 1), terrain.terrainData.size.y, terrain.terrainData.size.z / (terrainHeight - 1));
            terrainWidth--;
            terrainHeight--;

            //Setup m_verts array size
            int vertStart = 0;
            int prevVertCount = 0;
            int newVertCount = terrainWidth * terrainHeight;

            int minTri = 0;
            int maxTri = int.MaxValue;

            var m_verts = new float[newVertCount * 3];

            // Build vertice array
            var vertices = new UnityEngine.Vector3[terrainWidth * terrainHeight];

            for (int y = 0; y < terrainHeight; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    vertices[y * terrainWidth + x] = Vector3.Scale(meshScale, new Vector3(x, terrainData[y, x], y));
                }
            }

            int index = 0;

            // Build triangle indices: 3 indices into vertex array for each triangle
            int[] triangles = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];

            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    // For each grid cell output two triangles
                    triangles[index++] = (y * terrainWidth) + x;
                    triangles[index++] = ((y + 1) * terrainWidth) + x;
                    triangles[index++] = (y * terrainWidth) + x + 1;

                    triangles[index++] = ((y + 1) * terrainWidth) + x;
                    triangles[index++] = ((y + 1) * terrainWidth) + x + 1;
                    triangles[index++] = (y * terrainWidth) + x + 1;
                }
            }

            //Store new triangles into m_tris
            int triStart = 0;
            int newTriCount = triangles.Length;
            var m_tris = triangles;

            for (int i = 0; i < m_tris.Length; ++i)
            {
                minTri = Math.Max(m_tris[i], minTri);
                maxTri = Math.Min(m_tris[i], maxTri);
            }

            //Store vertices into m_verts
            for (int i = 0; i < newVertCount; ++i)
            {
                int v = vertStart + i * 3;
                m_verts[v + 0] = vertices[i].x;
                m_verts[v + 1] = vertices[i].y;
                m_verts[v + 2] = vertices[i].z;
            }

            //Update vert/tri counts
            //m_vertCount += vertices.Length;
            //m_triCount += triangles.Length / 3;

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        public List<Vector3> GetPath(Vector3 currPosition, Vector3 destination)
        {
            var paths = new List<Vector3>();
            GetPath(currPosition, destination, paths);
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
            if (sample != IntPtr.Zero)
            {
                RecastDll.FreeSoloMesh(sample);
                sample = IntPtr.Zero;
            }
        }
    }
}
#endif