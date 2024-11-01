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
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsDateTime(item));
        }
    }
}
