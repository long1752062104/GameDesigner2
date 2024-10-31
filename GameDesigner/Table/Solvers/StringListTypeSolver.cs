using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class StringListTypeSolver : ListTypeSolver<string>
    {
        public override List<string> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsString(item));
        }
    }
}