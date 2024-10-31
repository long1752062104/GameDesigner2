namespace Net.Table.Solvers
{
    public class UshortTypeSolver : TypeSolver<ushort>
    {
        public override ushort As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsUshort(text);
        }
    }
}