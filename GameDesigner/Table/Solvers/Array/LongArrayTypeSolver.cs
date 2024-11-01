namespace Net.Table.Solvers
{
    public class LongArrayTypeSolver : ArrayTypeSolver<long>
    {
        public override long[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsLong(item));
        }
    }
}