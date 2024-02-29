using Net.Share;
using System.Data;

public partial class UIDataConfig : IDataConfig
{
    public int ID { get; set; }
    /// <summary>UI名称</summary>
    public string Name { get; set; }

    /// <summary>是否导航</summary>
    public bool Navigation { get; set; }

    /// <summary>UI层级</summary>
    public int Level { get; set; }

    /// <summary>资源路径</summary>
    public string Path { get; set; }


    public void Init(DataRow row)
    {
        ID = ObjectConverter.AsInt(row["ID"]);
        Name = ObjectConverter.AsString(row["Name"]);
        Navigation = ObjectConverter.AsBool(row["Navigation"]);
        Level = ObjectConverter.AsInt(row["Level"]);
        Path = ObjectConverter.AsString(row["Path"]);

    }
}