namespace Net.Table.Solvers
{
    public class ShortArrayTypeSolver : ArrayTypeSolver<short>
    {
        public override short[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsShort(item));
        }
    }
}