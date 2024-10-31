namespace Net.Table.Solvers
{
    public class ByteTypeSolver : TypeSolver<byte>
    {
        public override byte As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsByte(text);
        }
    }
}