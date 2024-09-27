using Net.Share;
using System.Data;

public partial class SceneDataConfig : IEntityDataConfig
{
    public int ID { get; set; }

    /// <summary>场景名称</summary>
    public string Name { get; set; }

    /// <summary>场景索引</summary>
    public int Index { get; set; }

    /// <summary>场景路径</summary>
    public string Path { get; set; }



    public object this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return Name;
                case 1: return Index;
                case 2: return Path;

                default: return null;
            }
        }
        set
        {
            switch (index)
            {
                case 0: Name = (string)value; break;
                case 1: Index = (int)value; break;
                case 2: Path = (string)value; break;

            }
        }
    }

    public object this[string columnName] 
    {
        get
        {
            switch (columnName)
            {
                case "Name": return Name;
                case "Index": return Index;
                case "Path": return Path;

                default: return null;
            }
        }
        set
        {
            switch (columnName)
            {
                case "Name": Name = (string)value; break;
                case "Index": Index = (int)value; break;
                case "Path": Path = (string)value; break;

            }
        }
    }

    public void Init(DataRow row)
    {
        ID = ObjectConverter.AsInt(row["ID"]);
        Name = ObjectConverter.AsString(row["Name"]);
        Index = ObjectConverter.AsInt(row["Index"]);
        Path = ObjectConverter.AsString(row["Path"]);

    }
}