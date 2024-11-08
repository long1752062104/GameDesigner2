using System;
using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class DateTimeListTypeSolver : ListTypeSolver<DateTime>
    {
        public override List<DateTime> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is List<DateTime> list)
                return list;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsDateTime(item));
        }
    }
}
