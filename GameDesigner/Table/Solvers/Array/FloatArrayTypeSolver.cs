namespace Net.Table.Solvers
{
    public class FloatArrayTypeSolver : ArrayTypeSolver<float>
    {
        public override float[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsFloat(item));
        }
    }
}