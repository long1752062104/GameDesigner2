using Net.Share;
using System.Data;

public partial class SceneDataConfig : IDataConfig
{
    public int ID { get; set; }
    /// <summary>场景名称</summary>
    public string Name { get; set; }

    /// <summary>场景索引</summary>
    public int Index { get; set; }

    /// <summary>场景路径</summary>
    public string Path { get; set; }


    public void Init(DataRow row)
    {
        ID = ObjectConverter.AsInt(row["ID"]);
        Name = ObjectConverter.AsString(row["Name"]);
        Index = ObjectConverter.AsInt(row["Index"]);
        Path = ObjectConverter.AsString(row["Path"]);

    }
}