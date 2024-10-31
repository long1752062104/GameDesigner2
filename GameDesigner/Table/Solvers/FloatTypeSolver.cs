namespace Net.Table.Solvers
{
    public class FloatTypeSolver : TypeSolver<float>
    {
        public override float As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsFloat(text);
        }
    }
}