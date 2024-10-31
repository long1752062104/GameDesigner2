namespace Net.Table.Solvers
{
    public class IntArrayTypeSolver : ArrayTypeSolver<int>
    {
        public override int[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsInt(item));
        }
    }
}