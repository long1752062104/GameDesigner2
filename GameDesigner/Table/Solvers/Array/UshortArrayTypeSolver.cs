namespace Net.Table.Solvers
{
    public class UshortArrayTypeSolver : ArrayTypeSolver<ushort>
    {
        public override ushort[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is ushort[] ushorts)
                return ushorts;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsUshort(item));
        }
    }
}