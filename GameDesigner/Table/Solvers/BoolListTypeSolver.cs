using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class BoolListTypeSolver : ListTypeSolver<bool>
    {
        public override List<bool> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsBool(item));
        }
    }
}