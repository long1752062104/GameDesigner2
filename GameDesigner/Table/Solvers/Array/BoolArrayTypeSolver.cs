namespace Net.Table.Solvers
{
    public class BoolArrayTypeSolver : ArrayTypeSolver<bool>
    {
        public override bool[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is bool[] bools)
                return bools;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsBool(item));
        }
    }
}