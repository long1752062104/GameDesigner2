using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class DecimalListTypeSolver : ListTypeSolver<decimal>
    {
        public override List<decimal> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<decimal> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsDecimal(item));
        }
    }
}