using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class LongListTypeSolver : ListTypeSolver<long>
    {
        public override List<long> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<long> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsLong(item));
        }
    }
}