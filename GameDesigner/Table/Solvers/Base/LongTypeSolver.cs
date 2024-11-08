namespace Net.Table.Solvers
{
    public class LongTypeSolver : TypeSolver<long>
    {
        public override long As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is long value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsLong(text);
        }
    }
}