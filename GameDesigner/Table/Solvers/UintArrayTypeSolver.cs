namespace Net.Table.Solvers
{
    public class UintArrayTypeSolver : ArrayTypeSolver<uint>
    {
        public override uint[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsUint(item));
        }
    }
}