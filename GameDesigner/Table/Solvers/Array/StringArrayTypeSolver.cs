namespace Net.Table.Solvers
{
    public class StringArrayTypeSolver : ArrayTypeSolver<string>
    {
        public override string[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is string[] strings)
                return strings;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsString(item));
        }
    }
}