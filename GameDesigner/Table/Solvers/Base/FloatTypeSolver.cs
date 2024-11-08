namespace Net.Table.Solvers
{
    public class FloatTypeSolver : TypeSolver<float>
    {
        public override float As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is float value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsFloat(text);
        }
    }
}