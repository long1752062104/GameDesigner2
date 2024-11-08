using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class ByteListTypeSolver : ListTypeSolver<byte>
    {
        public override List<byte> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<byte> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsByte(item));
        }
    }
}