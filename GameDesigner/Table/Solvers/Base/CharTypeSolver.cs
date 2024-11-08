namespace Net.Table.Solvers
{
    public class CharTypeSolver : TypeSolver<char>
    {
        public override char As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is char value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsChar(text);
        }
    }
}