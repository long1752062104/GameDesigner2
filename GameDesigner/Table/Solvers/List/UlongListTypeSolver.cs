using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class UlongListTypeSolver : ListTypeSolver<ulong>
    {
        public override List<ulong> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<ulong> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsUlong(item));
        }
    }
}