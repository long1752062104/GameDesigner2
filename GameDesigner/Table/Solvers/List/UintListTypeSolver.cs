using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class UintListTypeSolver : ListTypeSolver<uint>
    {
        public override List<uint> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<uint> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsUint(item));
        }
    }
}