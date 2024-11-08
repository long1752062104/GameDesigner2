namespace Net.Table.Solvers
{
    public class UlongTypeSolver : TypeSolver<ulong>
    {
        public override ulong As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is ulong value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsUlong(text);
        }
    }
}