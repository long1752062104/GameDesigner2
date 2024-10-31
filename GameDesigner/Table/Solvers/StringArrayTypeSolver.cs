namespace Net.Table.Solvers
{
    public class StringArrayTypeSolver : ArrayTypeSolver<string>
    {
        public override string[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsString(item));
        }
    }
}