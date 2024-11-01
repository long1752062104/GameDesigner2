namespace Net.Table.Solvers
{
    public class BoolArrayTypeSolver : ArrayTypeSolver<bool>
    {
        public override bool[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsBool(item));
        }
    }
}