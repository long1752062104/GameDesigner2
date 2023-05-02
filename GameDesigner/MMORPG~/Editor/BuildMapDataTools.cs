#if UNITY_EDITOR
using Net.Helper;
using UnityEditor;
using UnityEngine;

namespace Net.MMORPG 
{
    public class BuildMapDataTools : EditorWindow
    {
        private Data data = new Data();

        [MenuItem("GameDesigner/MMORPG/BuildMapData")]
        public static void Init()
        {
            GetWindow<BuildMapDataTools>("BuildMapData", true);
        }
        private void OnEnable()
        {
            LoadData();
        }
        private void OnDisable()
        {
            SaveData();
        }
        void OnGUI()
        {
            EditorGUILayout.HelpBox("���ɵ�ǰ�򿪳���������, �����Ե�ǰ������Ϊ��ͼ�����ļ���", MessageType.Info);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("����·��:", data.savePath);
            if (GUILayout.Button("ѡ��·��", GUILayout.Width(100)))
            {
                data.savePath = EditorUtility.OpenFolderPanel("��ͼ����·��", "", "");
                SaveData();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("���ɵ�ͼ����", GUILayout.Height(40)))
            {
                if (string.IsNullOrEmpty(data.savePath))
                {
                    EditorUtility.DisplayDialog("��ʾ", "��ѡ�����ɽű�·��!", "ȷ��");
                    return;
                }
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                var path = data.savePath + "/" + scene.name + ".mapData";
                MapData.WriteData(path);
                AssetDatabase.Refresh();
            }
        }
        void LoadData()
        {
            data = PersistHelper.Deserialize<Data>("buildMapData.json");
        }
        void SaveData()
        {
            PersistHelper.Serialize(data, "buildMapData.json");
        }
        internal class Data
        {
            public string savePath;
        }
    }
}
#endif