using System.Collections.Generic;

namespace Net.Table
{
    public class DataTableInfo
    {
        public string TableName { get; set; }
        public List<DataColumnInfo> Columns { get; set; } = new List<DataColumnInfo>();
        public List<DataRowInfo> Rows { get; set; } = new List<DataRowInfo>();

        public DataTableInfo() { }
        
        public DataTableInfo(string tableName)
        {
            TableName = tableName;
        }

        public DataRowInfo NewRow()
        {
            return new DataRowInfo() { ItemArray = new object[Columns.Count] };
        }

        public void AcceptChanges()
        {
        }

        public override string ToString()
        {
            return $"TableName: {TableName}";
        }
    }
}