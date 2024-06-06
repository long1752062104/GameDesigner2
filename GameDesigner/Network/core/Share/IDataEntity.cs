using System;

namespace Net.Share
{
    public interface IDataEntity : IDataRow
    {
#if SERVICE
        void Update(bool immediately = false);
        string GetCellNameAndTextLength(int index, out uint length);
#endif
    }

    public class DataRowField : Attribute
    {
        public string name;
        public int index;

        public DataRowField(string name, int index)
        {
            this.name = name;
            this.index = index;
        }
    }
}