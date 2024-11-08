using Net.Serialize;

namespace Net.Table
{
    public class DataRowInfo
    {
        [Serialized]
        public object[] ItemArray;

        public object this[int columnIndex]
        {
            get
            {
                return ItemArray[columnIndex];
            }
            set
            {
                ItemArray[columnIndex] = value;
            }
        }

        public DataRowInfo() { }
    }
}