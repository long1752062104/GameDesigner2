namespace Net.Table.Solvers
{
    public class ByteTypeSolver : TypeSolver<byte>
    {
        public override byte As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is byte value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsByte(text);
        }
    }
}