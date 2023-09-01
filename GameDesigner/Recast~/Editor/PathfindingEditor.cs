#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Net.AI 
{
    [CustomEditor(typeof(Pathfinding))]
    public class PathfindingEditor : Editor
    {
        private Pathfinding pathfinding;

        private void OnEnable()
        {
            pathfinding = target as Pathfinding;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Bake"))
            {
                pathfinding.Bake();
            }
            if (GUILayout.Button("BakeTerrain"))
            {
                pathfinding.BakeTerrain();
            }
            if (GUILayout.Button("Save"))
            {
                pathfinding.Save();
            }
            if (GUILayout.Button("SaveTerrainMesh"))
            {
                pathfinding.SaveTerrainMesh();
            }
            if (GUILayout.Button("Load"))
            {
                pathfinding.Load();
            }
        }
    }
}
#endif