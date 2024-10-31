namespace Net.Table.Solvers
{
    public class SbyteArrayTypeSolver : ArrayTypeSolver<sbyte>
    {
        public override sbyte[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsSbyte(item));
        }
    }
}