namespace Net.Table.Solvers
{
    public class DoubleTypeSolver : TypeSolver<double>
    {
        public override double As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsDouble(text);
        }
    }
}