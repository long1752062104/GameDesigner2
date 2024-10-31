using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class ByteListTypeSolver : ListTypeSolver<byte>
    {
        public override List<byte> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsByte(item));
        }
    }
}