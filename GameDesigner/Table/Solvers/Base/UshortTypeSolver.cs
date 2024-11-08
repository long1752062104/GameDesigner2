namespace Net.Table.Solvers
{
    public class UshortTypeSolver : TypeSolver<ushort>
    {
        public override ushort As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is ushort value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsUshort(text);
        }
    }
}