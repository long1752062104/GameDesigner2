namespace Net.Table.Solvers
{
    public class UintTypeSolver : TypeSolver<uint>
    {
        public override uint As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is uint value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsUint(text);
        }
    }
}