namespace Net.Table.Solvers
{
    public class IntTypeSolver : TypeSolver<int>
    {
        public override int As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsInt(text);
        }
    }
}