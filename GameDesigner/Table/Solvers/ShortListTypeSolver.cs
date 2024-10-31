using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class ShortListTypeSolver : ListTypeSolver<short>
    {
        public override List<short> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsShort(item));
        }
    }
}