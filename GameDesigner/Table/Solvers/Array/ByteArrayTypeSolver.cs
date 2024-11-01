namespace Net.Table.Solvers
{
    public class ByteArrayTypeSolver : ArrayTypeSolver<byte>
    {
        public override byte[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsByte(item));
        }
    }
}