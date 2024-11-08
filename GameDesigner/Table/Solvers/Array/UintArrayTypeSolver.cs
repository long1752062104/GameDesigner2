namespace Net.Table.Solvers
{
    public class UintArrayTypeSolver : ArrayTypeSolver<uint>
    {
        public override uint[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is uint[] uints)
                return uints;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsUint(item));
        }
    }
}