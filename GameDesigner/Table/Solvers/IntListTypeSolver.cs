using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class IntListTypeSolver : ListTypeSolver<int>
    {
        public override List<int> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsInt(item));
        }
    }
}