using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class DoubleListTypeSolver : ListTypeSolver<double>
    {
        public override List<double> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<double> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsDouble(item));
        }
    }
}