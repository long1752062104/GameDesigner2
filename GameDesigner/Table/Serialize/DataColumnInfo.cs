using Net.Helper;
using Net.System;
using System;

namespace Net.Table
{
    public class DataColumnInfo
    {
        public string ColumnName { get; set; }
        public string DataTypeName { get; set; }
        private Type dataType;
        internal Type DataType
        {
            get
            {
                if (dataType == null)
                    dataType = AssemblyHelper.GetTypeNotOptimized(DataTypeName);
                return dataType;
            }
        }

        public DataColumnInfo() { }

        public DataColumnInfo(string columnName, Type dataType)
        {
            ColumnName = columnName;
            DataTypeName = dataType.ToString();
            this.dataType = dataType;
        }

        public override string ToString()
        {
            return $"ColumnName: {ColumnName} DataType: {DataTypeName}";
        }
    }
}