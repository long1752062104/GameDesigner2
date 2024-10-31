namespace Net.Table.Solvers
{
    public class CharTypeSolver : TypeSolver<char>
    {
        public override char As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsChar(text);
        }
    }
}