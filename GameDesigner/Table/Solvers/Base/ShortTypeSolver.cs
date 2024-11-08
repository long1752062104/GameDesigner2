namespace Net.Table.Solvers
{
    public class ShortTypeSolver : TypeSolver<short>
    {
        public override short As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is short value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsShort(text);
        }
    }
}