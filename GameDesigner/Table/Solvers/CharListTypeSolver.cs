using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class CharListTypeSolver : ListTypeSolver<char>
    {
        public override List<char> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsChar(item));
        }
    }
}