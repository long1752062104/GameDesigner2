namespace Net.Table.Solvers
{
    public class BoolTypeSolver : TypeSolver<bool>
    {
        public override bool As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsBool(text);
        }
    }
}