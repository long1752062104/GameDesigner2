namespace Net.Table.Solvers
{
    public class UlongTypeSolver : TypeSolver<ulong>
    {
        public override ulong As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsUlong(text);
        }
    }
}