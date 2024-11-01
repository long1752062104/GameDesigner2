namespace Net.Table.Solvers
{
    public class CharArrayTypeSolver : ArrayTypeSolver<char>
    {
        public override char[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsChar(item));
        }
    }
}