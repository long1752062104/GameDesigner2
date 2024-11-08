using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class IntListTypeSolver : ListTypeSolver<int>
    {
        public override List<int> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<int> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsInt(item));
        }
    }
}