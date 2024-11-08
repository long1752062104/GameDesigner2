namespace Net.Table.Solvers
{
    public class DecimalArrayTypeSolver : ArrayTypeSolver<decimal>
    {
        public override decimal[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is decimal[] decimals)
                return decimals;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsDecimal(item));
        }
    }
}