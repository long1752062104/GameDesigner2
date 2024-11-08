namespace Net.Table.Solvers
{
    public class IntTypeSolver : TypeSolver<int>
    {
        public override int As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is int value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsInt(text);
        }
    }
}