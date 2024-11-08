namespace Net.Table.Solvers
{
    public class DoubleTypeSolver : TypeSolver<double>
    {
        public override double As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is double value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsDouble(text);
        }
    }
}