using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class FloatListTypeSolver : ListTypeSolver<float>
    {
        public override List<float> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<float> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsFloat(item));
        }
    }
}