using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class LongListTypeSolver : ListTypeSolver<long>
    {
        public override List<long> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsLong(item));
        }
    }
}