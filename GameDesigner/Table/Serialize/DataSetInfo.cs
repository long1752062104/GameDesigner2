using System.Collections.Generic;

namespace Net.Table
{
    public class DataSetInfo
    {
        public int Version { get; set; }
        public List<DataTableInfo> Tables { get; set; } = new List<DataTableInfo>();

        public DataSetInfo() { }

        public void AcceptChanges()
        {
        }
    }
}