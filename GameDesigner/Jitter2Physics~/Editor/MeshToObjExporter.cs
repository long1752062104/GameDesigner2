using System.IO;
using UnityEngine;

public class MeshToObjExporter
{
    public static void ExportMeshToObj(Mesh mesh, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // 写入顶点
            foreach (Vector3 vertex in mesh.vertices)
            {
                writer.WriteLine($"v {vertex.x} {vertex.y} {vertex.z}");
            }

            // 写入法线（可选）
            foreach (Vector3 normal in mesh.normals)
            {
                writer.WriteLine($"vn {normal.x} {normal.y} {normal.z}");
            }

            // 写入纹理坐标（可选）
            foreach (Vector2 uv in mesh.uv)
            {
                writer.WriteLine($"vt {uv.x} {uv.y}");
            }

            // 写入面（使用三角形索引）
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                int index1 = mesh.triangles[i] + 1;   // OBJ 索引从 1 开始
                int index2 = mesh.triangles[i + 1] + 1;
                int index3 = mesh.triangles[i + 2] + 1;
                writer.WriteLine($"f {index1} {index2} {index3}");
            }
        }

        Debug.Log($"Mesh exported to: {filePath}");
    }

    [UnityEditor.MenuItem("Tools/Export Selected Mesh to OBJ")]
    public static void ExportSelectedMesh()
    {
        GameObject selected = UnityEditor.Selection.activeGameObject;
        if (selected == null || selected.GetComponent<MeshFilter>() == null)
        {
            Debug.LogError("Please select a GameObject with a MeshFilter.");
            return;
        }

        Mesh mesh = selected.GetComponent<MeshFilter>().sharedMesh;
        string path = UnityEditor.EditorUtility.SaveFilePanel("Save OBJ File", "", selected.name + ".obj", "obj");

        if (!string.IsNullOrEmpty(path))
        {
            ExportMeshToObj(mesh, path);
        }
    }
}