using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class SbyteListTypeSolver : ListTypeSolver<sbyte>
    {
        public override List<sbyte> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<sbyte> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsSbyte(item));
        }
    }
}