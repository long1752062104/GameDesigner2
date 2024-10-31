namespace Net.Table.Solvers
{
    public class DecimalArrayTypeSolver : ArrayTypeSolver<decimal>
    {
        public override decimal[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsDecimal(item));
        }
    }
}