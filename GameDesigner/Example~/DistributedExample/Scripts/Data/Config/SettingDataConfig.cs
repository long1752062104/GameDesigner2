using Net.Share;
using System.Data;

public partial class SettingDataConfig : IDataConfig
{
    public int ID { get; set; }

    public void Init(DataRow row)
    {
        ID = ObjectConverter.AsInt(row["ID"]);

    }
}