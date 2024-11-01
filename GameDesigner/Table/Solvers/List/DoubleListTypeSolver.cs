using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class DoubleListTypeSolver : ListTypeSolver<double>
    {
        public override List<double> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsDouble(item));
        }
    }
}