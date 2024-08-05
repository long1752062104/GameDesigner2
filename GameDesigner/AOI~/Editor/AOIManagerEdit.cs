#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Net.Component;

namespace Net.AOI
{
    [CustomEditor(typeof(AOIManager))]
    public class AOIManagerEdit : Editor
    {
        private AOIManager self;

        private void OnEnable()
        {
            self = target as AOIManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("初始化九宫格"))
            {
                self.InitAOI();
            }
        }
    }
}
#endif