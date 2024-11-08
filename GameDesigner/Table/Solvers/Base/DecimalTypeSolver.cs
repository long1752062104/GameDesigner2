namespace Net.Table.Solvers
{
    public class DecimalTypeSolver : TypeSolver<decimal>
    {
        public override decimal As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is decimal value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsDecimal(text);
        }
    }
}