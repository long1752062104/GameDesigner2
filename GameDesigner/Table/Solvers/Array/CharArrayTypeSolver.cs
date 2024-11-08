namespace Net.Table.Solvers
{
    public class CharArrayTypeSolver : ArrayTypeSolver<char>
    {
        public override char[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is char[] chars)
                return chars;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsChar(item));
        }
    }
}