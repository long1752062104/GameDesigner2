namespace Net.Table.Solvers
{
    public class DoubleArrayTypeSolver : ArrayTypeSolver<double>
    {
        public override double[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsDouble(item));
        }
    }
}