using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class SbyteListTypeSolver : ListTypeSolver<sbyte>
    {
        public override List<sbyte> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsSbyte(item));
        }
    }
}