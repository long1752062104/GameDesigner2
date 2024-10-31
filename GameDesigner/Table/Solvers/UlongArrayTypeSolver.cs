namespace Net.Table.Solvers
{
    public class UlongArrayTypeSolver : ArrayTypeSolver<ulong>
    {
        public override ulong[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsUlong(item));
        }
    }
}