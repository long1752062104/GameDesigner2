namespace Net.Table.Solvers
{
    public class SbyteTypeSolver : TypeSolver<sbyte>
    {
        public override sbyte As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            return ObjectConverter.AsSbyte(text);
        }
    }
}