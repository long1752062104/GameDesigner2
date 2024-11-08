namespace Net.Table.Solvers
{
    public class IntArrayTypeSolver : ArrayTypeSolver<int>
    {
        public override int[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is int[] ints)
                return ints;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsInt(item));
        }
    }
}