namespace Net.Table.Solvers
{
    public class StringTypeSolver : TypeSolver<string>
    {
        public override string As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is string value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsString(text);
        }
    }
}