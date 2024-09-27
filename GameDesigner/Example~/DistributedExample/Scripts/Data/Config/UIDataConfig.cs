using Net.Share;
using System.Data;

public partial class UIDataConfig : IEntityDataConfig
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



    public object this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return Name;
                case 1: return Navigation;
                case 2: return Level;
                case 3: return Path;

                default: return null;
            }
        }
        set
        {
            switch (index)
            {
                case 0: Name = (string)value; break;
                case 1: Navigation = (bool)value; break;
                case 2: Level = (int)value; break;
                case 3: Path = (string)value; break;

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
                case "Navigation": return Navigation;
                case "Level": return Level;
                case "Path": return Path;

                default: return null;
            }
        }
        set
        {
            switch (columnName)
            {
                case "Name": Name = (string)value; break;
                case "Navigation": Navigation = (bool)value; break;
                case "Level": Level = (int)value; break;
                case "Path": Path = (string)value; break;

            }
        }
    }

    public void Init(DataRow row)
    {
        ID = ObjectConverter.AsInt(row["ID"]);
        Name = ObjectConverter.AsString(row["Name"]);
        Navigation = ObjectConverter.AsBool(row["Navigation"]);
        Level = ObjectConverter.AsInt(row["Level"]);
        Path = ObjectConverter.AsString(row["Path"]);

    }
}