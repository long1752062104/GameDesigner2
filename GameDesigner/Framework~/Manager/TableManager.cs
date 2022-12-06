using System.Data;
using System.IO;
using UnityEngine;

namespace Framework
{
    public class TableManager : MonoBehaviour
    {
        private DataSet dataSet;

        internal void Init()
        {
            string path;
            if (Global.Resources.Mode == AssetBundleMode.LocalPath)
                path = Application.streamingAssetsPath + $"/AssetBundles/Table/GameData.json";
            else
                path = Application.persistentDataPath + $"/AssetBundles/Table/GameData.json";
            if (File.Exists(path)) 
            {
                var jsonStr = File.ReadAllText(path);
                dataSet = Newtonsoft_X.Json.JsonConvert.DeserializeObject<DataSet>(jsonStr);
                foreach (DataTable table in dataSet.Tables)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        table.Columns[i].ColumnName = table.Rows[0][i].ToString();
                    }
                }
            }
        }

        public DataTable GetTable(string sheetName)
        {
            return dataSet.Tables[sheetName];
        }
    }
}