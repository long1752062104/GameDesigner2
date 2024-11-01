namespace Net.Table.Solvers
{
    public class DecimalTypeSolver : TypeSolver<decimal>
    {
        public override decimal As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsDecimal(text);
        }
    }
}